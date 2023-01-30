// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonLongIdleSameMachineMode
{
  /// <summary>
  /// 
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // Id of the previous machine mode
    static readonly string PREVIOUS_MACHINE_MODE_ID = "PreviousMachineModeID";
    int m_previousMachineModeId = -1;

    // Cumulative time of the non-productive period
    static readonly string CUMULATIVE_IDLE_DURATION = "CumulativeIdleDuration";
    TimeSpan m_cumulativeIdleDuration;

    // Configuration
    string m_dynamicEnd;
    TimeSpan m_minimumDuration;
    TimeSpan m_maxGapDuration;
    
    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.LongIdleSameMachineMode")
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

      // Configuration
      m_dynamicEnd = configuration.DynamicEnd;
      if (!m_dynamicEnd.StartsWith (",")) {
        m_dynamicEnd = "," + m_dynamicEnd;
      }

      m_minimumDuration = configuration.MinDuration;
      m_maxGapDuration = configuration.MaxGapDuration;

      // Previous machine mode id
      var previousMachineModeId = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (
        this.Machine, GetKey (PREVIOUS_MACHINE_MODE_ID));
      if (previousMachineModeId != null) {
        m_previousMachineModeId = (int)previousMachineModeId.Value;
      }
      else {
        m_previousMachineModeId = -1;
      }

      // Cumulative duration
      var cumulativeDuration = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (
        this.Machine, GetKey (CUMULATIVE_IDLE_DURATION));
      if (cumulativeDuration != null) {
        m_cumulativeIdleDuration = (TimeSpan)cumulativeDuration.Value;
      }
      else {
        m_cumulativeIdleDuration = TimeSpan.FromSeconds (0);
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
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.LongIdleSameMachineMode.Check")) {

          // Get the next fact
          var nextFact = ModelDAOHelper.DAOFactory.FactDAO.FindFirstFactAfter (Machine, DateTime);
          if (nextFact != null) {

            // Possibly process a gap
            if (nextFact.Begin != this.DateTime) {
              ProcessGap (this.DateTime, nextFact.Begin);
            }

            if (nextFact.CncMachineMode != null) {
              if (nextFact.CncMachineMode.Running.HasValue && nextFact.CncMachineMode.Running.Value == true) {
                // We are in production: the idle duration is reset
                AddUpdateState (nextFact.CncMachineMode.Id, TimeSpan.FromSeconds (0));
              } else {
                // Currently no production
                ProcessIdleMachineMode (nextFact.CncMachineMode, nextFact.Begin, nextFact.End);
              }

              // Update the state with a machine mode
              AddUpdateState (nextFact.CncMachineMode.Id, m_cumulativeIdleDuration);
            } else {
              // Update the state with no machine mode (shouldn't arrive)
              AddUpdateState (-1, m_cumulativeIdleDuration);
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

    internal int GetPreviousMachineModeId () { return m_previousMachineModeId; }
    internal void ResetPreviousMachineModeId (int machineModeId) { m_previousMachineModeId = machineModeId; }
    internal TimeSpan GetIdleDuration() { return m_cumulativeIdleDuration; }
    internal void ResetIdleDuration (TimeSpan idleDuration) { m_cumulativeIdleDuration = idleDuration; }

    void AddUpdateState (int machineModeId, TimeSpan idleDuration)
    {
      var action = new UpdateStateAction (this, machineModeId, idleDuration);
      AddDelayedAction (action);
    }

    internal void UpdateState (int machineModeId, TimeSpan idleDuration)
    {
      m_previousMachineModeId = machineModeId;
      log.InfoFormat ("AutoReasonLongIdleSameMachineMode: updated '{0}' to '{1}'", GetKey (PREVIOUS_MACHINE_MODE_ID), m_previousMachineModeId);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_MACHINE_MODE_ID), m_previousMachineModeId);

      m_cumulativeIdleDuration = idleDuration;
      log.InfoFormat ("AutoReasonLongIdleSameMachineMode: updated '{0}' to '{1}'", GetKey (CUMULATIVE_IDLE_DURATION), m_cumulativeIdleDuration);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (CUMULATIVE_IDLE_DURATION), m_cumulativeIdleDuration);
    }

    void ProcessGap (DateTime startDateTime, DateTime endDateTime)
    {
      // Compute the current idle duration that can be added to the possibly existing cumulativeIdleDuration
      var currentIdleDuration = endDateTime.Subtract (DateTime);

      if (currentIdleDuration > m_maxGapDuration) {
        // The gap is considered to be a break
        m_cumulativeIdleDuration = TimeSpan.FromSeconds (0);
      } else {
        // The gap is attached to the previous machine mode, possibly extending the idle duration
        if (currentIdleDuration.TotalSeconds > 0) {
          if (m_cumulativeIdleDuration < m_minimumDuration && m_cumulativeIdleDuration + currentIdleDuration >= m_minimumDuration &&
          m_cumulativeIdleDuration.TotalDays < 1) {
            // Load the machine mode display
            var machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (m_previousMachineModeId);
            string machineModeDisplay = machineMode != null ? machineMode.Display : ("unknown machine mode " + m_previousMachineModeId);

            // Create a delayed action for applying the reason
            var action = new ApplyReasonAction (this, new UtcDateTimeRange (startDateTime.Subtract (m_cumulativeIdleDuration)), m_dynamicEnd, machineModeDisplay);
            AddDelayedAction (action);
          }
        }

        // Update the cumulative idle duration
        m_cumulativeIdleDuration = m_cumulativeIdleDuration.Add (currentIdleDuration);
      }
    }

    void ProcessIdleMachineMode (IMachineMode machineMode, DateTime startDateTime, DateTime endDateTime)
    {
      // Compute the current idle duration that can be added to the possibly existing cumulativeIdleDuration
      var currentIdleDuration = endDateTime.Subtract (DateTime);

      if (machineMode.Id == m_previousMachineModeId) {
        // The machine mode is the same, durations are cumulated and a reason is possibly triggered
        if (m_cumulativeIdleDuration < m_minimumDuration && m_cumulativeIdleDuration + currentIdleDuration >= m_minimumDuration &&
          m_cumulativeIdleDuration.TotalDays < 1) {
          // Create a delayed action for applying the reason
          var action = new ApplyReasonAction (this, new UtcDateTimeRange (startDateTime.Subtract(m_cumulativeIdleDuration)), m_dynamicEnd, machineMode.Display);
          AddDelayedAction (action);
        }
      } else {
        // A new idle period begins
        //m_cumulativeIdleDuration = TimeSpan.FromSeconds (0);

        if (currentIdleDuration >= m_minimumDuration && m_cumulativeIdleDuration.TotalDays < 1) {
          // Create a delayed action for applying the reason
          //var action = new ApplyReasonAction (this, new UtcDateTimeRange (startDateTime), m_dynamicEnd,
          //machineMode.Display);
          var previousMachineModeId = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (m_previousMachineModeId);
          if (null != previousMachineModeId && previousMachineModeId.Running.Value == false) {
            var action = new ApplyReasonAction (this, new UtcDateTimeRange (startDateTime.Subtract (currentIdleDuration)), m_dynamicEnd, previousMachineModeId.Display);
            AddDelayedAction (action);
          }
          else {
            var action = new ApplyReasonAction (this, new UtcDateTimeRange (startDateTime), m_dynamicEnd, machineMode.Display);
            AddDelayedAction (action);
          }
          
        }
        m_cumulativeIdleDuration = TimeSpan.FromSeconds (0);
      }

      m_cumulativeIdleDuration = m_cumulativeIdleDuration.Add (currentIdleDuration);
    }
  }
}
