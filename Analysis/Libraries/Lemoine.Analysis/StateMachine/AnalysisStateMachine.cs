// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis.StateMachine;

namespace Lemoine.Analysis.StateMachine
{
  /// <summary>
  /// State machine for the analysis
  /// </summary>
  /// <typeparam name="TContext">interface in Lemoine.Extensions.Analysis</typeparam>
  /// <typeparam name="TStateMachineAnalysis">analysis type</typeparam>
  public sealed class AnalysisStateMachine<TContext, TStateMachineAnalysis>
    : AdvancedStateMachine<TContext>
    where TContext : IContext<TContext>
    where TStateMachineAnalysis : IStateMachineAnalysis, TContext
  {
    readonly ILog log = LogManager.GetLogger (typeof (AnalysisStateMachine<TContext, TStateMachineAnalysis>).FullName);



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="initialState">not null</param>
    /// <param name="analysis">not null</param>
    /// <param name="throwStateException"></param>
    public AnalysisStateMachine (IState<TContext> initialState, TStateMachineAnalysis analysis, bool throwStateException = false)
      : base (analysis, new AnalysisStateMachineDefinition<TContext> (initialState, analysis, throwStateException))
    {
    }
  }
}
