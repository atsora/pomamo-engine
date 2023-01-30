// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// Interface for a state machine
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public interface IStateMachine<TContext>: IContext<TContext>
    where TContext : IContext<TContext>
  {
    /// <summary>
    /// Initial state
    /// </summary>
    IState<TContext> InitialState { get; }

    /// <summary>
    /// Run the state machine once
    /// 
    /// Suppose the internal properties are already correctly initialized
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>if true is returned, any other process can continue (kind of completed)</returns>
    bool Run (CancellationToken cancellationToken);
  }
}
