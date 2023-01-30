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
  /// AnalysisState: implementation for basic actions
  /// </summary>
  public class StateMachineState<TContext, TInternalContext> : State<TContext>
    where TContext : IContext<TContext>, TInternalContext
    where TInternalContext: IContext<TInternalContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (StateMachineState<TContext, TInternalContext>).FullName);

    readonly string m_name;
    readonly string m_perfName;
    readonly IStateMachine<TInternalContext> m_internalStateMachine;
    readonly IState<TContext> m_nextState;
    readonly IState<TContext> m_maxTimeState;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="internalStateMachine"></param>
    /// <param name="nextState"></param>
    /// <param name="exceptionState"></param>
    /// <param name="maxTimeState">if not null, test the maximum time after running the internal state machine</param>
    public StateMachineState (string name, IStateMachine<TInternalContext> internalStateMachine, IState<TContext> nextState, IState<TContext> exceptionState, IState<TContext> maxTimeState = null)
      : this (name, name, internalStateMachine, nextState, exceptionState, maxTimeState)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="perfName"></param>
    /// <param name="internalStateMachine"></param>
    /// <param name="nextState"></param>
    /// <param name="exceptionState"></param>
    /// <param name="maxTimeState">if not null, test the maximum time after running the internal state machine</param>
    public StateMachineState (string name, string perfName, IStateMachine<TInternalContext> internalStateMachine, IState<TContext> nextState, IState<TContext> exceptionState, IState<TContext> maxTimeState = null)
      : base (exceptionState)
    {
      m_name = name;
      m_perfName = perfName;
      m_internalStateMachine = internalStateMachine;
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
      var result = m_internalStateMachine.Run (cancellationToken);
      if (!result) {
        this.Context.SwitchToEndState ();
        return;
      }

      if ( (null != m_maxTimeState) && !TestMaxTime ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("Handle: max time reached");
        }
        this.Context.SwitchTo (m_maxTimeState);
        return;
      }

      this.Context.SwitchTo (m_nextState);
    }
    #endregion // Constructors

  }
}
