// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis.StateMachine;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AnalysisStateMachineLiveOperationStatus
{
  public class MonitoredMachineActivityAnalysisStateMachineExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IMonitoredMachineActivityAnalysisStateMachineExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineActivityAnalysisStateMachineExtension).FullName);

    IMachineActivityAnalysis m_context;
    IState<IMonitoredMachineActivityAnalysis> m_initializationState;
    Configuration m_configuration;

    public double Priority => m_configuration?.StateMachinePriority ?? 20.0;

    public IState<IMonitoredMachineActivityAnalysis> InitialState => m_initializationState;

    public bool Initialize (IMonitoredMachineActivityAnalysis context)
    {
      Debug.Assert (null != context);
      if (null == context) {
        log.Fatal ($"Initialize: context is null, unexpected");
        return false;
      }
      if (false == LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: load configuration returned false");
        return false;
      }

      m_context = context;

      if (!m_configuration.CheckMachineFilter (m_context.Machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: CheckMachineFilter returned false");
        }
        return false;
      }

      var productionState = GetProductionState ();
      var notProductionState = GetNotProductionState ();
      var productionSwitchState = new ProductionSwitchState<IMonitoredMachineActivityAnalysis> ("ProductionSwitch", m_context.Machine, productionState, notProductionState);

      var catchUpState = GetFirstCatchUpState ();
      var catchUpSwitchState = new CatchUpSwitchState<IMonitoredMachineActivityAnalysis> ("CatchUpSwitch", m_context.Machine, catchUpState, productionSwitchState);

      m_initializationState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineActivityAnalysisInitialization", (c, t) => c.Initialize (), catchUpSwitchState, null);

      return true;
    }

    IState<IMonitoredMachineActivityAnalysis> GetProductionState ()
    {
      var noTimeSpan = TimeSpan.FromTicks (0);

      IState<IMonitoredMachineActivityAnalysis> nextState;
      var endState = new EndState<IMonitoredMachineActivityAnalysis> ();
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IMonitoredMachineActivityAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c,t) => c.RunProcessingReasonSlotsAnalysis (t, m_configuration.ProcessingReasonSlotsMaxTime, noTimeSpan), nextState, nextState, maxTimeState: endState);
      var normalPriority = m_configuration.NormalModificationPriority;
      var lowPriority = m_configuration.LowModificationPriority;
      var lowPriorityFrequency = m_configuration.LowPriorityFrequency;
      var veryLowPriorityFrequency = m_configuration.VeryLowPriorityFrequency;
      var allPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsAll", (c,t) => c.RunPendingModificationsAnalysis (t, m_configuration.PendingModificationsMaxTime, noTimeSpan, 0, 0), nextState, nextState, maxTimeState: endState);
      var lowPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsLow", (c, t) => c.RunPendingModificationsAnalysis (t, m_configuration.PendingModificationsMaxTime, noTimeSpan, lowPriority, lowPriority), nextState, nextState, maxTimeState: endState);
      var normalPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsNormal", (c, t) => c.RunPendingModificationsAnalysis (t, m_configuration.PendingModificationsMaxTime, noTimeSpan, normalPriority, normalPriority), nextState, nextState, maxTimeState: endState);
      var lowOrNormalState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", lowPriorityFrequency, lowPendingModificationsState, normalPendingModificationsState);
      nextState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", veryLowPriorityFrequency, allPendingModificationsState, lowOrNormalState);

      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: normalPendingModificationsState);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("AutoSequence", (c, t) => c.RunAutoSequenceAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Detection", (c, t) => c.RunDetectionAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysisLastPeriod (t, TimeSpan.FromHours (8)), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", (c, t) => c.RunActivityAnalysis (t), nextState, nextState, maxTimeState: nextState);
      var activityState = nextState;

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Production", (c, t) => c.RunProductionAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("OperationSlotSplit", (c, t) => c.RunOperationSlotSplitAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", (c, t) => c.ManageMachineStateTemplates (t), nextState, nextState);

      return nextState;
    }

    IState<IMonitoredMachineActivityAnalysis> GetNotProductionState ()
    {
      IState<IMonitoredMachineActivityAnalysis> nextState;
      var endState = new EndState<IMonitoredMachineActivityAnalysis> ();
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IMonitoredMachineActivityAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("AutoSequence", (c, t) => c.RunAutoSequenceAnalysis (t), nextState, nextState, maxTimeState: endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Detection", (c, t) => c.RunDetectionAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), nextState, nextState, maxTimeState: nextState);
      var activityStateExceptionState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), endState, endState, maxTimeState: endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", (c, t) => c.RunActivityAnalysis (t), nextState, activityStateExceptionState, maxTimeState: nextState);
      var activityState = nextState;

      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);

      var normalPriority = m_configuration.NormalModificationPriority;
      var lowPriority = m_configuration.LowModificationPriority;
      var lowPriorityFrequency = m_configuration.LowPriorityFrequency;
      var veryLowPriorityFrequency = m_configuration.VeryLowPriorityFrequency;
      var allPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsAll", (c, t) => c.RunPendingModificationsAnalysis (t, 0, 0), nextState, nextState, maxTimeState: activityState);
      var lowPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsLow", (c, t) => c.RunPendingModificationsAnalysis (t, 0, lowPriority), nextState, nextState, maxTimeState: activityState);
      var normalPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsNormal", (c, t) => c.RunPendingModificationsAnalysis (t, 0, normalPriority), nextState, nextState, maxTimeState: activityState);
      var lowOrNormalState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", lowPriorityFrequency, lowPendingModificationsState, normalPendingModificationsState);
      nextState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", veryLowPriorityFrequency, allPendingModificationsState, lowOrNormalState);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Production", (c, t) => c.RunProductionAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("OperationSlotSplit", (c, t) => c.RunOperationSlotSplitAnalysis (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", (c, t) => c.ManageMachineStateTemplates (t), nextState, nextState);

      return nextState;
    }

    IState<IMonitoredMachineActivityAnalysis> GetFirstCatchUpState ()
    {
      var maxTime = TimeSpan.FromDays (1);
      var endState = new EndState<IMonitoredMachineActivityAnalysis> ();
      IState<IMonitoredMachineActivityAnalysis> nextState = endState;
      nextState = new DeleteApplicationStateState<IMonitoredMachineActivityAnalysis> ("DeleteApplicationStateState", m_context.Machine, $"Analysis.CatchUp.{m_context.Machine.Id}", nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModificationsCatchUp", (c, t) => c.CleanFlaggedModifications (t, maxTime, maxTime), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("AutoSequence", "AutoSequenceCatchUp", (c, t) => c.RunAutoSequenceAnalysis (t, maxTime, maxTime, int.MaxValue), nextState, nextState, maxTimeState: endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Detection", "DetectionCatchUp", (c, t) => c.RunDetectionAnalysis (t, maxTime, maxTime, int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", "ProcessingReasonSlotsCatchUp", (c, t) => c.RunProcessingReasonSlotsAnalysis (t, maxTime, maxTime, maxLoopNumber: int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", "ActivityCatchUp", (c, t) => c.RunActivityAnalysis (t, int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsCatchUp", (c, t) => c.RunPendingModificationsAnalysis (t, maxTime, maxTime, 0, 0), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Production", "ProductionCatchUp", (c, t) => c.RunProductionAnalysis (t, maxTime, maxTime), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("OperationSlotSplit", "OperationSlotSplitCatchUp", (c, t) => c.RunOperationSlotSplitAnalysis (t, maxTime, maxTime, TimeSpan.FromDays (31)), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", "MachinteStateTemplateCatchUp", (c, t) => c.ManageMachineStateTemplates (t, maxTime, maxTime), nextState, nextState);

      return nextState;
    }
  }
}
