// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.StateMachine;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// AnalysisState: implementation for basic actions
  /// </summary>
  public class AnalysisState<TContext> : State<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (AnalysisState<TContext>).FullName);

    readonly string m_name;
    readonly string m_perfName;
    readonly Func<TContext, CancellationToken, bool> m_action;
    readonly IState<TContext> m_nextState;
    readonly IState<TContext> m_maxTimeState;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action">if false is returned, it switch to an end state</param>
    /// <param name="nextState"></param>
    /// <param name="exceptionState"></param>
    /// <param name="maxTimeState">if not null, test the maximum time after running the internal state machine</param>
    public AnalysisState (string name, Func<TContext, CancellationToken, bool> action, IState<TContext> nextState, IState<TContext> exceptionState, IState<TContext> maxTimeState = null)
      : this (name, name, action, nextState, exceptionState, maxTimeState)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="perfName"></param>
    /// <param name="action"></param>
    /// <param name="nextState"></param>
    /// <param name="exceptionState"></param>
    /// <param name="maxTimeState">if not null, test the maximum time after running the internal state machine</param>
    public AnalysisState (string name, string perfName, Func<TContext, CancellationToken, bool> action, IState<TContext> nextState, IState<TContext> exceptionState, IState<TContext> maxTimeState = null)
      : base (exceptionState)
    {
      m_name = name;
      m_perfName = perfName;
      m_action = action;
      m_nextState = nextState;
      m_maxTimeState = maxTimeState;
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => m_perfName;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      var result = m_action (this.Context, cancellationToken);
      if (!result) {
        this.Context.SwitchToEndState ();
        return;
      }

      if ((null != m_maxTimeState) && !TestMaxTime ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("Handle: max time reached");
        }
        this.Context.SwitchTo (m_maxTimeState);
        return;
      }

      this.Context.SwitchTo (m_nextState);
    }

  }
}
