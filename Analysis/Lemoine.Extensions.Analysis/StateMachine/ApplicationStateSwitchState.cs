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
  /// ApplicationStateSwitchState: switch to a specific state if the application state with the specified key is on
  /// </summary>
  public class ApplicationStateSwitchState<TContext>
    : State<TContext>
    where TContext : IContext<TContext>
  {
    ILog log = LogManager.GetLogger (typeof (ApplicationStateSwitchState<TContext>).FullName);

    readonly string m_name;
    readonly IMachine m_machine;
    readonly string m_key;
    readonly IState<TContext> m_onState;
    readonly IState<TContext> m_detaultState;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    /// <param name="key">not null or empty</param>
    /// <param name="onState">not null</param>
    /// <param name="defaultState">not null</param>
    public ApplicationStateSwitchState (string name, IMachine machine, string key, IState<TContext> onState, IState<TContext> defaultState)
      : base (null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != onState);
      Debug.Assert (null != defaultState);

      m_name = name;
      m_machine = machine;
      m_key = key;
      m_onState = onState;
      m_detaultState = defaultState;

      log = LogManager.GetLogger (typeof (CatchUpSwitchState<TContext>).FullName + "." + m_machine.Id);
    }

    /// <summary>
    /// Constructor with no machine reference
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key">not null or empty</param>
    /// <param name="onState">not null</param>
    /// <param name="defaultState">not null</param>
    public ApplicationStateSwitchState (string name, string key, IState<TContext> onState, IState<TContext> defaultState)
      : base (null)
    {
      Debug.Assert (!string.IsNullOrEmpty (key));
      Debug.Assert (null != onState);
      Debug.Assert (null != defaultState);

      m_name = name;
      m_machine = null;
      m_key = key;
      m_onState = onState;
      m_detaultState = defaultState;

      log = LogManager.GetLogger (typeof (CatchUpSwitchState<TContext>).FullName);
    }

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string Name => m_name;

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override string PerfName => "";

    /// <summary>
    /// <see cref="IState{TContext}"/>
    /// </summary>
    public override void Handle (CancellationToken cancellationToken)
    {
      try {
        bool catchUp;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (m_key);
          catchUp = null != applicationState;
        }

        if (catchUp) {
          this.Context.SwitchTo (m_onState);
        }
        else {
          this.Context.SwitchTo (m_detaultState);
        }
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
