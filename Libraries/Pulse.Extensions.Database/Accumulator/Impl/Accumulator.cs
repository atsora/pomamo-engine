// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Pulse.Extensions.Database.Accumulator.Impl
{
  /// <summary>
  /// Description of Accumulator.
  /// </summary>
  public abstract class Accumulator
    : IChecked
    , ICheckedCallers
    , IAccumulator
  {
    IList<IChecked> m_callers = new List<IChecked> ();

    #region ICheckedCallers implementation
    /// <summary>
    /// ICheckedCallers implementation
    /// </summary>
    /// <param name="caller"></param>
    public void AddCheckedCaller (IChecked caller)
    {
      if (!m_callers.Contains (caller)) {
        m_callers.Add (caller);
      }
    }
    #endregion // ICheckedCallers implementation

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void SetActive ()
    {
      foreach (var caller in m_callers) {
        caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      foreach (var caller in m_callers) {
        caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      foreach (var caller in m_callers) {
        caller.ResumeCheck ();
      }
    }
    #endregion // IChecked implementation

    /// <summary>
    /// Store the content of the accumulator in the database
    /// </summary>
    /// <param name="transactionName"></param>
    public abstract void Store (string transactionName);

    /// <summary>
    /// Is the accumulator a reason slot accumulator ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsReasonSlotAccumulator ()
    {
      return (this is IReasonSlotAccumulator);
    }

    /// <summary>
    /// Is the accumulator an operation slot accumulator ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsOperationSlotAccumulator ()
    {
      return (this is IOperationSlotAccumulator);
    }

    /// <summary>
    /// Is the accumulator an operation cycle accumulator ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsOperationCycleAccumulator ()
    {
      return (this is IOperationCycleAccumulator);
    }

    /// <summary>
    /// Is the accumulator an observation state slot accumulator ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsObservationStateSlotAccumulator ()
    {
      return (this is IObservationStateSlotAccumulator);
    }
  }
}
