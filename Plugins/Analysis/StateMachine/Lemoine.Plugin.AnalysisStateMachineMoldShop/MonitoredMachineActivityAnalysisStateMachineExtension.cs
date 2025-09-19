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

namespace Lemoine.Plugin.AnalysisStateMachineProductionShop
{
  public class MonitoredMachineActivityAnalysisStateMachineExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IMonitoredMachineActivityAnalysisStateMachineExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineActivityAnalysisStateMachineExtension).FullName);

    static readonly string HIGH_PRIORITY_KEY = "Analysis.HighPriority";
    static readonly int HIGH_PRIORITY_DEFAULT = 1000;

    static readonly string NORMAL_PRIORITY_KEY = "Analysis.NormalPriority";
    static readonly int NORMAL_PRIORITY_DEFAULT = 100; // 80 includes the auto-reasons

    static readonly string LOW_PRIORITY_FREQUENCY_KEY = "Analysis.LowPriorityFrequency";
    static readonly TimeSpan LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (2);

    static readonly string LOW_PRIORITY_KEY = "Analysis.LowPriority";
    static readonly int LOW_PRIORITY_DEFAULT = 50;

    static readonly string VERY_LOW_PRIORITY_FREQUENCY_KEY = "Analysis.VeryLowPriorityFrequency";
    static readonly TimeSpan VERY_LOW_PRIORITY_FREQUENCY_DEFAULT = TimeSpan.FromMinutes (5);

    IMachineActivityAnalysis m_context;
    IState<IMonitoredMachineActivityAnalysis> m_initializationState;

    public double Priority => 10.0;

    public IState<IMonitoredMachineActivityAnalysis> InitialState => m_initializationState;

    public bool Initialize (IMonitoredMachineActivityAnalysis context)
    {
      Debug.Assert (null != context);
      if (null == context) {
        log.Fatal ($"Initialize: context is null, unexpected");
        return false;
      }

      var highPriority = Lemoine.Info.ConfigSet
        .LoadAndGet (HIGH_PRIORITY_KEY, HIGH_PRIORITY_DEFAULT);
      m_context = context;
      IState<IMonitoredMachineActivityAnalysis> nextState;
      var endState = new EndState<IMonitoredMachineActivityAnalysis> ();
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IMonitoredMachineActivityAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);
      var autoSequenceState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("AutoSequence", (c, t) => c.RunAutoSequenceAnalysis (t), nextState, nextState, maxTimeState: endState);
      var processingState2 = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots2", "ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), autoSequenceState, autoSequenceState, maxTimeState: autoSequenceState);
      var beforeProcessingState2 = new CheckMaxTime<IMonitoredMachineActivityAnalysis> (autoSequenceState, processingState2);
      var modificationsState2 = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications2", "PendingModificationsHigh", (c, t) => c.RunPendingModificationsAnalysis (t, highPriority, highPriority), beforeProcessingState2, beforeProcessingState2, maxTimeState: autoSequenceState);
      var beforeModificationsState2 = new CheckMaxTime<IMonitoredMachineActivityAnalysis> (autoSequenceState, modificationsState2);

      var detectionState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Detection", (c, t) => c.RunDetectionAnalysis (t), beforeModificationsState2, beforeModificationsState2, maxTimeState: autoSequenceState);

      var processingState1 = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots1", "ProcessingReasonSlots", (c,t) => c.RunProcessingReasonSlotsAnalysis (t), detectionState, detectionState, maxTimeState: detectionState);
      var beforeProcessingState1 = new CheckMaxTime<IMonitoredMachineActivityAnalysis> (detectionState, processingState1);
      var modificationsState1 = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications1", "PendingModificationsHigh", (c, t) => c.RunPendingModificationsAnalysis (t, highPriority, highPriority), beforeProcessingState1, beforeProcessingState1, maxTimeState: detectionState);
      var beforeModificationsState1 = new CheckMaxTime<IMonitoredMachineActivityAnalysis> (detectionState, modificationsState1);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), beforeModificationsState1, beforeModificationsState1, maxTimeState: detectionState);
      var activityStateExceptionState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), endState, endState, maxTimeState: endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", (c, t) => c.RunActivityAnalysis (t), nextState, activityStateExceptionState, maxTimeState: nextState);
      var activityState = nextState;

      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);

      var normalPriority = Lemoine.Info.ConfigSet
        .LoadAndGet (NORMAL_PRIORITY_KEY, NORMAL_PRIORITY_DEFAULT);
      var lowPriorityFrequency = Lemoine.Info.ConfigSet
        .LoadAndGet (LOW_PRIORITY_FREQUENCY_KEY, LOW_PRIORITY_FREQUENCY_DEFAULT);
      var lowPriority = Lemoine.Info.ConfigSet
        .LoadAndGet (LOW_PRIORITY_KEY, LOW_PRIORITY_DEFAULT);
      var veryLowPriorityFrequency = Lemoine.Info.ConfigSet
        .LoadAndGet (VERY_LOW_PRIORITY_FREQUENCY_KEY, VERY_LOW_PRIORITY_FREQUENCY_DEFAULT);
      var allPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsAll", (c, t) => c.RunPendingModificationsAnalysis (t, 0, 0), nextState, nextState, maxTimeState: activityState);
      var lowPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsLow", (c, t) => c.RunPendingModificationsAnalysis (t, 0, lowPriority), nextState, nextState, maxTimeState: activityState);
      var normalPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsNormal", (c, t) => c.RunPendingModificationsAnalysis (t, 0, normalPriority), nextState, nextState, maxTimeState: activityState);
      var lowOrNormalState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", lowPriorityFrequency, lowPendingModificationsState, normalPendingModificationsState);
      nextState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", veryLowPriorityFrequency, allPendingModificationsState, lowOrNormalState);

      var operationSlotSplitOption = (OperationSlotSplitOption)Lemoine.Info.ConfigSet
        .LoadAndGet<int> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption),
          (int)OperationSlotSplitOption.None);
      if (operationSlotSplitOption.IsActive ()) {
        nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("OperationSlotSplit", (c, t) => c.RunOperationSlotSplitAnalysis (t), nextState, nextState, maxTimeState: nextState);
      }

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", (c,t) => c.ManageMachineStateTemplates (t), nextState, nextState, maxTimeState: nextState);

      var catchUpState = GetFirstCatchUpState ();
      nextState = new CatchUpSwitchState<IMonitoredMachineActivityAnalysis> ("CatchUpSwitch", m_context.Machine, catchUpState, nextState);

      m_initializationState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineActivityAnalysisInitialization", (c, t) => c.Initialize (), nextState, null);

      return true;
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
      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", "ProcessingReasonSlotsCatchUp", (c, t) => c.RunProcessingReasonSlotsAnalysis (t, maxTime, maxTime, maxLoopNumber: int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", "ActivityCatchUp", (c, t) => c.RunActivityAnalysis (t, int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsCatchUp", (c, t) => c.RunPendingModificationsAnalysis (t, maxTime, maxTime, 0, 0), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", "MachinteStateTemplateCatchUp", (c, t) => c.ManageMachineStateTemplates (t, maxTime, maxTime), nextState, nextState);

      return nextState;
    }

  }
}
