// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonMachineMode
{
  /// <summary>
  /// Detect machine modes and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // Id of the previous machine mode
    static readonly string PREVIOUS_MACHINE_MODE_ID = "PreviousMachineModeID";
    int m_previousMachineModeId = -1;

    string m_dynamicEnd;
    IMachineMode m_monitoredMachineMode;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base ("AutoReason.MachineMode")
    {
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      // Dynamic end (possibly prepend with a coma if it's not done already)
      m_dynamicEnd = configuration.DynamicEnd;
      if (!m_dynamicEnd.StartsWith (",")) {
        m_dynamicEnd = "," + m_dynamicEnd;
      }

      m_monitoredMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
        .FindById (configuration.MachineModeId);
      if (null == m_monitoredMachineMode) {
        log.ErrorFormat ("InitializeAdditionalConfigurations: machine mode id {0} does not exist",
          configuration.MachineModeId);
        return false;
      }

      // Previous machine mode id
      var previousMachineModeId = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .GetAutoReasonState (this.Machine, GetKey (PREVIOUS_MACHINE_MODE_ID));
      if (previousMachineModeId != null) {
        m_previousMachineModeId = (int)previousMachineModeId.Value;
      }
      else {
        m_previousMachineModeId = -1;
      }

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
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.MachineMode.Check")) {

          // Get the next fact
          var nextFact = ModelDAOHelper.DAOFactory.FactDAO.FindFirstFactAfter (Machine, DateTime);

          if (nextFact != null) {

            // Skip the new fact if the machine mode has already been processed
            if (nextFact.CncMachineMode != null && nextFact.CncMachineMode.Id != m_previousMachineModeId) {
              AddUpdatePreviousMachineModeIdAction (nextFact.CncMachineMode.Id);

              // Process the new machine mode
              ProcessMachineMode (nextFact.CncMachineMode, nextFact.Begin);
            }

            // Update the current date time
            GoOn (nextFact.End);
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      AddUpdateDateTimeDelayedAction (dateTime);
    }

    void AddUpdatePreviousMachineModeIdAction (int machineModeId)
    {
      var action = new UpdatePreviousMachineModeIdAction (this, machineModeId);
      AddDelayedAction (action);
    }

    internal int GetPreviousMachineModeId ()
    {
      return m_previousMachineModeId;
    }

    internal void UpdatePreviousMachineModeId (int machineModeId)
    {
      m_previousMachineModeId = machineModeId;
      log.InfoFormat ("AutoReasonMachineMode: updated '{0}' to '{1}'", GetKey (PREVIOUS_MACHINE_MODE_ID), m_previousMachineModeId);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_MACHINE_MODE_ID), m_previousMachineModeId);
    }

    internal void ResetPreviousMachineModeId (int machineModeId)
    {
      m_previousMachineModeId = machineModeId;
    }

    void ProcessMachineMode (IMachineMode machineMode, DateTime startDateTime)
    {
      if (!machineMode.IsDescendantOrSelfOf (m_monitoredMachineMode)) {
        return;
      }

      var range = new UtcDateTimeRange (startDateTime);
      var details = machineMode.Display;
      AddReason (range, m_dynamicEnd, details);
    }

    void AddReason (UtcDateTimeRange range, string dynamic, string details)
    {
      var action = new ApplyReasonAction (this, range, dynamic, details);
      AddDelayedAction (action);
    }
  }
}
