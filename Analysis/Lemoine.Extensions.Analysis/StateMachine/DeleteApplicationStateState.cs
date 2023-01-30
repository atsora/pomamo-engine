// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.StateMachine;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// DeleteApplicationStateState: delete an application state spefifying its key
  /// </summary>
  public sealed class DeleteApplicationStateState<TContext>
    : State<TContext>
    where TContext : IContext<TContext>
  {
    ILog log = LogManager.GetLogger (typeof (DeleteApplicationStateState<TContext>).FullName);

    readonly string m_name;
    readonly IMachine m_machine;
    readonly string m_key;
    readonly IState<TContext> m_nextState;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    /// <param name="key">not null or empty</param>
    /// <param name="nextState">not null</param>
    public DeleteApplicationStateState (string name, IMachine machine, string key, IState<TContext> nextState)
      : base (null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != nextState);

      m_name = name;
      m_machine = machine;
      m_key = key;
      m_nextState = nextState;

      log = LogManager.GetLogger (typeof (DeleteApplicationStateState<TContext>).FullName + "." + m_machine.Id);
    }

    /// <summary>
    /// Constructor with no reference to a machine
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key">not null or empty</param>
    /// <param name="nextState">not null</param>
    public DeleteApplicationStateState (string name, string key, IState<TContext> nextState)
      : base (null)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != nextState);

      m_name = name;
      m_machine = null;
      m_key = key;
      m_nextState = nextState;

      log = LogManager.GetLogger (typeof (DeleteApplicationStateState<TContext>).FullName);
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Analysis.DeleteApplicationStateState")) {
            var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .GetApplicationState (m_key);
            if (null != applicationState) {
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakeTransient (applicationState);
            }
            transaction.Commit ();
          }
        }

        this.Context.SwitchTo (m_nextState);
        return;
      }
      catch (OperationCanceledException ex) {
        log.Error ("Handle: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        log.Error ("Handle: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        log.Fatal ("Handle: unexpected error", ex);
        throw;
      }
    }
    #endregion // Constructors

  }
}
