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
using System.Linq;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;

namespace Lemoine.Plugin.CycleDurationSummary
{
  public sealed class CycleDurationSummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 402;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new CycleDurationSummaryAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator for the CycleDurationSummary analysis table
  /// </summary>
  public class CycleDurationSummaryAccumulator
    : Accumulator
    , IOperationCycleAccumulator
    , IOperationSlotAccumulator
  {
    sealed class CycleDurationSummaryKey
    {
      readonly IMachine m_machine;
      readonly DateTime m_day;
      readonly IShift m_shift;
      readonly IWorkOrder m_workOrder;
      readonly ILine m_line;
      readonly ITask m_task;
      readonly IComponent m_component;
      readonly IOperation m_operation;
      readonly int m_offset;

      /// <summary>
      /// Machine
      /// </summary>
      public IMachine Machine
      {
        get { return m_machine; }
      }

      /// <summary>
      /// Day
      /// </summary>
      public DateTime Day
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
      /// Task
      /// </summary>
      public ITask Task
      {
        get { return m_task; }
      }

      /// <summary>
      /// Component
      /// </summary>
      public IComponent Component
      {
        get { return m_component; }
      }

      /// <summary>
      /// Operation
      /// </summary>
      public IOperation Operation
      {
        get { return m_operation; }
      }

      /// <summary>
      /// Offset
      /// </summary>
      public int Offset
      {
        get { return m_offset; }
      }

      /// <summary>
      /// Constructor
      /// </summary>
      public CycleDurationSummaryKey (IMachine machine, DateTime day, IShift shift,
                                      IWorkOrder workOrder, ILine line, ITask task,
                                      IComponent component, IOperation operation,
                                      int offset)
      {
        m_machine = machine;
        m_day = day;
        m_workOrder = workOrder;
        m_line = line;
        m_task = task;
        m_component = component;
        m_operation = operation;
        m_offset = offset;

        if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByShift () && AnalysisConfigHelper.SplitCycleSummaryByShift) {
          m_shift = shift;
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

        CycleDurationSummaryKey other = obj as CycleDurationSummaryKey;
        if (null == other) {
          return false;
        }

        return m_machine.Equals (other.m_machine)
          && m_day.Equals (other.m_day)
          && object.Equals (m_shift, other.m_shift)
          && object.Equals (m_workOrder, other.m_workOrder)
          && object.Equals (m_line, other.m_line)
          && object.Equals (m_task, other.m_task)
          && object.Equals (m_component, other.m_component)
          && object.Equals (m_operation, other.m_operation)
          && m_offset.Equals (other.m_offset);
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
          hashCode += 1000000009 * m_day.GetHashCode ();
          if (null != m_shift) {
            hashCode += 1000000011 * m_shift.GetHashCode ();
          }
          if (null != m_workOrder) {
            hashCode += 1000000013 * m_workOrder.GetHashCode ();
          }
          if (null != m_line) {
            hashCode += 1000000015 * m_line.GetHashCode ();
          }
          if (null != m_component) {
            hashCode += 1000000017 * m_component.GetHashCode ();
          }
          if (null != m_operation) {
            hashCode += 1000000019 * m_operation.GetHashCode ();
          }
          hashCode += 1000000021 * m_offset.GetHashCode ();
          if (null != m_task) {
            hashCode += 1000000023 * m_task.GetHashCode ();
          }
        }
        return hashCode;
      }
    };

    struct CycleDurationSummaryValue
    {
      public int Full;
      public int Partial;
    }

