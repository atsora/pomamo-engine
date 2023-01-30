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

namespace Lemoine.Plugin.CycleCountSummary
{
  public sealed class AccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 300.0;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new CycleCountSummaryAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator for the CycleCountSummary analysis table
  /// 
  /// This is an operation slot accumulator if the operation slots are split by day and shift,
  /// else this is an operation cycle accumulator
  /// </summary>
  public sealed class CycleCountSummaryAccumulator
    : Accumulator
    , IOperationCycleAccumulator
    , IOperationSlotAccumulator
  {
    sealed class CycleCountSummaryKey
    {
      readonly IMachine m_machine;
      readonly DateTime m_day;
      readonly IShift m_shift;
      readonly IWorkOrder m_workOrder;
      readonly ILine m_line;
      readonly ITask m_task;
      readonly IComponent m_component;
      readonly IOperation m_operation;

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
      /// Constructor
      /// </summary>
      public CycleCountSummaryKey (IMachine machine, DateTime day, IShift shift,
                                   IWorkOrder workOrder, ILine line, ITask task,
                                   IComponent component, IOperation operation)
      {
        m_machine = machine;
        m_day = day;
        m_workOrder = workOrder;
        m_line = line;
        m_task = task;
        m_component = component;
        m_operation = operation;

        if (AnalysisConfigHelper.SplitCycleSummaryByShift) {
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

        CycleCountSummaryKey other = obj as CycleCountSummaryKey;
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
          && object.Equals (m_operation, other.m_operation);
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
          if (null != m_task) {
            hashCode += 1000000021 * m_task.GetHashCode ();
          }
        }
        return hashCode;
      }
    };

    struct CycleCountSummaryValue
    {
      public int Full;
      public int Partial;
    }

    static readonly string DAY_FROM_DATETIME_RANGE_KEY = "Analysis.CycleCountSummaryAccumulator.DayFromDateTimeRange";
    static readonly bool DAY_FROM_DATETIME_RANGE_VALUE = false;

    #region Members
    readonly IDictionary<CycleCountSummaryKey, CycleCountSummaryValue> m_cycleCountSummaryAccumulator =
      new Dictionary<CycleCountSummaryKey, CycleCountSummaryValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CycleCountSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CycleCountSummaryAccumulator ()
    {
    }
    #endregion // Constructors

    #region IAccumulator
    /// <summary>
    /// Is this accumulator an operation cycle accumulator ?
    /// </summary>
    /// <returns></returns>
    public override bool IsOperationCycleAccumulator ()
    {
      return !AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ();
    }
    #endregion // IAccumulator

    #region Methods
    /// <summary>
    /// IOperationCycleAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationCycleUpdated (IOperationCycle before,
                                       IOperationCycle after)
    {
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
        // Use the IOperationSlotAccumulator implementation instead
        return;
      }

      if ((null != before)
          && (null != after)
          && (null != before.OperationSlot)
          && (null != after.OperationSlot)
          && (before.OperationSlot.Id == after.OperationSlot.Id)
          && object.Equals (before.Full, after.Full)) { // Only the day may change
        DateTime? beforeDay = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (before.DateTime);
        DateTime? afterDay = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (after.DateTime);
        if (object.Equals (beforeDay, afterDay)) {
          return;
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
    /// Update the cycle Count of a cycle
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <param name="increment">+1 or -1</param>
    void UpdateCycle (IOperationCycle operationCycle,
                      int increment)
    {
      if (0 == increment) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateCycleCount: " +
                           "increment is 0 => nothing to do");
        }
        return;
      }

