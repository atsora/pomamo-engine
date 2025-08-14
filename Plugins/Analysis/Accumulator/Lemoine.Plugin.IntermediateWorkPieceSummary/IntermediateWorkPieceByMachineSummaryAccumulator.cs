// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Linq;
using Pulse.Extensions.Database.Accumulator;
using Pulse.Extensions.Database.Accumulator.Impl;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public sealed class IntermediateWorkPieceByMachineSummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 250;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new IntermediateWorkPieceByMachineSummaryAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator to update the IntermediateWorkPieceByMachine analysis table
  /// </summary>
  public class IntermediateWorkPieceByMachineSummaryAccumulator
    : Accumulator
    , IOperationSlotAccumulator
  {
    sealed class IntermediateWorkPieceByMachineSummaryKey
    {
      readonly IMachine m_machine;
      readonly IIntermediateWorkPiece m_intermediateWorkPiece;
      readonly IComponent m_component;
      readonly IWorkOrder m_workOrder;
      readonly ILine m_line;
      readonly IManufacturingOrder m_manufacturingOrder;
      readonly DateTime? m_day;
      readonly IShift m_shift;

      /// <summary>
      /// Machine
      /// </summary>
      public IMachine Machine
      {
        get { return m_machine; }
      }

      /// <summary>
      /// Intermediate work piece
      /// </summary>
      public IIntermediateWorkPiece IntermediateWorkPiece
      {
        get { return m_intermediateWorkPiece; }
      }

      /// <summary>
      /// Component
      /// </summary>
      public IComponent Component
      {
        get { return m_component; }
      }

      /// <summary>
      /// Work order
      /// </summary>
      public IWorkOrder WorkOrder
      {
        get { return m_workOrder; }
      }

      /// <summary>
      /// Line
      /// </summary>
      public ILine Line
      {
        get { return m_line; }
      }

      /// <summary>
      /// Manufacturing order
      /// </summary>
      public IManufacturingOrder ManufacturingOrder
      {
        get { return m_manufacturingOrder; }
      }

      /// <summary>
      /// Day
      /// </summary>
      public DateTime? Day
      {
        get { return m_day; }
      }

      /// <summary>
      /// Shift
      /// </summary>
      public IShift Shift
      {
        get { return m_shift; }
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="machine"></param>
      /// <param name="intermediateWorkPiece"></param>
      /// <param name="component"></param>
      /// <param name="workOrder"></param>
      /// <param name="line"></param>
      /// <param name="manufacturingOrder"></param>
      /// <param name="day"></param>
      /// <param name="shift"></param>
      public IntermediateWorkPieceByMachineSummaryKey (IMachine machine,
                                                       IIntermediateWorkPiece intermediateWorkPiece, IComponent component, IWorkOrder workOrder,
                                                       ILine line, IManufacturingOrder manufacturingOrder,
                                                       DateTime? day, IShift shift)
      {
        m_machine = machine;
        m_intermediateWorkPiece = intermediateWorkPiece;
        m_component = component;
        m_workOrder = workOrder;
        m_line = line;
        m_manufacturingOrder = manufacturingOrder;

        if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByShift ()) {
          m_day = day;
          m_shift = shift;
        }
        else if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
          m_day = day;
        }
      }

      /// <summary>
      ///   Determines whether the specified Object
      ///   is equal to the current Object
      /// </summary>
      /// <param name="obj">The object to compare with the current object</param>
      /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
      public override bool Equals (object obj)
      {
        if (object.ReferenceEquals (this, obj)) {
          return true;
        }

        IntermediateWorkPieceByMachineSummaryKey other = obj as IntermediateWorkPieceByMachineSummaryKey;
        if (null == other) {
          return false;
        }

        return m_machine.Equals (other.m_machine)
          && m_intermediateWorkPiece.Equals (other.m_intermediateWorkPiece)
          && object.Equals (m_component, other.m_component)
          && object.Equals (m_workOrder, other.m_workOrder)
          && object.Equals (m_line, other.m_line)
          && object.Equals (m_manufacturingOrder, other.m_manufacturingOrder)
          && object.Equals (m_day, other.m_day)
          && object.Equals (m_shift, other.m_shift);
      }

      /// <summary>
      ///   Serves as a hash function for a particular type
      /// </summary>
      /// <returns>A hash code for the current Object</returns>
      public override int GetHashCode ()
      {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * m_machine.GetHashCode ();
          hashCode += 1000000009 * m_intermediateWorkPiece.GetHashCode ();
          if (null != m_component) {
            hashCode += 1000000011 * m_component.GetHashCode ();
          }
          if (null != m_workOrder) {
            hashCode += 1000000013 * m_workOrder.GetHashCode ();
          }
          if (null != m_line) {
            hashCode += 1000000017 * m_line.GetHashCode ();
          }
          if (m_day.HasValue) {
            hashCode += 1000000019 * m_day.Value.GetHashCode ();
          }
          if (null != m_shift) {
            hashCode += 1000000021 * m_shift.GetHashCode ();
          }
          if (null != m_manufacturingOrder) {
            hashCode += 1000000023 * m_manufacturingOrder.GetHashCode ();
          }
        }
        return hashCode;
      }

      /// <summary>
      /// Override ToString()
      /// </summary>
      /// <returns></returns>
      public override string ToString ()
      {
        return string.Format ("[IntermediateWorkPieceByMachineSummaryKey Machine={0}, IntermediateWorkPiece={1}, Component={2}, WorkOrder={3}, Line={4}, ManufacturingOrder={5}, Day={6}, Shift={7}]",
                             m_machine, m_intermediateWorkPiece, m_component, m_workOrder, m_line, m_manufacturingOrder, m_day, m_shift);
      }
    };

    struct IntermediateWorkPieceByMachineSummaryValue
    {
      public int TotalCycles;
      public int AdjustedCycles;
      public int AdjustedQuantity;
    }

    #region Members
    IDictionary<IntermediateWorkPieceByMachineSummaryKey, IntermediateWorkPieceByMachineSummaryValue> m_intermediateWorkPieceByMachineSummaryAccumulator =
      new Dictionary<IntermediateWorkPieceByMachineSummaryKey, IntermediateWorkPieceByMachineSummaryValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceByMachineSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public IntermediateWorkPieceByMachineSummaryAccumulator ()
    {
    }
    #endregion // Constructors

    #region IOperationSlotAccumulator
    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after">not null</param>
    public void OperationSlotUpdated (IOperationSlot before,
                                      IOperationSlot after)
    {
      Debug.Assert (null != after);

      if ((null == before)
          || !object.Equals (before.Operation, after.Operation)
          || !object.Equals (before.Component, after.Component)
          || !object.Equals (before.WorkOrder, after.WorkOrder)
          || !object.Equals (before.Line, after.Line)
          || !object.Equals (before.ManufacturingOrder, after.ManufacturingOrder)
          || !object.Equals (before.Day, after.Day)
          || !object.Equals (before.Shift, after.Shift)
          || !object.Equals (before.TotalCycles, after.TotalCycles)
          || !object.Equals (before.AdjustedCycles, after.AdjustedCycles)
          || !object.Equals (before.AdjustedQuantity, after.AdjustedQuantity)) { // Some changes
        if (null != before) {
          OperationSlotUpdate (before, -1);
        }
        if (null != after) {
          OperationSlotUpdate (after, +1);
        }
      }
    }

    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="initialState"></param>
    public void OperationSlotRemoved (IOperationSlot operationSlot,
                                      IOperationSlot initialState)
    {
      OperationSlotUpdate (initialState, -1);
    }

    void OperationSlotUpdate (IOperationSlot operationSlot, int multiplicator)
    {
      if ((null != operationSlot)
          && (null != operationSlot.Operation)) {
        foreach (IIntermediateWorkPiece intermediateWorkPiece in operationSlot.Operation.IntermediateWorkPieces) {
          IntermediateWorkPieceByMachineSummaryKey key =
            new IntermediateWorkPieceByMachineSummaryKey (operationSlot.Machine, intermediateWorkPiece,
                                                           operationSlot.Component, operationSlot.WorkOrder,
                                                           operationSlot.Line, operationSlot.ManufacturingOrder,
                                                           operationSlot.Day, operationSlot.Shift);
          IntermediateWorkPieceByMachineSummaryValue v;
          if (!m_intermediateWorkPieceByMachineSummaryAccumulator.TryGetValue (key, out v)) {
            v.TotalCycles = operationSlot.TotalCycles * multiplicator;
            v.AdjustedCycles = operationSlot.AdjustedCycles * multiplicator;
            v.AdjustedQuantity = operationSlot.AdjustedQuantity * multiplicator;
          }
          else {
            v.TotalCycles += operationSlot.TotalCycles * multiplicator;
            v.AdjustedCycles += operationSlot.AdjustedCycles * multiplicator;
            v.AdjustedQuantity += operationSlot.AdjustedQuantity * multiplicator;
          }
          m_intermediateWorkPieceByMachineSummaryAccumulator[key] = v;
        }
      }
    }
    #endregion // IOperationSlotAccumulator

    #region Methods
    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_intermediateWorkPieceByMachineSummaryAccumulator
        .Where (d => (d.Value.TotalCycles != d.Value.AdjustedCycles) || (0 != d.Value.AdjustedQuantity));
      foreach (KeyValuePair<IntermediateWorkPieceByMachineSummaryKey, IntermediateWorkPieceByMachineSummaryValue> data
               in withChangeItems) {
        IntermediateWorkPieceByMachineSummaryKey key = data.Key;
        IIntermediateWorkPieceByMachineSummary summary = new IntermediateWorkPieceByMachineSummaryDAO ()
          .FindByKey (key.Machine, key.IntermediateWorkPiece, key.Component, key.WorkOrder, key.Line, key.ManufacturingOrder, key.Day, key.Shift);
        IntermediateWorkPieceByMachineSummaryValue v = data.Value;
        int offset = (v.TotalCycles - v.AdjustedCycles) * key.IntermediateWorkPiece.OperationQuantity + v.AdjustedQuantity;
        if (0 != offset) {
          if (null == summary) {
            summary = new IntermediateWorkPieceByMachineSummary (key.Machine,
                                                            key.IntermediateWorkPiece,
                                                            key.Component,
                                                            key.WorkOrder,
                                                            key.Line,
                                                            key.ManufacturingOrder,
                                                            key.Day,
                                                            key.Shift);
            new IntermediateWorkPieceByMachineSummaryDAO ()
              .MakePersistent (summary);
          }
          summary.Counted += offset;
          if (log.IsWarnEnabled && (summary.Corrected + offset < 0)) {
            log.Warn ($"Store: Corrected is getting a negative value {summary.Corrected + offset} after {summary.Corrected} on {key}");
          }
          summary.Corrected += offset;
          if (summary.IsEmpty ()) {
            log.InfoFormat ("Store: " +
                            "{0} is empty, delete it",
                            summary);
            new IntermediateWorkPieceByMachineSummaryDAO ()
              .MakeTransient (summary);
          }
          else { // Just in case...
            new IntermediateWorkPieceByMachineSummaryDAO ()
              .MakePersistent (summary);
          }
        }
      }
      m_intermediateWorkPieceByMachineSummaryAccumulator.Clear ();
    }
    #endregion // Methods
  }
}
