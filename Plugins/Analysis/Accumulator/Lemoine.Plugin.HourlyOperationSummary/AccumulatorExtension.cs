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
using Lemoine.Collections;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  public sealed class AccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 300.0;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new HourlyOperationSummaryAccumulator ();
    }
  }

  public static class OperationCycleExtensions
  {
    public static DateTime GetLocalDateHour (this IOperationCycle operationCycle)
    {
      return operationCycle.DateTime.ToLocalDateHour ();
    }
  }

  public sealed class HourlyOperationSummaryAccumulator
    : Accumulator
    , IOperationCycleAccumulator
    , IOperationSlotAccumulator
  {
    sealed class HourlyOperationSummaryKey
    {
      readonly IMachine m_machine;
      readonly IOperation m_operation;
      readonly IComponent m_component;
      readonly IWorkOrder m_workOrder;
      readonly ILine m_line;
      readonly IManufacturingOrder m_manufacturingOrder;
      readonly DateTime? m_day;
      readonly IShift m_shift;
      readonly DateTime m_localDateHour;

      public IMachine Machine => m_machine;
      public IOperation Operation => m_operation;
      public IComponent Component => m_component;
      public IWorkOrder WorkOrder => m_workOrder;
      public ILine Line => m_line;
      public IManufacturingOrder ManufacturingOrder => m_manufacturingOrder;
      public DateTime? Day => m_day;
      public IShift Shift => m_shift;
      public DateTime LocalDateHour => m_localDateHour;

      /// <summary>
      /// Constructor
      /// </summary>
      public HourlyOperationSummaryKey (IMachine machine, IOperation operation, IComponent component, IWorkOrder workOrder, ILine line, IManufacturingOrder manufacturingOrder, DateTime? day, IShift shift, DateTime localDateHour)
      {
        Debug.Assert (null != machine);
        Debug.Assert (null != operation);

        m_machine = machine;
        m_operation = operation;
        m_component = component;
        m_workOrder = workOrder;
        m_line = line;
        m_manufacturingOrder = manufacturingOrder;
        m_day = day;
        m_shift = shift;
        m_localDateHour = localDateHour;
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

        var other = obj as HourlyOperationSummaryKey;
        if (null == other) {
          return false;
        }

        return m_machine.Equals (other.m_machine)
          && object.Equals (m_operation, other.m_operation)
          && object.Equals (m_component, other.m_component)
          && object.Equals (m_workOrder, other.m_workOrder)
          && object.Equals (m_line, other.m_line)
          && object.Equals (m_manufacturingOrder, other.m_manufacturingOrder)
          && m_day.Equals (other.m_day)
          && object.Equals (m_shift, other.m_shift)
          && object.Equals (m_localDateHour, other.m_localDateHour);
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
          if (null != m_operation) {
            hashCode += 1000000009 * m_operation.GetHashCode ();
          }
          if (null != m_component) {
            hashCode += 1000000011 * m_component.GetHashCode ();
          }
          if (null != m_workOrder) {
            hashCode += 1000000013 * m_workOrder.GetHashCode ();
          }
          if (null != m_line) {
            hashCode += 1000000015 * m_line.GetHashCode ();
          }
          if (null != m_manufacturingOrder) {
            hashCode += 1000000017 * m_manufacturingOrder.GetHashCode ();
          }
          if (m_day.HasValue) {
            hashCode += 1000000019 * m_day.GetHashCode ();
          }
          if (null != m_shift) {
            hashCode += 1000000021 * m_shift.GetHashCode ();
          }
          hashCode += 1000000023 * m_localDateHour.GetHashCode ();
        }
        return hashCode;
      }

      public override string ToString ()
      {
        return $"[machine={m_machine.Id} operation={((IDataWithId)m_operation)?.Id} component={((IDataWithId)m_component)?.Id} workOrder={((IDataWithId)m_workOrder)?.Id} day={m_day} shift={m_shift} hour={m_localDateHour}";
      }
    };

    struct HourlyIntermediateWorkPieceSummaryValue
    {
      public TimeSpan Duration;
      public int TotalCycles;
      public int AdjustedCycles;
      public int AdjustedQuantity;
    }

    #region Members
    readonly IDictionary<HourlyOperationSummaryKey, HourlyIntermediateWorkPieceSummaryValue> m_summaryAccumulator =
      new Dictionary<HourlyOperationSummaryKey, HourlyIntermediateWorkPieceSummaryValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (HourlyOperationSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    public HourlyOperationSummaryAccumulator ()
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
      return true;
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
      if ((null != before)
          && (null != after)
          && (null != before.OperationSlot)
          && (null != after.OperationSlot)
          && (before.OperationSlot.Id == after.OperationSlot.Id)
          && object.Equals (before.Full, after.Full)) { // Only the local date hour may change
        DateTime beforeLocalDateHour = before.GetLocalDateHour ();
        DateTime afterLocalDateHour = after.GetLocalDateHour ();
        if (object.Equals (beforeLocalDateHour, afterLocalDateHour)) {
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

      IMachine machine = operationCycle.Machine;
      Debug.Assert (null != machine);

      var key = new HourlyOperationSummaryKey (machine, operationSlot.Operation,
        operationSlot.Component, operationSlot.WorkOrder, operationSlot.Line, operationSlot.ManufacturingOrder,
        operationSlot.Day, operationSlot.Shift, operationCycle.GetLocalDateHour ());
      if (!m_summaryAccumulator.TryGetValue (key, out var v)) {
        v = new HourlyIntermediateWorkPieceSummaryValue {
          TotalCycles = increment
        };
        if (operationCycle.Quantity.HasValue) {
          v.AdjustedCycles = increment;
          v.AdjustedQuantity = operationCycle.Quantity.Value * increment;
        }
        m_summaryAccumulator[key] = v;
      }
      else {
        v.TotalCycles += increment;
        if (operationCycle.Quantity.HasValue) {
          v.AdjustedCycles += increment;
          v.AdjustedQuantity += operationCycle.Quantity.Value * increment;
        }
        m_summaryAccumulator[key] = v;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"UpdateCycle({operationCycle.Machine.Id}): total cycles={v.TotalCycles} increment={increment} duration={v.Duration} localDateHour={key.LocalDateHour} shift={key.Shift}");
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

      if (null != before) {
        OperationSlotUpdate (before, -1);
      }
      OperationSlotUpdate (after, +1);
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

    DateTime ConvertToLocalDateHour (DateTime utcDateTime)
    {
      var localDateTime = utcDateTime.ToLocalTime ();
      return new DateTime (localDateTime.Year, localDateTime.Month, localDateTime.Day,
        localDateTime.Hour, 00, 00, DateTimeKind.Local);
    }

    void OperationSlotUpdate (IOperationSlot operationSlot, int multiplicator)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"OperationSlotUpdate({operationSlot?.Machine?.Id}): range={operationSlot?.DateTimeRange} multiplicator={multiplicator}");
      }

      if ((null != operationSlot)
          && (null != operationSlot.Operation)) {

        DateTime lowerBound;
        if (!operationSlot.DateTimeRange.Lower.HasValue) {
          log.Error ($"OperationSlotUpdate: range {operationSlot.DateTimeRange} from -oo");
          Debug.Assert (false);
          lowerBound = DateTime.UtcNow.Subtract (TimeSpan.FromDays (30));
        }
        else {
          lowerBound = operationSlot.DateTimeRange.Lower.Value;
        }

        var now = DateTime.UtcNow;
        var upperBound = Bound.Compare<DateTime> (operationSlot.DateTimeRange.Upper, now) < 0
          ? operationSlot.DateTimeRange.Upper.Value
          : now.AddDays (7);
        var localDateHour = ConvertToLocalDateHour (lowerBound);
        var maxLocalDateHour = ConvertToLocalDateHour (upperBound);
        while (localDateHour <= maxLocalDateHour) {
          if (log.IsDebugEnabled) {
            log.Debug ($"OperationSlotUpdate({operationSlot?.Machine?.Id}): range={operationSlot?.DateTimeRange} multiplicator={multiplicator} hour={localDateHour}");
          }
          OperationSlotUpdate (operationSlot, multiplicator, localDateHour);
          localDateHour = localDateHour.AddHours (1);
        }
      }
      // else nothing to do
    }

    void OperationSlotUpdate (IOperationSlot operationSlot, int multiplicator, DateTime localDateHour)
    {
      Debug.Assert ((-1 == multiplicator) || (+1 == multiplicator));

      if ((null != operationSlot)
          && (null != operationSlot.Operation)) {
        var key =
          new HourlyOperationSummaryKey (operationSlot.Machine,
            operationSlot.Operation,
            operationSlot.Component,
            operationSlot.WorkOrder, operationSlot.Line,
            operationSlot.ManufacturingOrder,
            operationSlot.Day, operationSlot.Shift,
            localDateHour);
        var summaryRange = new UtcDateTimeRange (localDateHour.ToUniversalTime (),
          localDateHour.AddHours (1).ToUniversalTime ());
        var intersection = new UtcDateTimeRange (summaryRange.Intersects (operationSlot.DateTimeRange));
        if (!intersection.IsEmpty ()) {
          Debug.Assert (intersection.Duration.HasValue);
          if (TimeSpan.FromHours (2) < intersection.Duration.Value) {
            log.Fatal ($"OperationSlotUpdate: invalid intesection {intersection}");
            Debug.Assert (false);
          }
          if (!m_summaryAccumulator.TryGetValue (key, out var v)) {
            v = new HourlyIntermediateWorkPieceSummaryValue {
              Duration = TimeSpan.FromSeconds (multiplicator * intersection.Duration.Value.TotalSeconds)
            };
            m_summaryAccumulator[key] = v;
          }
          else {
            v.Duration = v.Duration.Add (TimeSpan.FromSeconds (multiplicator * intersection.Duration.Value.TotalSeconds));
            m_summaryAccumulator[key] = v;
            if (log.IsWarnEnabled) {
              if (TimeSpan.FromHours (2) < v.Duration) {
                log.Warn ($"OperationSlotUpdate: duration for key {key} is {v.Duration}, more than 2 hours");
              }
              if (v.Duration < TimeSpan.FromHours (-2)) {
                log.Warn ($"OperationSlotUpdate: duration for key {key} is {v.Duration}, less than -2 hours");
              }
            }
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"OperationSlotUpdate({operationSlot.Machine.Id}): new duration {v.Duration} intersection={intersection} localDateHour={localDateHour} shift={key.Shift}");
          }
        }
      }
    }

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_summaryAccumulator
        .Where (x => (x.Value.TotalCycles != 0) || (0 != x.Value.Duration.TotalSeconds));
      foreach (KeyValuePair<HourlyOperationSummaryKey, HourlyIntermediateWorkPieceSummaryValue> data
               in withChangeItems) {
        SetActive ();
        var key = data.Key;
        var v = data.Value;
        (new HourlyOperationSummaryDAO ())
          .UpdateOffset (key.Machine,
                   key.Operation,
                   key.Component,
                   key.WorkOrder,
                   key.Line,
                   key.ManufacturingOrder,
                   key.Day,
                   key.Shift,
                   key.LocalDateHour,
                   v.Duration,
                   v.TotalCycles,
                   v.AdjustedCycles,
                   v.AdjustedQuantity);
      }
      m_summaryAccumulator.Clear ();
      SetActive ();
    }
    #endregion // Methods
  }
}
