// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// Check a condition and swith to alternative states according to the condition result
  /// </summary>
  public class ConditionState<TContext> : State<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CheckMaxTime<TContext>).FullName);

    readonly string m_name;
    readonly Func<bool> m_test;
    readonly IState<TContext> m_trueState;
    readonly IState<TContext> m_falseState;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="test"></param>
    /// <param name="trueState"></param>
    /// <param name="falseState"></param>
    public ConditionState (string name, Func<bool> test, IState<TContext> trueState, IState<TContext> falseState)
      : base (null)
    {
      m_name = name;
      m_test = test;
      m_trueState = trueState;
      m_falseState = falseState;
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => "";

    /// <summary>
    /// Test
    /// </summary>
    /// <returns></returns>
    protected virtual bool Test ()
    {
      return m_test ();
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      try {
        if (Test ()) {
          this.Context.SwitchTo (m_trueState);
        }
        else {
          this.Context.SwitchTo (m_falseState);
        }
      }
      catch (Exception ex) {
        log.Fatal ("Handle: unexpected error", ex);
        throw;
      }
    }
    #endregion // Constructors

  }
}
