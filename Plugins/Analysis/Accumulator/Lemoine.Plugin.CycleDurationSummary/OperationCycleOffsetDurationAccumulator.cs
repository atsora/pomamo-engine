// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.CycleDurationSummary
{
  public sealed class OperationCycleOffsetDurationAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 401;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new OperationCycleOffsetDurationAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator for the OperationCycle analysis table
  /// that updates the offset duration
  /// </summary>
  public class OperationCycleOffsetDurationAccumulator
    : Accumulator
    , IOperationCycleAccumulator
  {
    sealed class OperationCycleValue
    {
      readonly IOperation m_initialOperation;
      IOperation m_newOperation;

      /// <summary>
      /// Initial operation
      /// </summary>
      public IOperation InitialOperation
      {
        get { return m_initialOperation; }
      }

      /// <summary>
      /// New operation
      /// </summary>
      public IOperation NewOperation
      {
        get { return m_newOperation; }
        set { m_newOperation = value; }
      }

      /// <summary>
      /// Initial duration
      /// </summary>
      public TimeSpan? InitialDuration { get; private set; }

      /// <summary>
      /// New duration
      /// </summary>
      public TimeSpan? NewDuration { get; set; }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="operationCycle">If not null, it must be initialized (else risk of inefficiency)</param>
      public OperationCycleValue (IOperationCycle operationCycle)
      {
        m_initialOperation = null;
        if (null != operationCycle) {
          if (!ModelDAOHelper.DAOFactory.IsInitialized (operationCycle)) {
            log.ErrorFormat ("OperationCycleValue: operationCycle is not initialized. StackTrace={0}",
              System.Environment.StackTrace);
          }
          if ((null != operationCycle.OperationSlot)) {
            m_initialOperation = operationCycle.OperationSlot.Operation;
          }
          if ((operationCycle.End.HasValue) && (operationCycle.Begin.HasValue)) {
            this.InitialDuration = operationCycle.End.Value.Subtract (operationCycle.Begin.Value);
          }
        }
      }
    };

    #region Members
    readonly IDictionary<OperationCycle, OperationCycleValue> m_operationCycleAccumulator =
      new Dictionary<OperationCycle, OperationCycleValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleOffsetDurationAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCycleOffsetDurationAccumulator ()
    {
    }
    #endregion // Constructors

    #region IOperationCycleAccumulator
    /// <summary>
    /// IOperationCycleAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationCycleUpdated (IOperationCycle before,
                                IOperationCycle after)
    {
      if (null != before) {
        UpdateCycle (before, -1);
      }
      if (null != after) {
        UpdateCycle (after, +1);
      }
    }

    /// <summary>
    /// Add an operation cycle
    /// </summary>
    /// <param name="cycle"></param>
    /// <param name="increment"></param>
    void UpdateCycle (IOperationCycle cycle,
                      int increment)
    {
      if (0 == cycle.Id) {
        return;
      }

      OperationCycle operationCycle = (OperationCycle)cycle;
      OperationCycleValue v;
      if (!m_operationCycleAccumulator.TryGetValue (operationCycle, out v)) {
        m_operationCycleAccumulator[operationCycle] = new OperationCycleValue (cycle);
      }
      else if (0 < increment) {
        if (null != cycle) {
          if (!ModelDAOHelper.DAOFactory.IsInitialized (cycle)) {
            log.ErrorFormat ("UpdateCycle: cycle is not initialized. StackTrace={0}", System.Environment.StackTrace);
          }
        }
        if ((null != cycle) && (null != cycle.OperationSlot)) {
          v.NewOperation = cycle.OperationSlot.Operation;
        }
        else {
          v.NewOperation = null;
        }
        if ((null != cycle) && (cycle.End.HasValue) && (cycle.Begin.HasValue)) {
          v.NewDuration = operationCycle.End.Value.Subtract (operationCycle.Begin.Value);
        }
        else {
          v.NewDuration = null;
        }
      }
      // else do nothing, the cycle is removed
    }
    #endregion // IOperationCycleAccumulator

    #region IAccumulator
    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      foreach (KeyValuePair<OperationCycle, OperationCycleValue> data
               in m_operationCycleAccumulator) {
        if (!object.Equals (data.Value.InitialDuration, data.Value.NewDuration)) {
          OperationCycle cycle = (OperationCycle)data.Key;
          cycle.RecomputeOffsetDuration ();
          cycle.RecomputeBetweenCyclesOffsetDuration ();
        }
        else if (!object.Equals (data.Value.InitialOperation, data.Value.NewOperation)) { // The operation changed
          OperationCycle cycle = (OperationCycle)data.Key;
          // Update the offset duration
          IOperation oldOperation = data.Value.InitialOperation;
          IOperation newOperation = data.Value.NewOperation;
          if ((null == oldOperation) || (null == newOperation)) {
            cycle.RecomputeOffsetDuration ();
            cycle.RecomputeBetweenCyclesOffsetDuration ();
          }
          else { // Both operations are not null
            if (!object.Equals (oldOperation.MachiningDuration, newOperation.MachiningDuration)) {
              cycle.RecomputeOffsetDuration ();
            }
            if (!object.Equals (oldOperation.LoadingDuration, newOperation.LoadingDuration)
                || !object.Equals (oldOperation.UnloadingDuration, newOperation.UnloadingDuration)) {
              cycle.RecomputeBetweenCyclesOffsetDuration ();
            }
          }
        }
      }
      m_operationCycleAccumulator.Clear ();
    }
    #endregion // Methods
  }
}
