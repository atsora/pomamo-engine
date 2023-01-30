// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason.Action;
using System.Linq;

namespace Lemoine.Plugin.AutoReasonToolChange
{
  /// <summary>
  /// Detect a tool change
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // ID of the latest tool life event corresponding to a current tool life reset
    static readonly string LAST_TOOL_CHANGED_EVENT = "LastToolChangedEvent";

    static readonly TimeSpan DEFAULT_RIGHT_MARGIN_DURATION = TimeSpan.FromMinutes (10);

    int? m_lastToolChangedEventId = null;
    int[] m_machineModuleIds;
    EventToolLifeType[] m_trackedEventToolLifeTypes = new[] { EventToolLifeType.CurrentLifeReset, EventToolLifeType.RestLifeReset };
    TimeSpan m_rightMarginDuration;
    Configuration m_configuration;

    /// <summary>
    /// Set it to true for unit test
    /// False by default
    /// </summary>
    public bool TestMode { get; set; } = false;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.ToolChange")
    {
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager.GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_configuration = configuration;

      if (0 == configuration.RightMargin.Ticks) {
        m_rightMarginDuration = DEFAULT_RIGHT_MARGIN_DURATION;
      }
      else {
        m_rightMarginDuration = configuration.RightMargin;
      }

      // Last tool changed event id
      var lastToolChangedEventState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (LAST_TOOL_CHANGED_EVENT));
      if (lastToolChangedEventState != null) {
        m_lastToolChangedEventId = (int)lastToolChangedEventState.Value;
      }

