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

namespace Lemoine.Plugin.AnalysisStateMachineMoldShop
{
  public class MachineActivityAnalysisStateMachineExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IMachineActivityAnalysisStateMachineExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineActivityAnalysisStateMachineExtension).FullName);

    IMachineActivityAnalysis m_context;
    IState<IMachineActivityAnalysis> m_initializationState;

    public double Priority => 10.0;

    public IState<IMachineActivityAnalysis> InitialState => m_initializationState;

    public bool Initialize (IMachineActivityAnalysis context)
    {
      Debug.Assert (null != context);
      if (null == context) {
        log.Fatal ($"Initialize: context is null, unexpected");
        return false;
      }

      m_context = context;
      IState<IMachineActivityAnalysis> nextState;
      var endState = new EndState<IMachineActivityAnalysis> ();
      nextState = new AnalysisState<IMachineActivityAnalysis> ("CleanFlaggedModifications", "CleanFLaggedModifications", (c, t) => c.CleanFlaggedModifications (t), endState, endState, maxTimeState: endState);
      nextState = new ConditionState<IMachineActivityAnalysis> ("TestIsCleanFlaggedModificationsRequired",
        m_context.IsCleanFlaggedModificationsRequired, nextState, endState);
      nextState = new MultiState<IMachineActivityAnalysis> (m_context.GetExtensionAnalysisStates<IMachineActivityAnalysis> (), nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMachineActivityAnalysis> ("PendingModifications", (c, t) => c.RunPendingModificationsAnalysis (t, 0, 0), nextState, nextState, maxTimeState: nextState);
      nextState = new AnalysisState<IMachineActivityAnalysis> ("MachineStateTemplate", (c, t) => c.ManageMachineStateTemplates (t), nextState, nextState, maxTimeState: nextState);
      m_initializationState = new AnalysisState<IMachineActivityAnalysis> ("MachineActivityAnalysisInitialization", (c, t) => c.Initialize (), nextState, null);

      return true;
    }

  }
}
