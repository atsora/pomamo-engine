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
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.AnalysisStateMachineRunTimeOnly
{
  public class MonitoredMachineActivityAnalysisStateMachineExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IMonitoredMachineActivityAnalysisStateMachineExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineActivityAnalysisStateMachineExtension).FullName);

    IMachineActivityAnalysis m_context;
    IState<IMonitoredMachineActivityAnalysis> m_initializationState;
    Configuration m_configuration;

    public double Priority => m_configuration?.StateMachinePriority ?? 10.0;

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
      IState<IMonitoredMachineActivityAnalysis> nextState;
      var endState = new EndState<IMonitoredMachineActivityAnalysis> ();
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IMonitoredMachineActivityAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);

      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);
      var extensionAnalysisState = nextState;

      var normalPriority = m_configuration.NormalModificationPriority;
      var normalPriorityFrequency = m_configuration.NormalPriorityFrequency;
      var lowPriority = m_configuration.LowModificationPriority;
      var lowPriorityFrequency = m_configuration.LowPriorityFrequency;
      var veryLowPriorityFrequency = m_configuration.VeryLowPriorityFrequency;
      var allPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsAll", (c, t) => c.RunPendingModificationsAnalysis (t, 0, 0), nextState, nextState, maxTimeState: endState);
      var lowPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsLow", (c, t) => c.RunPendingModificationsAnalysis (t, 0, lowPriority), nextState, nextState, maxTimeState: endState);
      var normalPendingModificationsState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsNormal", (c, t) => c.RunPendingModificationsAnalysis (t, 0, normalPriority), nextState, nextState, maxTimeState: endState);
      var normalStateOrSkip = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", normalPriorityFrequency, normalPendingModificationsState, extensionAnalysisState);
      var lowOrNormalState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", lowPriorityFrequency, lowPendingModificationsState, normalStateOrSkip);
      nextState = new FrequencyState<IMonitoredMachineActivityAnalysis> ("PendingModificationsSwitch", veryLowPriorityFrequency, allPendingModificationsState, lowOrNormalState);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), nextState, nextState, maxTimeState: nextState);
      var activityStateExceptionState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", (c, t) => c.RunProcessingReasonSlotsAnalysis (t), endState, endState, maxTimeState: endState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", (c, t) => c.RunActivityAnalysis (t), nextState, activityStateExceptionState, maxTimeState: nextState);

      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", (c, t) => c.ManageMachineStateTemplates (t), nextState, nextState);

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
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("ProcessingReasonSlots", "ProcessingReasonSlotsCatchUp", (c, t) => c.RunProcessingReasonSlotsAnalysis (t, maxTime, maxTime, maxLoopNumber: int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("Activity", "ActivityCatchUp", (c, t) => c.RunActivityAnalysis (t, int.MaxValue), nextState, nextState, maxTimeState: nextState);
      nextState = new MultiState<IMonitoredMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMonitoredMachineActivityAnalysis> (), nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("PendingModifications", "PendingModificationsCatchUp", (c, t) => c.RunPendingModificationsAnalysis (t, maxTime, maxTime, 0, 0), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMonitoredMachineActivityAnalysis> ("MachineStateTemplate", "MachinteStateTemplateCatchUp", (c, t) => c.ManageMachineStateTemplates (t, maxTime, maxTime), nextState, nextState);

      return nextState;
    }

  }
}
