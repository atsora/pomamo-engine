// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// Check if the max time for the state machine execution is reached,
  /// and switch to the next state according to the result
  /// </summary>
  public class CheckMaxTime<TContext> : ConditionState<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CheckMaxTime<TContext>).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="elapsedTimeState"></param>
    /// <param name="nextState"></param>
    public CheckMaxTime (IState<TContext> elapsedTimeState, IState<TContext> nextState)
      : base ("CheckMaxTime", () => true, nextState, elapsedTimeState)
    {
    }

    /// <summary>
    /// Use TestMaxTime for the test
    /// </summary>
    /// <returns></returns>
    protected override bool Test ()
    {
      return TestMaxTime ();
    }
    #endregion // Constructors

  }
}