    #region Members
    IDictionary<CycleDurationSummaryKey, CycleDurationSummaryValue> m_cycleDurationSummaryAccumulator =
      new Dictionary<CycleDurationSummaryKey, CycleDurationSummaryValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CycleDurationSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CycleDurationSummaryAccumulator ()
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
      if ((null != before)
          && (null != after)
          && (null != before.OperationSlot)
          && (null != after.OperationSlot)
          && (before.OperationSlot.Id == after.OperationSlot.Id)
          && object.Equals (before.Full, after.Full)
          && object.Equals (before.OffsetDuration, after.OffsetDuration)) { // No change, nothing to do
        if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
          return;
        }
        else { // The day may change
          DateTime? beforeDay = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (before.DateTime);
          DateTime? afterDay = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (after.DateTime);
          if (object.Equals (beforeDay, afterDay)) {
            return;
          }
        }
      }
      if (null != before) {
        UpdateCycle (before, -1);
      }
      if (null != after) {
        UpdateCycle (after, +1);
      }
    }

    /// <summary>
    /// Update the cycle count of a cycle
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <param name="increment"></param>
    public void UpdateCycle (IOperationCycle operationCycle,
                             int increment)
    {
      if (0 == increment) {
        log.DebugFormat ("UpdateCycleCount: " +
                         "increment is 0 => nothing to do");
        return;
      }

      double? offsetDuration = operationCycle.OffsetDuration;
      if (!offsetDuration.HasValue) {
        log.DebugFormat ("UpdateCycleCount: " +
                         "offsetDuration is null => nothing to do");
        return;
      }

      if (!operationCycle.Begin.HasValue && !operationCycle.End.HasValue) {
        log.DebugFormat ("UpdateCycleCount: " +
                         "the operation cycle count has not been fully initialized yet " +
                         "(no begin and no end) " +
                         "=> nothing to do");
        return;
      }

      IOperationSlot operationSlot = operationCycle.OperationSlot;
      if (null == operationSlot) {
        log.DebugFormat ("UpdateCycleCount: " +
                         "operationSlot is null " +
                         "=> skip it");
        return;
      }
      if (!ModelDAOHelper.DAOFactory.IsInitialized (operationSlot)) {
        operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindById (operationSlot.Id, operationCycle.Machine);
      }
      if (null == operationSlot.Operation) {
        log.DebugFormat ("UpdateCycleCount: " +
                         "operation is null " +
                         "=> skip it");
        return;
      }
      Debug.Assert (null != operationSlot);
      Debug.Assert (null != operationSlot.Operation);

      DateTime day = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay (operationCycle.DateTime);

      IMachine machine = operationCycle.Machine;
      Debug.Assert (null != machine);

      CycleDurationSummaryKey key =
        new CycleDurationSummaryKey (machine, day,
                                     operationSlot.Shift,
                                     operationSlot.WorkOrder, operationSlot.Line, operationSlot.Task,
                                     operationSlot.Component, operationSlot.Operation,
                                     (int)Math.Round (offsetDuration.Value));
      CycleDurationSummaryValue v;
      if (!m_cycleDurationSummaryAccumulator.TryGetValue (key, out v)) {
        v = new CycleDurationSummaryValue ();
        if (operationCycle.Full) {
          v.Full = increment;
        }
        else {
          v.Partial = increment;
        }
      }
      else {
        if (operationCycle.Full) {
          v.Full += increment;
        }
        else {
          v.Partial += increment;
        }
      }
      m_cycleDurationSummaryAccumulator[key] = v;
    }
    #endregion // IOperationCycleAccumulator

    #region IOperationSlotAccumulator
    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationSlotUpdated (IOperationSlot before,
                                      IOperationSlot after)
    {
      Debug.Assert (null != after);

      if ((null != before)
          && (!object.Equals (before.Component, after.Component)
              || !object.Equals (before.WorkOrder, after.WorkOrder)
              || !object.Equals (before.Line, after.Line)
              || !object.Equals (before.Task, after.Task))) {
        IList<IOperationCycle> cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (after);
        foreach (var cycle in cycles) {
          double? offsetDuration = cycle.OffsetDuration;
          if (!offsetDuration.HasValue) {
            continue;
          }
          if (!cycle.Begin.HasValue && !cycle.End.HasValue) {
            continue;
          }
          DateTime day = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (cycle.DateTime);
          IMachine machine = cycle.Machine;
          Debug.Assert (null != machine);
          { // - Remove the old data
            CycleDurationSummaryKey key =
              new CycleDurationSummaryKey (machine, day,
                                           before.Shift,
                                           before.WorkOrder, before.Line, before.Task,
                                           before.Component, before.Operation,
                                           (int)Math.Round (offsetDuration.Value));
            CycleDurationSummaryValue v;
            if (!m_cycleDurationSummaryAccumulator.TryGetValue (key, out v)) {
              log.ErrorFormat ("AfterOperationSlotUpdate: " +
                               "no summary found for cycle {0}, which is not expected",
                               cycle);
            }
            else {
              if (cycle.Full) {
                v.Full -= 1;
              }
              else {
                v.Partial -= 1;
              }
              m_cycleDurationSummaryAccumulator[key] = v;
            }
          }
          { // - Add the new data
            CycleDurationSummaryKey key =
              new CycleDurationSummaryKey (machine, day,
                                           after.Shift,
                                           after.WorkOrder, after.Line, after.Task,
                                           after.Component, after.Operation,
                                           (int)Math.Round (offsetDuration.Value));
            CycleDurationSummaryValue v;
            if (!m_cycleDurationSummaryAccumulator.TryGetValue (key, out v)) {
              v = new CycleDurationSummaryValue ();
              if (cycle.Full) {
                v.Full = 1;
              }
              else {
                v.Partial = 1;
              }
            }
            else {
              if (cycle.Full) {
                v.Full += 1;
              }
              else {
                v.Partial += 1;
              }
            }
          }
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
    }
    #endregion // IOperationSlotAccumulator

    #region IAccumulator
    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_cycleDurationSummaryAccumulator
        .Where (d => (0 != d.Value.Full) || (0 != d.Value.Partial));
      foreach (KeyValuePair<CycleDurationSummaryKey, CycleDurationSummaryValue> cycleDurationSummaryData
               in withChangeItems) {
        CycleDurationSummaryKey key = cycleDurationSummaryData.Key;
        (new CycleDurationSummaryDAO ())
          .UpdateDay (key.Machine,
                      key.Day,
                      key.Shift,
                      key.WorkOrder,
                      key.Line,
                      key.Task,
                      key.Component,
                      key.Operation,
                      key.Offset,
                      cycleDurationSummaryData.Value.Full,
                      cycleDurationSummaryData.Value.Partial);
      }
      m_cycleDurationSummaryAccumulator.Clear ();
    }
    #endregion // IAccumulator
  }
}
