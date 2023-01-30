// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonShiftStart
{
  /// <summary>
  /// Detect idle periods at the beginning of a shift and apply an auto-reason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // ID of the previous shift
    static readonly string PREVIOUS_SHIFT_ID = "PreviousShiftID";
    int m_previousShiftId = -2; // -2 is not initialized, -1 will be "no shift"
    TimeSpan m_maxDuration;
    TimeSpan m_detectionMargin;

    /// <summary>
    /// Set it to true for unit test
    /// False by default
    /// </summary>
    public bool TestMode { get; set; }

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.ShiftStart")
    {
      TestMode = false;
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager.GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      // Previous shift id
      var previousShiftId = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (PREVIOUS_SHIFT_ID));
      m_previousShiftId = (previousShiftId != null) ? (int)previousShiftId.Value : -1;

      m_maxDuration = configuration.MaxDuration;
      m_detectionMargin = configuration.DetectionMargin;

      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ($"AutoReason.ShiftStart.Check.{this.Machine.Id}")) {

          // Find an observation state slot after the current date time
          DateTime maxForDetection = DateTime.UtcNow;
          var nextOSSs = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindFirstOverlapsRange (
            this.Machine, new UtcDateTimeRange (this.DateTime.AddSeconds (1), maxForDetection, true, true), 1, false);

          if (nextOSSs != null && nextOSSs.Any ()) {
            IObservationStateSlot nextOSS = nextOSSs.First ();

            // Is there a beginning of a slot?
            if (nextOSS.BeginDateTime.HasValue && nextOSS.BeginDateTime.Value > this.DateTime && nextOSS.BeginDateTime.Value <= maxForDetection) {
              int newShiftId = (nextOSS.Shift != null) ? nextOSS.Shift.Id : -1;
              bool canGoOn = true;
              if (m_previousShiftId != -2 && m_previousShiftId != newShiftId) {
                canGoOn = CreateAutoReason (nextOSS.BeginDateTime.Value);
              }

              // Datetime will be the beginning of the OSS
              if (canGoOn) { // Otherwise we wait
                this.AddUpdatePreviousShiftIdAction (newShiftId);
                GoOn (nextOSS.BeginDateTime.Value);
              }
            }
            else if (nextOSS.EndDateTime.HasValue && nextOSS.EndDateTime.Value > this.DateTime && nextOSS.EndDateTime.Value <= maxForDetection) {
              // Datetime will be the end of the OSS
              GoOn (nextOSS.EndDateTime.Value.AddSeconds (-1));
            }
            else {
              // Datetime will be the end of the studied range
              GoOn (maxForDetection);
            }
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      this.AddUpdateDateTimeDelayedAction (dateTime);
    }

    void AddUpdatePreviousShiftIdAction (int shiftId)
    {
      var action = new UpdatePreviousShiftIdAction (this, shiftId);
      AddDelayedAction (action);
    }

    internal int GetPreviousShiftId ()
    {
      return m_previousShiftId;
    }

    internal void UpdatePreviousShiftId (int id)
    {
      m_previousShiftId = id;
      var key = GetKey (PREVIOUS_SHIFT_ID);
      log.Info ($"UpdatePreviousShiftId: updated {key}={m_previousShiftId}");
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, key, m_previousShiftId);
    }

    internal void ResetPreviousShiftId (int id)
    {
      m_previousShiftId = id;
    }

    bool CreateAutoReason (DateTime dateTime)
    {
      if (TestMode) {
        // Bouchon
        StartReason (dateTime);
        return true;
      }

      // Find the next production end
      UtcDateTimeRange limit = new UtcDateTimeRange (dateTime, dateTime.Add (m_detectionMargin), "[]");
      IDynamicTimeResponse dynamicStartResponse = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime ("NextProductionEnd", this.Machine, dateTime, new UtcDateTimeRange ("(,)"), limit);
      Debug.Assert (null != dynamicStartResponse);

      if (dynamicStartResponse.NotApplicable) {
        StartReason (dateTime);
        log.Info ($"CreateAutoReason: next production end not applicable at {dateTime}");
      }
      else if (dynamicStartResponse.NoData) {
        log.Info ($"CreateAutoReason: next production end not found in {limit}");
      }
      else if (dynamicStartResponse.Final.HasValue) {
        StartReason (dynamicStartResponse.Final.Value);
        log.Info ($"CreateAutoReason: next production end at {dynamicStartResponse.Final.Value} for {dateTime}");
      }
      else {
        log.Info ($"CreateAutoReason: next production end at {dateTime} not known yet => wait");
        return false;
      }

      return true;
    }

    void StartReason (DateTime start)
    {
      var range = new UtcDateTimeRange (start, start.Add (m_maxDuration));
      var action = new ApplyReasonAction (this, range, ",NextProductionStart");
      AddDelayedAction (action);
    }
  }
}
