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
using Lemoine.Core.Performance;

namespace Lemoine.Core.StateMachine
{
  /// <summary>
  /// StateMachine
  /// </summary>
  /// <typeparam name="TContext"></typeparam>
  public class StateMachine<TContext>
    : IContext<TContext>, IStateMachine<TContext>
    where TContext : IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (StateMachine<TContext>).FullName);

    readonly TContext m_context;
    readonly IState<TContext> m_initialState;
    DateTime? m_startDateTime;
    IState<TContext> m_state;

    /// <summary>
    /// Initial state
    /// </summary>
    public IState<TContext> InitialState => m_initialState;

    /// <summary>
    /// <see cref="IContext"/>
    /// </summary>
    public DateTime StateMachineStartDateTime => m_startDateTime.Value;

    /// <summary>
    /// <see cref="IContext"/>
    /// </summary>
    public TimeSpan MaxTime => m_context.MaxTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">not null</param>
    /// <param name="initialState">not null</param>
    public StateMachine (TContext context, IState<TContext> initialState)
    {
      Debug.Assert (null != context);
      Debug.Assert (null != initialState);

      m_context = context;
      m_initialState = initialState;
    }

    /// <summary>
    /// <see cref="IContext{TContext}"/>
    /// </summary>
    /// <param name="state"></param>
    public void SwitchTo (IState<TContext> state)
    {
      m_state = state;
      state.SetContext (m_context);
    }

    /// <summary>
    /// Switch to the inital state of the state machine
    /// </summary>
    void SwitchToInitialState ()
    {
      m_context.SwitchTo (this.InitialState);
    }

    /// <summary>
    /// Switch to an end state
    /// 
    /// <see cref="IContext"/>
    /// </summary>
    public void SwitchToEndState ()
    {
      m_context.SwitchTo (new EndState<TContext> ());
    }

    /// <summary>
    /// Is an exception state set in the current state ?
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsExceptionState ()
    {
      Debug.Assert (null != m_state);

      return null != m_state.ExceptionState;
    }

    /// <summary>
    /// Switch to the exception state of the current state
    /// </summary>
    protected virtual void SwitchToExceptionState ()
    {
      Debug.Assert (null != m_state);
      Debug.Assert (null != m_state.ExceptionState);

      m_context.SwitchTo (m_state.ExceptionState);
    }

    /// <summary>
    /// Run the state machine
    /// 
    /// Suppose the internal properties are already correctly initialized
    /// </summary>
    /// <returns>if true is returned, any other process can continue (kind of completed)</returns>
    public bool Run (CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested ();
      try {
        m_startDateTime = DateTime.UtcNow;
        this.SwitchToInitialState ();
        while (!m_state.End) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Run: current state is {m_state.Name}");
          }
          cancellationToken.ThrowIfCancellationRequested ();
          if (!RunState (m_state, cancellationToken)) {
            if (log.IsInfoEnabled) {
              log.Info ($"Run: RunState returned false, switch to an end state");
            }
            cancellationToken.ThrowIfCancellationRequested ();
            SwitchToEndState ();
            return false;
          }
        } // while
      }
      catch (OperationCanceledException ex) {
        log.Warn ($"Run: OperationCanceledException raised", ex);
        throw;
      }
      catch (Exception ex) {
        log.Error ($"Run: RunState returned an exception", ex);
        throw;
      }
      finally {
        cancellationToken.ThrowIfCancellationRequested ();
        this.SwitchToInitialState ();
        try {
          EndStateMachine ();
        }
        catch (Exception ex) {
          log.Error ($"Run: EndStateMachine return an exception", ex);
          throw;
        }
        finally {
          m_startDateTime = null;
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ("Run: completed");
      }
      return true;
    }

    /// <summary>
    /// Get the PerfTracker key
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected virtual string GetPerfTrackerKey (IState<TContext> state)
    {
      return state.PerfName;
    }

    /// <summary>
    /// Execute the current state
    /// </summary>
    /// <returns></returns>
    protected virtual bool RunState (IState<TContext> state, CancellationToken cancellationToken)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"RunState: state={state.Name}");
      }
      cancellationToken.ThrowIfCancellationRequested ();
      try {
        var perfTrackerKey = GetPerfTrackerKey (state);
        if (string.IsNullOrEmpty (perfTrackerKey)) {
          state.Handle (cancellationToken);
        }
        else {
          using (var perfTracker = new PerfTracker (perfTrackerKey)) {
            state.Handle (cancellationToken);
          }
        }
      }
      catch (Lemoine.Threading.AbortException ex) {
        log.Error ($"RunState: AbortException in {state.Name}", ex);
        throw;
      }
      catch (OperationCanceledException ex) {
        log.Warn ($"RunState: OperationCanceledException in {state.Name}", ex);
        throw;
      }
      catch (Exception ex) {
        log.Exception (ex, $"RunState: exception in {state.Name}");
        return ManageException (ex);
      }
      cancellationToken.ThrowIfCancellationRequested ();
      return true;
    }

    /// <summary>
    /// Manage an exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns>continue if true, interrupt if false</returns>
    protected virtual bool ManageException (Exception ex)
    {
      if (!IsExceptionState ()) {
        log.Error ($"ManageException: no exception state is defined => throw an exception", ex);
        throw new Exception ("No exception state", ex);
      }
      log.Info ($"ManageException: switch to the exception state", ex);
      this.SwitchToExceptionState ();
      return true;
    }

    /// <summary>
    /// Run once the state machine reached a final state or was interrupted
    /// </summary>
    protected virtual void EndStateMachine ()
    {
    }
  }
}