      // Get the machine modules ids
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (this.Machine.Id);
        m_machineModuleIds = machine.MachineModules
          .Select (x => x.Id)
          .ToArray ();
      }
      if (!m_machineModuleIds.Any ()) {
        log.Error ($"InitializeAdditionalConfigurations: no machine module is associated to machine {this.Machine.Id} => return false");
        return false;
      }

      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      if (m_machineModuleIds.Length == 0) {
        log.Fatal ("no machine module ids");
        return;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (m_lastToolChangedEventId == null) {
          // Initialize the last tool change event id
          if (log.IsDebugEnabled) {
            log.Debug ("Check: get last tool life event to initialize lastToolChangedEventId");
          }
          using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.ToolChange.UpdateLastToolChangedEventId")) {
            var lastEvent = ModelDAOHelper.DAOFactory.EventToolLifeDAO.GetLast (m_machineModuleIds);
            if (lastEvent != null) {
              AddUpdateLastToolChangedEventIdAction (lastEvent.Id);
              if (log.IsDebugEnabled) {
                log.Debug ($"Check: got last tool life event {lastEvent.Id}");
              }
            }
            transaction.Commit ();
          }
        }
        else {
          CheckAutoReason ();
        }
      }
    }

    void CheckAutoReason ()
    {
      Debug.Assert (m_lastToolChangedEventId.HasValue);

      if (log.IsDebugEnabled) {
        log.Debug ($"CheckAutoReason: lastToolChangedEventId={m_lastToolChangedEventId}");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        IEventToolLife nextEvent;
        string toolNumber;
        DateTime dateTime;
        DateTime? minDateTime = null;

        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.ToolChange.Check")) {
          nextEvent = ModelDAOHelper.DAOFactory.EventToolLifeDAO.FindNextEventByType (
  m_machineModuleIds, m_trackedEventToolLifeTypes, m_lastToolChangedEventId.Value);
          if (null == nextEvent) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckAutoReason: no tool life event after {m_lastToolChangedEventId.Value}");
            }
            return;
          }

          dateTime = nextEvent.DateTime;
          var toolId = nextEvent.ToolId;
          toolNumber = "T" + nextEvent.ToolNumber;

          // Left limit
          if (!string.IsNullOrEmpty (toolId)) {
            // Search the last expiration event occurring in the previous 24 hours
            IList<IEventToolLife> expirationEvents = ModelDAOHelper.DAOFactory.EventToolLifeDAO.FindEventsByType (
              m_machineModuleIds, EventToolLifeType.ExpirationReached, new UtcDateTimeRange (dateTime.AddHours (-24), dateTime), toolId);
            minDateTime = GetMaxDateTime (expirationEvents);
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckAutoReason: min date/time from last expiration events is {minDateTime}");
            }
          }

          // Find the last production end
          if (!TestMode) {
            var limit = new UtcDateTimeRange ("(,)");
            if (minDateTime.HasValue) {
              limit = new UtcDateTimeRange (minDateTime.Value);
            }
            IDynamicTimeResponse dynamicStartResponse =
              Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime ("LastProductionEnd", this.Machine, dateTime, new UtcDateTimeRange ("(,)"), limit);
            Debug.Assert (null != dynamicStartResponse);
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckAutoReason: response for LastProductionEnd dateTime={dateTime} is {dynamicStartResponse}");
            }
            if (dynamicStartResponse.NotApplicable) {
              // Not in production continue with existing minDateTime
            }
            else if (dynamicStartResponse.NoData) {
              // Not found in limit, we keep the existing minDateTime
            }
            else if (dynamicStartResponse.Final.HasValue) {
              // A last production end is found before the limit
              if (log.IsDebugEnabled) {
                log.Debug ($"CheckAutoReason: final for LastProductionEnd dateTime={dateTime} is {dynamicStartResponse}");
              }
              if (!minDateTime.HasValue || (minDateTime.Value < dynamicStartResponse.Final.Value)) {
                minDateTime = dynamicStartResponse.Final.Value;
              }
              Debug.Assert (minDateTime.HasValue);
            }
            else {
              // TODO: record the hint to optimize the process
              return; // Wait a bit (not processed yet)
            }
          }
        }

        var (dynamic, range) = GetDynamicRange (minDateTime, dateTime);
        var details = toolNumber;
        AddUpdateLastToolChangedEventIdAction (nextEvent.Id);
        if (range.IsEmpty ()) {
          log.Warn ("CheckAutoReason: empty range");
        }
        else {
          AddReason (range, dynamic, details);
        }
      } // session
    }

    (string, UtcDateTimeRange) GetDynamicRange (DateTime? minDateTime, DateTime toolChangeDateTime)
    {
      if (!minDateTime.HasValue && log.IsWarnEnabled) {
        log.Warn ($"GetDynamicRange: no min date/time");
      }

      var dynamic = string.IsNullOrEmpty (m_configuration.DynamicEnd)
        ? ""
        : "," + m_configuration.DynamicEnd;
      var upper = dynamic.EndsWith ("+")
        ? toolChangeDateTime
        : toolChangeDateTime.Add (m_rightMarginDuration);
      var lower = minDateTime ?? toolChangeDateTime;
      if (DateTime.Equals (lower, upper)) {
        upper = toolChangeDateTime.AddSeconds (1);
      }
      var range = new UtcDateTimeRange (lower, upper);
      return (dynamic, range);
    }

    DateTime? GetMaxDateTime (IList<IEventToolLife> etls)
    {
      if (!etls.Any ()) {
        return null;
      }

      return etls.Max (x => x.DateTime);
    }

    void AddUpdateLastToolChangedEventIdAction (int lastToolChangedEventId)
    {
      var action = new UpdateLastToolChangedEventIdAction (this, lastToolChangedEventId);
      AddDelayedAction (action);
    }

    internal int? GetLastToolChangedEventId ()
    {
      return m_lastToolChangedEventId;
    }

    internal void UpdateLastToolChangedEventId (int lastToolChangedEventId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"UpdateLastToolChangedEventId: with {lastToolChangedEventId}");
      }
      m_lastToolChangedEventId = lastToolChangedEventId;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (LAST_TOOL_CHANGED_EVENT), lastToolChangedEventId);
    }

    internal void ResetLastToolChangedEventId (int? lastToolChangedEventId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"ResetLastToolChangedEventId: with {lastToolChangedEventId}");
      }
      m_lastToolChangedEventId = lastToolChangedEventId;
    }

    void AddReason (UtcDateTimeRange range, string dynamic, string details)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"AddReason: range={range} dynamic={dynamic} details={details}");
      }
      var action = new ApplyReasonAction (this, range, dynamic, details);
      AddDelayedAction (action);
    }
  }
}