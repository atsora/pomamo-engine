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

namespace Lemoine.Plugin.DefaultAccumulators
{
  public sealed class ShiftByMachineAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 452;

    public bool Initialize ()
    {
      return AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift);
    }

    public IAccumulator Create ()
    {
      return new ShiftByMachineAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator to track the ShiftByMachine duration changes
  /// </summary>
  public class ShiftByMachineAccumulator
    : Accumulator
    , IObservationStateSlotAccumulator
  {
    readonly IDictionary<int, ShiftByMachineAccumulatorByMachine> m_accumulators =
      new Dictionary<int, ShiftByMachineAccumulatorByMachine> ();

    /// <summary>
    /// Store
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      foreach (var accumulator in m_accumulators.Values) {
        accumulator.Store (transactionName);
      }
    }

    /// <summary>
    /// Implementation of <see cref="IObservationStateSlotAccumulator"/>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void AddObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      Debug.Assert (null != slot);
      ShiftByMachineAccumulatorByMachine accumulator;
      if (!m_accumulators.TryGetValue (slot.Machine.Id, out accumulator)) {
        m_accumulators[slot.Machine.Id] = accumulator = new ShiftByMachineAccumulatorByMachine (slot.Machine);
      }
      accumulator.AddObservationStateSlotPeriod (slot, range);
    }

    /// <summary>
    /// Implementation of <see cref="IObservationStateSlotAccumulator"/>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void RemoveObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      Debug.Assert (null != slot);
      ShiftByMachineAccumulatorByMachine accumulator;
      if (!m_accumulators.TryGetValue (slot.Machine.Id, out accumulator)) {
        m_accumulators[slot.Machine.Id] = accumulator = new ShiftByMachineAccumulatorByMachine (slot.Machine);
      }
      accumulator.RemoveObservationStateSlotPeriod (slot, range);
    }
  }

  /// <summary>
  /// Accumulator to track the ShiftByMachine duration changes for a specific machine
  /// </summary>
  internal class ShiftByMachineAccumulatorByMachine
    : DateTimeRangeValueChangeTrackerAccumulator<IShift>
  {
    /// <summary>
    /// Machine modification priority to use in the auto shiftmachineassociation
    /// </summary>
    static readonly string AUTO_SHIFT_MACHINE_ASSOCIATION_PRIORITY = "Analysis.ShiftByMachineAccumulator.AutoPriority";

    #region Members
    readonly IMachine m_machine;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ShiftByMachineAccumulatorByMachine).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftByMachineAccumulatorByMachine (IMachine machine)
    {
      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                typeof (ShiftByMachineAccumulatorByMachine).FullName,
                                                machine.Id));
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an observation state slot period
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void AddObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      this.Add (range, slot.Shift);
    }

    /// <summary>
    /// Remove an observation state slot period
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void RemoveObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      this.Remove (range, slot.Shift);
    }

    /// <summary>
    /// Store
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      if (!AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
        log.Debug ("Store: return immediately because the option to get the shift by machine is not active");
        return;
      }

      this.Purge ();

      // Global range from the accumulator
      var globalRange = this.GlobalRange;
      if (globalRange.IsEmpty ()) {
        log.Debug ("Store: return because the accumulator is empty");
        return;
      }

      if (Bound.Compare<DateTime> (DateTime.UtcNow, globalRange.Lower) <= 0) { // In the future only
        log.Debug ($"Store: global range {globalRange} in future only => nothing to do");
        return;
      }

      IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
        .FindById (m_machine.Id);
      if (null == operationSlotSplit) {
        log.WarnFormat ("Store: " +
                        "operationSlotSplit did not exist for machine {0} " +
                        "=> return",
                        m_machine.Id);
        return;
      }
      if (Bound.Compare<DateTime> (operationSlotSplit.End, globalRange.Lower) <= 0) {
        log.DebugFormat ("Store: " +
                         "global range {0} after operationSlotSplit.End {1} " +
                         "=> nothing to do",
                         globalRange, operationSlotSplit.End);
        return;
      }

      // TODO: could be optimized:
      // check if ranges can be merged

      var priority = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTO_SHIFT_MACHINE_ASSOCIATION_PRIORITY, AnalysisConfigHelper.AutoModificationPriority);
      foreach (var dateTimeRangeValue in this.DateTimeRangeValues) {
        if (Bound.Compare<DateTime> (operationSlotSplit.End, dateTimeRangeValue.Range.Lower) <= 0) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Store: break the loop because operationSlotSplit.End {operationSlotSplit.End} was reached");
          }
          break;
        }
        var association =
          new ShiftMachineAssociation (m_machine, null, dateTimeRangeValue.Value.New, dateTimeRangeValue.Range);
        association.Auto = true;
        association.Priority = priority;
        (new ShiftMachineAssociationDAO ()).MakePersistent (association);
      }
    }
    #endregion // Methods
  }
}
