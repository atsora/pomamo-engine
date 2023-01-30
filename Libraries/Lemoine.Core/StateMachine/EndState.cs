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
  /// End state implementation
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public class EndState<TContext>
    : IState<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (EndState<TContext>).FullName);

    TContext m_context;

    #region Getters / Setters
    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public string Name => "End";

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public string PerfName
    {
      get
      {
        log.Fatal ("PerfName: invalid call, end state");
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public bool End => true;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public TContext Context => m_context;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public IState<TContext> ExceptionState
    {
      get
      {
        log.Fatal ("ExceptionState.get: invalid call for an end state");
        throw new InvalidOperationException ();
      }
      set
      {
        log.Fatal ("ExceptionState.get: invalid call for an end state");
        throw new InvalidOperationException ();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public EndState ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public void Handle (CancellationToken cancellationToken)
    {
      log.Fatal ("PerfName: invalid call, end state");
      throw new InvalidOperationException ();
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    /// <param name="context"></param>
    public void SetContext (TContext context)
    {
      m_context = context;
    }
  }
}
