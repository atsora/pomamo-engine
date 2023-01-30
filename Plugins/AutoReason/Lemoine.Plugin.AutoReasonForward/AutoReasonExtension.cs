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
using Lemoine.Extensions.AutoReason.Action;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Collections;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonForward
{
  /// <summary>
  /// Detect short acquisition errors and remove them
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    static readonly string LAST_MODIFICATION_ID_KEY = "LastModificationId";
    static readonly string DYNAMIC_END_IS_SOURCE = "Source";

    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    Configuration m_configuration;
    IEnumerable<int> m_sourceReasonIds;
    IMachine m_targetMachine;
    string m_dynamicEnd;
    string m_dynamicStart;
    bool m_dynamicEndIsSource;
    long m_lastModificationId;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension ()
      : base ("AutoReason.Forward")
    {
    }

    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      m_configuration = configuration;

      if ((null != configuration.SourceReasonIds) && configuration.SourceReasonIds.Any ()) {
        m_sourceReasonIds = configuration.SourceReasonIds;
      }
      else {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"InitializeAdditionalConfigurations: use default {this.Reason.Id} for source reason id");
        }
        m_sourceReasonIds = new List<int> { this.Reason.Id };
      }

      if ((0 == configuration.TargetMachineId) && ((null == configuration.SourceReasonIds) || !configuration.SourceReasonIds.Any (x => this.Reason.Id != x))) {
        GetLogger ().Error ("InitializeAdditionalConfigurations: the target machine is not set and no source reason is set");
        return false;
      }

      if (0 == configuration.TargetMachineId) {
        m_targetMachine = this.Machine;
      }
      else {
        m_targetMachine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (configuration.TargetMachineId);
        if (null == m_targetMachine) {
          GetLogger ().Error ($"InitializeAdditionalConfigurations: target machine {configuration.TargetMachineId} does not exist");
          return false;
        }
      }
      Debug.Assert (null != m_targetMachine);

      m_dynamicEnd = configuration.DynamicEnd;
      m_dynamicEndIsSource = m_dynamicEnd.Equals (DYNAMIC_END_IS_SOURCE);

      m_dynamicStart = configuration.DynamicStart;

      var lastModificationIdState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (LAST_MODIFICATION_ID_KEY));
      if (null != lastModificationIdState) {
        m_lastModificationId = (long)lastModificationIdState.Value;
      }
      else {
        var maxModificationId = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .GetMaxModificationId (this.Machine);
        if (maxModificationId.HasValue) {
          m_lastModificationId = maxModificationId.Value;
          { // Set the auto-reason state right now to make the process faster the next time
            AddUpdateLastModificationIdAction (maxModificationId.Value);
            this.ProcessPendingActions ();
          }
        }
        else { // !maxModificationId.HasValue
          if (GetLogger ().IsWarnEnabled) {
            GetLogger ().Warn ($"InitializeAdditionalConfigurations: no modification for this machine yet");
          }
          m_lastModificationId = 0;
        }
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().Info ($"InitializeAdditionalConfigurations: first run, set m_lastModificationId {m_lastModificationId}");
        }
      }

      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var nextModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .GetNextAncestorAuto (this.Machine, m_lastModificationId);
        if (null == nextModification) {
          return;
        }
        Debug.Assert (null != nextModification.Reason);
        Debug.Assert (null != this.Reason);
        if (!m_sourceReasonIds.Contains (nextModification.Reason.Id)) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"Check: next reason machine association with reason {nextModification.Reason.Id} does not match configured reason {this.Reason.Id}");
          }
          AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
          return;
        }

        if (!string.IsNullOrEmpty (m_dynamicStart)) {
          if (!nextModification.Range.Lower.HasValue) {
            if (GetLogger ().IsErrorEnabled) {
              GetLogger ().Error ($"Check: modification with id {((IDataWithId<long>)nextModification).Id} with no range lower value");
            }
            AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
            return;
          }
          var dynamicStartResponse = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime (m_dynamicStart, m_targetMachine, nextModification.Range.Lower.Value);
          if (dynamicStartResponse.Timeout) {
            if (GetLogger ().IsErrorEnabled) {
              GetLogger ().Error ($"Check: timeout for dynamic start time {m_dynamicStart} at {nextModification.Range.Lower.Value}");
            }
            AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
            return;
          }
          else if (dynamicStartResponse.NotApplicable) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"Check: not application for dynamic start time {m_dynamicStart} at {nextModification.Range.Lower.Value}");
            }
            AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
            return;
          }
          else if (dynamicStartResponse.NoData) {
            if (GetLogger ().IsInfoEnabled) {
              GetLogger ().Info ($"Check: no data for dynamic start time {m_dynamicStart} at {nextModification.Range.Lower.Value}");
            }
            AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
            return;
          }
          else if (dynamicStartResponse.Final.HasValue) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: final is {dynamicStartResponse.Final.Value} for dynamic start time {m_dynamicStart} at {nextModification.Range.Lower.Value}");
            }
            AddReason (dynamicStartResponse.Final.Value, nextModification);
          }
          else { // Pending
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: pending for dynamic start time {m_dynamicStart} at {nextModification.Range.Lower.Value}");
            }
            return;
          }
        }
        else { // string.IsNullOrEmpty (m_dynamicStart) // No Dynamic start
          AddReason (nextModification);
        }
        AddUpdateLastModificationIdAction (((IDataWithId<long>)nextModification).Id);
      }
    }

    void AddUpdateLastModificationIdAction (long lastModificationId)
    {
      var action = new UpdateLastModificationIdAction (this, lastModificationId);
      AddDelayedAction (action);
    }

    internal long GetLastModificationId ()
    {
      return m_lastModificationId;
    }

    /// <summary>
    /// Update the last modification id in autoreasonstate
    /// </summary>
    /// <param name="lastModificationId"></param>
    internal void UpdateLastModificationId (long lastModificationId)
    {
      m_lastModificationId = lastModificationId;
      var lastModificationIdState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (this.Machine, GetKey (LAST_MODIFICATION_ID_KEY), lastModificationId);
    }

    internal void ResetLastModificationId (long lastModificationId)
    {
      m_lastModificationId = lastModificationId;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    string GetDynamicEnd (IReasonMachineAssociation nextModification)
    {
      var dynamicEnd = m_dynamicEndIsSource
        ? nextModification.DynamicEnd
        : m_dynamicEnd;
      if (!string.IsNullOrEmpty (dynamicEnd)) {
        return "," + dynamicEnd;
      }
      else {
        return "";
      }
    }

    (string, UtcDateTimeRange) GetDynamicRange (IReasonMachineAssociation nextModification)
    {
      var dynamic = GetDynamicEnd (nextModification);
      UtcDateTimeRange range;
      if (!m_configuration.StartOnly || string.IsNullOrEmpty (dynamic)) {
        range = nextModification.Range;
      }
      else {
        range = new UtcDateTimeRange (nextModification.Range.Lower);
      }
      return (dynamic, range);
    }

    (string, UtcDateTimeRange) GetDynamicRange (DateTime start, IReasonMachineAssociation nextModification)
    {
      var dynamic = GetDynamicEnd (nextModification);
      UtcDateTimeRange range;
      if (!m_configuration.StartOnly || string.IsNullOrEmpty (dynamic)) {
        range = new UtcDateTimeRange (start, nextModification.Range.Upper);
      }
      else {
        range = new UtcDateTimeRange (start);
      }
      return (dynamic, range);
    }

    void AddReason (DateTime start, IReasonMachineAssociation nextModification)
    {
      if (nextModification.Option.HasValue
  && nextModification.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
        AddReasonDynamicEndBeforeRealEnd (start, nextModification);
      }
      else {
        AddReasonNoDynamicEndBeforeRealEnd (start, nextModification);
      }
    }

    void AddReasonNoDynamicEndBeforeRealEnd (DateTime start, IReasonMachineAssociation nextModification)
    {
      var (dynamic, range) = GetDynamicRange (start, nextModification);
      var action = new ApplyReasonAction (this, m_targetMachine, range, dynamic, nextModification.ReasonDetails, nextModification.OverwriteRequired);
      AddDelayedAction (action);
    }

    void AddReasonDynamicEndBeforeRealEnd (DateTime start, IReasonMachineAssociation nextModification)
    {
      var (dynamic, range) = GetDynamicRange (start, nextModification);
      var action = new ApplyReasonDynamicEndBeforeRealEndAction (this, m_targetMachine, range, dynamic, nextModification.ReasonDetails, nextModification.OverwriteRequired);
      AddDelayedAction (action);
    }

    void AddReason (IReasonMachineAssociation nextModification)
    {
      if (nextModification.Option.HasValue
  && nextModification.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
        AddReasonDynamicEndBeforeRealEnd (nextModification);
      }
      else {
        AddReasonNoDynamicEndBeforeRealEnd (nextModification);
      }
    }

    void AddReasonNoDynamicEndBeforeRealEnd (IReasonMachineAssociation nextModification)
    {
      var (dynamic, range) = GetDynamicRange (nextModification);
      var action = new ApplyReasonAction (this, m_targetMachine, range, dynamic, nextModification.ReasonDetails, nextModification.OverwriteRequired);
      AddDelayedAction (action);
    }

    void AddReasonDynamicEndBeforeRealEnd (IReasonMachineAssociation nextModification)
    {
      var (dynamic, range) = GetDynamicRange (nextModification);
      var action = new ApplyReasonDynamicEndBeforeRealEndAction (this, m_targetMachine, range, dynamic, nextModification.ReasonDetails, nextModification.OverwriteRequired);
      AddDelayedAction (action);
    }
  }
}
