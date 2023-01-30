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
  /// MultiState
  /// </summary>
  public class MultiState<TContext> : State<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (MultiState<TContext>).FullName);

    readonly IState<TContext> m_currentState;
    readonly IEnumerable<IState<TContext>> m_remainingStates;
    readonly IState<TContext> m_nextState;
    readonly IState<TContext> m_maxTimeState;
    readonly bool m_nextIsExceptionState;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="states"></param>
    /// <param name="nextState">not null. Next state once all the states have been processed</param>
    /// <param name="nextIsExceptionState"></param>
    /// <param name="maxTimeState">if not null, test the maximum time after each iteration. If the time is reached, switch to the specified state</param>
    public MultiState (IEnumerable<IState<TContext>> states, IState<TContext> nextState, bool nextIsExceptionState = true, IState<TContext> maxTimeState = null)
      : base (null)
    {
      Debug.Assert (null != nextState);

      m_nextIsExceptionState = nextIsExceptionState;
      m_maxTimeState = maxTimeState;
      m_nextState = nextState;
      if (states.Any ()) {
        m_currentState = states.First ();
        m_remainingStates = states.Skip (1);
        if (null != m_currentState.ExceptionState) {
          this.ExceptionState = m_currentState.ExceptionState;
        }
        else if (nextIsExceptionState) {
          this.ExceptionState = GetNextState ();
        }
      }
      else {
        m_currentState = null;
      }
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override bool End => m_currentState?.End ?? false;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_currentState?.Name ?? "EmptyMultiState";

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => m_currentState?.PerfName ?? "";

    IState<TContext> GetNextState ()
    {
      if (m_remainingStates.Any ()) {
        return new MultiState<TContext> (m_remainingStates, m_nextState, m_nextIsExceptionState, m_maxTimeState);
      }
      else if (null != m_nextState) {
        return m_nextState;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      if (null != m_currentState) {
        m_currentState.Handle (cancellationToken);

        if ( (null != m_maxTimeState) && !TestMaxTime ()) {
          if (log.IsDebugEnabled) {
            log.Debug ("Handle: max time reached");
          }
          this.Context.SwitchTo (m_maxTimeState);
          return;
        }

        var next = GetNextState ();
        if (null != next) {
          this.Context.SwitchTo (next);
        }
      }
      else { // Initially no state
        this.Context.SwitchTo (m_nextState);
      }
    }

  }
}
