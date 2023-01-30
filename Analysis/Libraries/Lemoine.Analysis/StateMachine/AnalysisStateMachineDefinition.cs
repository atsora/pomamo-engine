// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Core.StateMachine;
using Lemoine.Extensions.Analysis.StateMachine;

namespace Lemoine.Analysis.StateMachine
{
  /// <summary>
  /// AnalysisStateMachineDefinition
  /// </summary>
  internal class AnalysisStateMachineDefinition<TContext>
    : IAdvancedStateMachineDefinition<TContext>
    where TContext: IContext, IContext<TContext>
  {
    readonly ILog log = LogManager.GetLogger (typeof (AnalysisStateMachineDefinition<TContext>).FullName);

    #region Getters / Setters
    readonly IState<TContext> m_initialState;
    readonly IStateMachineAnalysis m_analysis;
    readonly bool m_throwStateException;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="initialState">not null</param>
    /// <param name="analysis">not null</param>
    /// <param name="throwStateException"></param>
    public AnalysisStateMachineDefinition (IState<TContext> initialState, IStateMachineAnalysis analysis, bool throwStateException = false)
    {
      Debug.Assert (null != initialState);
      Debug.Assert (null != analysis);

      m_initialState = initialState;
      m_analysis = analysis;
      m_throwStateException = throwStateException;
    }

    /// <summary>
    /// <see cref="IAdvancedStateMachineDefinition{TContext}"/>
    /// </summary>
    public IState<TContext> InitialState => m_initialState;

    /// <summary>
    /// <see cref="IAdvancedStateMachineDefinition{TContext}"/>
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool EndStateExcecution (IState<TContext> state)
    {
      m_analysis.SetActive ();
      if (m_analysis.IsPauseRequested ()) {
        if (log.IsInfoEnabled) {
          log.Info ($"EndStateExecution: interrupt the analysis after {state.Name} because the modification analysis Id {m_analysis.GetPauseTriggeringModificationId ()} is run on the same monitored machine");
        }
        return false;
      }
      return true;
    }

    /// <summary>
    /// <see cref="IAdvancedStateMachineDefinition{TContext}"/>
    /// </summary>
    public void EndStateMachine ()
    {
      var stateExceptions = m_analysis.StateExceptions;
      if (stateExceptions.Any ()) {
        if (log.IsErrorEnabled) {
          log.Error ($"EndStateMachine: {stateExceptions.Count ()} exceptions");
          foreach (var ex in stateExceptions) {
            log.Exception (ex, "EndStateMachine");
          }
        } // IsErrorEnabled
        if (stateExceptions.Any (ex => m_analysis.IsExitRequired (ex))) {
          var requiresToExitException = stateExceptions.First (ex => m_analysis.IsExitRequired (ex));
          throw new Exception ("Exception requires to exit in MakeAnalysis", requiresToExitException);
        }
        else {
          if (m_throwStateException) {
            log.Error ($"EndStateMachine: throw the first exception", stateExceptions.First ());
            throw new Exception ("Exception in MakeAnalysis (inner is first)", stateExceptions.First ());
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"EndStateMachine: {stateExceptions.Count ()} exceptions but return true");
            }
          }
        }
      } // exceptions.Any ()
    }

    public string GetPerfTrackerKey (Core.StateMachine.IState<TContext> state)
    {
      if (string.IsNullOrEmpty (state.PerfName)) {
        return null;
      }
      else {
        return $"Analysis.{state.PerfName}{m_analysis.PerfSuffix}";
      }
    }

    public bool ManageException (Exception ex)
    {
      if (ex is InterruptException) {
        if (log.IsInfoEnabled) {
          log.Info ("ManageException: interrupt the activity analysis because it was requested by an Interrupt exception", ex);
        }
        return false;
      }

      if (ex is OutOfMemoryException) {
        log.Error ("ManageException: OutOfMemoryException, give up", ex);
        m_analysis.SetExitRequested ();
        throw new OutOfMemoryException ("Out of memory exception", ex);
      }

      if (m_analysis.IsExitRequired (ex)) {
        log.Error ("ManageException: exit required", ex);
        throw new Lemoine.Threading.AbortException ("Exit is required.", ex);
      }

      m_analysis.AddStateException (ex);
      return true;
    }
    #endregion // Constructors
  }
}