      if (!operationCycle.Begin.HasValue && !operationCycle.End.HasValue) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateCycleCount: " +
                           "the operation cycle count has not been fully initialized yet " +
                           "(no begin and no end) " +
                           "=> nothing to do");
        }
        return;
      }

      IOperationSlot operationSlot = operationCycle.OperationSlot;
      if (null == operationSlot) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateCycleCount: " +
                           "operationSlot is null " +
                           "=> skip it");
        }
        return;
      }
      if (!ModelDAOHelper.DAOFactory.IsInitialized (operationCycle.OperationSlot)) {
        operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
      }
      if (null == operationSlot.Operation) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateCycleCount: " +
                           "operation is null " +
                           "=> skip it");
        }
        return;
      }
      Debug.Assert (null != operationSlot);
      Debug.Assert (null != operationSlot.Operation);

      if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
        Debug.Assert (false); // OperationSlotAccumulator used instead
        log.FatalFormat ("UpdateCycle: UpdateCycle called while the option IsSplitByDay is On");
      }
      var day = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay (operationCycle.DateTime);

      IMachine machine = operationCycle.Machine;
      Debug.Assert (null != machine);

      CycleCountSummaryKey key =
        new CycleCountSummaryKey (machine, day,
                                  operationSlot.Shift,
                                  operationSlot.WorkOrder, operationSlot.Line, operationSlot.Task,
                                  operationSlot.Component, operationSlot.Operation);
      CycleCountSummaryValue v;
      if (!m_cycleCountSummaryAccumulator.TryGetValue (key, out v)) {
        v = new CycleCountSummaryValue ();
        if (operationCycle.Full) {
          v.Full = increment;
        }
        else {
          v.Partial = increment;
        }
        m_cycleCountSummaryAccumulator[key] = v;
      }
      else {
        if (operationCycle.Full) {
          v.Full += increment;
        }
        else {
          v.Partial += increment;
        }
        m_cycleCountSummaryAccumulator[key] = v;
      }
    }

    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationSlotUpdated (IOperationSlot before,
                                      IOperationSlot after)
    {
      Debug.Assert (null != after);

      // Try with the day
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
        if (null != before) {
          if (!OperationSlotUpdate (before, -1)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("OperationSlotUpdated: OperationSlotUpdate for operation slot before {0} not processed (no day)", before);
            }
          }
        }
        if (!OperationSlotUpdate (after, +1)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("OperationSlotUpdated: OperationSlotUpdate for operation slot after {0} not processed (no day)", after);
          }
        }
        return;
      }

      if ((null != before)
          && (!object.Equals (before.Component, after.Component)
             || !object.Equals (before.WorkOrder, after.WorkOrder)
             || !object.Equals (before.Line, after.Line)
             || !object.Equals (before.Task, after.Task))) {
        IList<IOperationCycle> cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (after);
        foreach (var cycle in cycles) {
          SetActive ();
          DateTime day = ModelDAOHelper.DAOFactory.DaySlotDAO.GetDay (cycle.DateTime);
          IMachine machine = cycle.Machine;
          Debug.Assert (null != machine);
          { // - Remove the old data
            CycleCountSummaryKey key =
              new CycleCountSummaryKey (machine, day,
                                        before.Shift,
                                        before.WorkOrder, before.Line, before.Task,
                                        before.Component, before.Operation);
            CycleCountSummaryValue v;
            if (!m_cycleCountSummaryAccumulator.TryGetValue (key, out v)) {
              log.ErrorFormat ("OperationSlotUpdated: " +
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
              m_cycleCountSummaryAccumulator[key] = v;
            }
          }
          { // - Add the new data
            CycleCountSummaryKey key =
              new CycleCountSummaryKey (machine, day,
                                        after.Shift,
                                        after.WorkOrder, after.Line, after.Task,
                                        after.Component, after.Operation);
            CycleCountSummaryValue v;
            if (!m_cycleCountSummaryAccumulator.TryGetValue (key, out v)) {
              v = new CycleCountSummaryValue ();
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
        SetActive ();
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
      var processed = OperationSlotUpdate (initialState, -1);
      if (log.IsInfoEnabled && !processed && AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
        log.InfoFormat ("OperationSlotRemoved: called on an initial operation slot {0} without any day",
          initialState.Id);
      }
    }

    /// <summary>
    /// Return false if the day could not be determined
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="multiplicator"></param>
    /// <returns></returns>
    bool OperationSlotUpdate (IOperationSlot operationSlot, int multiplicator)
    {
      if ((null != operationSlot)
          && (null != operationSlot.Operation)) {
        if (operationSlot.Day.HasValue) {
          OperationSlotUpdate (operationSlot, multiplicator, operationSlot.Day.Value);
          return true;
        }

        bool dayFromDateTimeRange = Lemoine.Info.ConfigSet
          .LoadAndGet (DAY_FROM_DATETIME_RANGE_KEY, DAY_FROM_DATETIME_RANGE_VALUE);
        if (dayFromDateTimeRange) {
          // This block is probably not necessary since the day will be set afterwards
          if (operationSlot.DayRange.Lower.HasValue && operationSlot.DayRange.Upper.HasValue
                    && operationSlot.DayRange.Lower.Value.Equals (operationSlot.DayRange.Upper.Value)) { // Unique day
            var day = operationSlot.DayRange.Lower.Value;
            OperationSlotUpdate (operationSlot, multiplicator, day);
            return true;
          }
        }

        if (log.IsDebugEnabled) {
          log.DebugFormat ("OperationSlotUpdate: no day for operationSlot with range {0} dayrange {1} yet",
            operationSlot.DateTimeRange, operationSlot.DayRange);
        }
        return false;
      }
      else { // Nothing to do
        return true;
      }
    }

    void OperationSlotUpdate (IOperationSlot operationSlot, int multiplicator, DateTime day)
    {
      Debug.Assert ((-1 == multiplicator) || (+1 == multiplicator));

      if ((null != operationSlot)
          && (null != operationSlot.Operation)) {
        CycleCountSummaryKey key =
          new CycleCountSummaryKey (operationSlot.Machine,
                                    day, operationSlot.Shift,
                                    operationSlot.WorkOrder, operationSlot.Line,
                                    operationSlot.Task,
                                    operationSlot.Component,
                                    operationSlot.Operation);
        CycleCountSummaryValue v;
        if (!m_cycleCountSummaryAccumulator.TryGetValue (key, out v)) {
          v = new CycleCountSummaryValue ();
          v.Full = operationSlot.TotalCycles * multiplicator;
          v.Partial = operationSlot.PartialCycles * multiplicator;
          m_cycleCountSummaryAccumulator[key] = v;
        }
        else {
          v.Full += operationSlot.TotalCycles * multiplicator;
          v.Partial += operationSlot.PartialCycles * multiplicator;
          m_cycleCountSummaryAccumulator[key] = v;
        }
      }
    }

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_cycleCountSummaryAccumulator
        .Where (d => (0 != d.Value.Full) || (0 != d.Value.Partial));
      foreach (KeyValuePair<CycleCountSummaryKey, CycleCountSummaryValue> cycleCountSummaryData
               in withChangeItems) {
        SetActive ();
        CycleCountSummaryKey key = cycleCountSummaryData.Key;
        (new CycleCountSummaryDAO ())
          .Update (key.Machine,
                   key.Day,
                   key.Shift,
                   key.WorkOrder,
                   key.Line,
                   key.Task,
                   key.Component,
                   key.Operation,
                   cycleCountSummaryData.Value.Full,
                   cycleCountSummaryData.Value.Partial);
      }
      m_cycleCountSummaryAccumulator.Clear ();
      SetActive ();
    }
    #endregion // Methods
  }
}
