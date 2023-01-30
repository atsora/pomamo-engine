// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// State machine interface common to all state machine definitions
  /// </summary>
  public interface IAdvancedStateMachineDefinition<TContext>
    where TContext: IContext<TContext>
  {
    /// <summary>
    /// Initial state
    /// </summary>
    IState<TContext> InitialState { get; }

    /// <summary>
    /// Get a specific performance tracker
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    string GetPerfTrackerKey (IState<TContext> state);

    /// <summary>
    /// Run after an end excecution (only if successful)
    /// </summary>
    /// <param name="state"></param>
    /// <returns>false if the state machine must be interrupted</returns>
    bool EndStateExcecution (IState<TContext> state);

    /// <summary>
    /// Manage an exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns>continue if true, interrupt if false</returns>
    bool ManageException (Exception ex);

    /// <summary>
    /// Run once the state machine reached a final state or was interrupted
    /// </summary>
    void EndStateMachine ();
  }
}
