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
  /// CatchUpSwitchState: switch to a specific state if the catch-up mode is on
  /// </summary>
  public sealed class CatchUpSwitchState<TContext>
    : ApplicationStateSwitchState<TContext>
    where TContext : IContext<TContext>
  {
    ILog log = LogManager.GetLogger (typeof (CatchUpSwitchState<TContext>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    /// <param name="catchupState">not null</param>
    /// <param name="defaultState">not null</param>
    public CatchUpSwitchState (string name, IMachine machine, IState<TContext> catchupState, IState<TContext> defaultState)
      : base (name, machine, $"Analysis.CatchUp.{machine.Id}", catchupState, defaultState)
    {
      log = LogManager.GetLogger (typeof (CatchUpSwitchState<TContext>).FullName + "." + machine.Id);
    }

    /// <summary>
    /// Constructor for global catch up
    /// </summary>
    /// <param name="name"></param>
    /// <param name="catchupState">not null</param>
    /// <param name="defaultState">not null</param>
    public CatchUpSwitchState (string name, IState<TContext> catchupState, IState<TContext> defaultState)
      : base (name, $"Analysis.CatchUp.g", catchupState, defaultState)
    {
      log = LogManager.GetLogger (typeof (CatchUpSwitchState<TContext>).FullName);
    }
    #endregion // Constructors

  }
}
