// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// StateMachine
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public class AdvancedStateMachine<TContext>
    : StateMachine<TContext>
    , IContext<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (AdvancedStateMachine<TContext>).FullName);

    readonly IAdvancedStateMachineDefinition<TContext> m_definition;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">not null</param>
    /// <param name="stateMachineDefinition">not null</param>
    public AdvancedStateMachine (TContext context, IAdvancedStateMachineDefinition<TContext> stateMachineDefinition)
      : base (context, stateMachineDefinition.InitialState)
    {
      m_definition = stateMachineDefinition;
    }
    #endregion // Constructors

    /// <summary>
    /// Get the PerfTracker key
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected override string GetPerfTrackerKey (IState<TContext> state)
    {
      return m_definition.GetPerfTrackerKey (state);
    }

    /// <summary>
    /// Execute the current state
    /// </summary>
    /// <returns></returns>
    protected override bool RunState (IState<TContext> state, CancellationToken cancellationToken)
    {
      if (base.RunState (state, cancellationToken)) { // Successful
        return m_definition.EndStateExcecution (state);
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Manage an exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns>continue if true, interrupt if false</returns>
    protected override bool ManageException (Exception ex)
    {
      if (m_definition.ManageException (ex)) {
        if (!IsExceptionState ()) {
          log.Error ($"ManageException: no exception state is defined => throw an exception", ex);
          throw new Exception ("No exception state", ex);
        }
        log.Info ($"ManageException: switch to the exception state", ex);
        this.SwitchToExceptionState ();
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Run once the state machine reached a final state or was interrupted
    /// </summary>
    protected override void EndStateMachine ()
    {
      m_definition.EndStateMachine ();
    }
  }
}
