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
  /// ProductionSwitchState: switch to a specific state during production periods
  /// </summary>
  public sealed class ProductionSwitchState<TContext>
    : State<TContext>
    where TContext : IContext<TContext>
  {
    ILog log = LogManager.GetLogger (typeof (ProductionSwitchState<TContext>).FullName);

    readonly string m_name;
    readonly IMachine m_machine;
    readonly IState<TContext> m_productionState;
    readonly IState<TContext> m_notProductionState;

    IState<TContext> m_currentNextState = null;
    UpperBound<DateTime> m_currentLimitDateTime = DateTime.MinValue;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="machine">not null</param>
    /// <param name="productionState">not null</param>
    /// <param name="notProductionState">not null</param>
    public ProductionSwitchState (string name, IMachine machine, IState<TContext> productionState, IState<TContext> notProductionState)
      : base (null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != productionState);
      Debug.Assert (null != notProductionState);

      m_name = name;
      m_machine = machine;
      m_productionState = productionState;
      m_notProductionState = notProductionState;

      log = LogManager.GetLogger (typeof (ProductionSwitchState<TContext>).FullName + "." + m_machine.Id);
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
        var now = DateTime.UtcNow;
        if (now < m_currentLimitDateTime) {
          Debug.Assert (null != m_currentNextState);
          this.Context.SwitchTo (m_currentNextState);
          return;
        }

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var currentObservationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindAt (m_machine, now);
          if (null == currentObservationStateSlot) {
            log.Error ($"Handle: no observation state slot at {now} => switch to the notProductionState");
            this.Context.SwitchTo (m_notProductionState);
            return;
          }
          if (null == currentObservationStateSlot.MachineObservationState) {
            log.Error ($"Handle: null machine observation state at {now} => switch to the productionState (fallback)");
            this.Context.SwitchTo (m_productionState);
            return;
          }
          var isProduction = currentObservationStateSlot.MachineObservationState.IsProduction;
          m_currentNextState = isProduction
            ? m_productionState
            : m_notProductionState;
          m_currentLimitDateTime = currentObservationStateSlot.DateTimeRange.Upper;
          if (log.IsDebugEnabled) {
            log.Debug ($"Handle: production={isProduction}");
          }
          this.Context.SwitchTo (m_currentNextState);
          return;
        }
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
