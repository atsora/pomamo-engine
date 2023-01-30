// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// Interface for a state
  /// 
  /// Not generic so without the SetContext method
  /// </summary>
  public interface IState
  {
    /// <summary>
    /// Name of the state
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Name of the state to use in the performance tracker
    /// 
    /// If null or empty, to performance tracker is used
    /// </summary>
    string PerfName { get; }

    /// <summary>
    /// Is it the last state ?
    /// </summary>
    bool End { get; }

    /// <summary>
    /// Handle it
    /// </summary>
    /// <param name="cancellationToken"></param>
    void Handle (CancellationToken cancellationToken);
  }

  /// <summary>
  /// State interface
  /// </summary>
  /// <typeparam name="TContext">real type of the context</typeparam>
  public interface IState<TContext>
    : IState
    where TContext : IContext<TContext>
  {
    /// <summary>
    /// Associated context
    /// </summary>
    TContext Context { get; }

    /// <summary>
    /// Next state in case an exception occurs. If null, throw the exception
    /// </summary>
    IState<TContext> ExceptionState { get; set; }

    /// <summary>
    /// Set the context
    /// </summary>
    /// <param name="context"></param>
    void SetContext (TContext context);
  }
}
