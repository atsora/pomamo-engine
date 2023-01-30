// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// IContext interface not generic, so without the SwitchTo method
  /// </summary>
  public interface IContext
  {
    /// <summary>
    /// Start date/time
    /// </summary>
    DateTime StateMachineStartDateTime { get; }

    /// <summary>
    /// Get the max time in the state machine
    /// </summary>
    TimeSpan MaxTime { get; }

    /// <summary>
    /// Switch to an end state
    /// </summary>
    void SwitchToEndState ();
  }

  /// <summary>
  /// IContext interface
  /// </summary>
  /// <typeparam name="TContext">real type of the context</typeparam>
  public interface IContext<TContext>
    : IContext
    where TContext : IContext<TContext>
  {
    /// <summary>
    /// Switch the context to a new state
    /// </summary>
    /// <param name="state"></param>
    void SwitchTo (IState<TContext> state);
  }
}
