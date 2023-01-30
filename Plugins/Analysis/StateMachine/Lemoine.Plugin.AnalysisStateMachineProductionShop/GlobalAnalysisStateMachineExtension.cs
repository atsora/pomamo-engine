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
  public class GlobalAnalysisStateMachineExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGlobalAnalysisStateMachineExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GlobalAnalysisStateMachineExtension).FullName);

    IGlobalAnalysis m_context;
    IState<IGlobalAnalysis> m_initializationState;
    Configuration m_configuration;

    public double Priority => m_configuration?.StateMachinePriority ?? 10.0;

    public IState<IGlobalAnalysis> InitialState => m_initializationState;

    public bool Initialize (IGlobalAnalysis context)
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
      IState<IGlobalAnalysis> nextState;
      var endState = new EndState<IGlobalAnalysis> ();
      nextState = new AnalysisState<IGlobalAnalysis> ("CleanFlaggedModifications", "GlobalCleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IGlobalAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageWeekNumbers", (c, t) => c.ManageWeekNumbers (t), nextState, nextState, maxTimeState: endState);
      nextState = new AnalysisState<IGlobalAnalysis> ("PendingModifications", "GlobalPendingModifications", (c, t) => c.RunPendingModificationsAnalysis (t, 0, 0), nextState, nextState, maxTimeState: endState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageShiftTemplates", (c, t) => c.ManageShiftTemplates (t), nextState, nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageDayTemplates", (c, t) => c.ManageDayTemplates (t), nextState, nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ResetPresent", (c, t) => c.ResetPresent (), nextState, nextState);

      var catchUpState = GetFirstCatchUpState ();
      nextState = new CatchUpSwitchState<IGlobalAnalysis> ("CatchUpSwitch", catchUpState, nextState);

      m_initializationState = new AnalysisState<IGlobalAnalysis> ("GlobalAnalysisInitialization", (c, t) => c.Initialize (), nextState, null);

      if (log.IsDebugEnabled) {
        log.Debug ("Initialize: successfully initialized");
      }

      return true;
    }

    IState<IGlobalAnalysis> GetFirstCatchUpState ()
    {
      var maxTime = TimeSpan.FromDays (1);
      var endState = new EndState<IGlobalAnalysis> ();
      IState<IGlobalAnalysis> nextState = endState;
      nextState = new DeleteApplicationStateState<IGlobalAnalysis> ("DeleteApplicationStateState", $"Analysis.CatchUp.g", nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("CleanFlaggedModifications", "GlobalCleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t, maxTime), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("PendingModifications", "GlobalPendingModifications", (c, t) => c.RunPendingModificationsAnalysis (t, maxTime, 0, 0), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageShiftTemplates", (c, t) => c.ManageShiftTemplates (t, maxTime), nextState, nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageWeekNumbers", (c, t) => c.ManageWeekNumbers (t), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ManageDayTemplates", (c, t) => c.ManageDayTemplates (t, maxTime), nextState, nextState);
      nextState = new AnalysisState<IGlobalAnalysis> ("ResetPresent", (c, t) => c.ResetPresent (), nextState, nextState);

      return nextState;
    }
  }
}
