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
  /// State: base class (abstract)
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public abstract class State<TContext>
    : IState<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (State<TContext>).FullName);

    TContext m_context;

    #region Getters / Setters
    /// <summary>
    /// Context
    /// </summary>
    public TContext Context => m_context;

    /// <summary>
    /// Exception state
    /// 
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public virtual IState<TContext> ExceptionState { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="exceptionState">if null, the exception is thrown</param>
    public State (IState<TContext> exceptionState)
    {
      this.ExceptionState = exceptionState;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public abstract string PerfName { get; }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public virtual bool End => false;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public abstract void Handle (CancellationToken cancellationToken);

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    /// <param name="context"></param>
    public void SetContext (TContext context)
    {
      m_context = context;
    }

    /// <summary>
    /// If false is returned, the maximum time is reached
    /// </summary>
    /// <returns></returns>
    public bool TestMaxTime ()
    {
      TimeSpan timeSpent = DateTime.UtcNow.Subtract (this.Context.StateMachineStartDateTime);
      if (this.Context.MaxTime < timeSpent) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ($"TestMaxTime: the time spent in MakeAnalysis is already {timeSpent} (more than {this.Context.MaxTime}) => exit");
        }
        return false;
      }
      return true;
    }
  }
}
