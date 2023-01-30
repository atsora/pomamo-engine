// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Lemoine.Business.Config;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;
using Lemoine.GDBPersistentClasses;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.OperationSlotCycles
{
  public sealed class OperationSlotCyclesAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 200;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new OperationSlotCyclesAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator for the OperationSlot analysis table
  /// </summary>
  public class OperationSlotCyclesAccumulator
    : Accumulator
    , IOperationCycleAccumulator
    , IOperationSlotAccumulator
  {
    static readonly string USE_STEP_KEY = "Plugin.OperationSlotCycles.UseStep";
    static readonly bool USE_STEP_DEFAULT = false;

    sealed class OperationSlotValue
    {
      /// <summary>
      /// Total cycles offset
      /// </summary>
      public int TotalCycles { get; set; }

      /// <summary>
      /// Adjusted cycles offset
      /// </summary>
      public int AdjustedCycles { get; set; }

      /// <summary>
      /// Adjusted quantity offset
      /// </summary>
      public int AdjustedQuantity { get; set; }

      /// <summary>
      /// Partial cycles offset
      /// </summary>
      public int PartialCycles { get; set; }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="operationCycle"></param>
      /// <param name="increment"></param>
      public OperationSlotValue (IOperationCycle operationCycle, int increment)
      {
        if (operationCycle.Full) {
          TotalCycles = increment;
          if (operationCycle.Quantity.HasValue) {
            AdjustedCycles = increment;
            AdjustedQuantity = increment * operationCycle.Quantity.Value;
          }
          PartialCycles = 0;
        }
        else {
          TotalCycles = 0;
          PartialCycles = increment;
        }
      }

      /// <summary>
      /// Add an operation cycle / increment
      /// </summary>
      /// <param name="operationCycle"></param>
      /// <param name="increment"></param>
      public void Add (IOperationCycle operationCycle, int increment)
      {
        if (operationCycle.Full) {
          TotalCycles += increment;
          if (operationCycle.Quantity.HasValue) {
            AdjustedCycles += increment;
            AdjustedQuantity += increment * operationCycle.Quantity.Value;
          }
        }
        else {
          PartialCycles += increment;
        }
      }

      /// <summary>
      /// Is empty ? (nothing to do)
      /// </summary>
      /// <returns></returns>
      public bool IsEmpty ()
      {
        return (0 == TotalCycles) && (0 == AdjustedCycles) && (0 == PartialCycles);
      }
    };

    #region Members
    readonly IDictionary<OperationSlot, OperationSlotValue> m_operationSlotAccumulator =
      new Dictionary<OperationSlot, OperationSlotValue> ();
    // Set of removed operation slots
    readonly HashSet<IOperationSlot> m_removedOperationSlots = new HashSet<IOperationSlot> ();
    // Dictionary (current, previous)
    readonly IDictionary<IOperationSlot, IOperationSlot> m_updatedOperationSlots = new Dictionary<IOperationSlot, IOperationSlot> ();
    readonly IDictionary<IOperationCycle, IOperationCycle> m_adjustBeginCycles = new Dictionary<IOperationCycle, IOperationCycle> (); // adjust = previous if known
    readonly HashSet<IOperationCycle> m_adjustEndCycles = new HashSet<IOperationCycle> ();

    IEnumerable<Pulse.Extensions.Database.IOperationCycleFullExtension> m_operationCycleFullExtensions = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationSlotCyclesAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationSlotCyclesAccumulator ()
    {
    }
    #endregion // Constructors

    #region IOperationSlotAccumulator
    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationSlotUpdated (IOperationSlot before, IOperationSlot after)
    {
      Debug.Assert (null != after);
      if ((null != before) && object.Equals (before.DateTimeRange, after.DateTimeRange)) {
        // No change in date/time ranges
        return;
      }

      m_removedOperationSlots.Remove (after);
      if (!m_updatedOperationSlots.ContainsKey (after)) {
        m_updatedOperationSlots[after] = before;
      }
    }

    /// <summary>
    /// IOperationSlotAccumulator implementation
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="initialState"></param>
    public void OperationSlotRemoved (IOperationSlot operationSlot, IOperationSlot initialState)
    {
      m_removedOperationSlots.Add (operationSlot);
      m_updatedOperationSlots.Remove (operationSlot);
    }
    #endregion // IOperationSlotAccumulator

    #region IOperationCycleAccumulator
    /// <summary>
    /// IOperationCycleAccumulator implementation
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void OperationCycleUpdated (IOperationCycle before,
                                       IOperationCycle after)
    {
      if ((null == before)
          || (null == after)
          || (null == before.OperationSlot)
          || (null == after.OperationSlot)
          || !object.Equals (before.Full, after.Full)
          || (before.OperationSlot.Id != after.OperationSlot.Id)
          || !object.Equals (before.Quantity, after.Quantity)
          || !object.Equals (before.End, after.End)) { // End because it may have an impacted on the average cycle time
        if (null != before) {
          UpdateCycle (before, -1);
        }
        if (null != after) {
          UpdateCycle (after, +1);
        }
      }
    }

    /// <summary>
    /// Add an operation cycle
    /// </summary>
    /// <param name="cycle">not null</param>
    /// <param name="increment"></param>
    void UpdateCycle (IOperationCycle cycle,
                      int increment)
    {
      Debug.Assert (null != cycle);

      if (null != cycle.OperationSlot) {
        var operationSlot = cycle.OperationSlot;
        if (!ModelDAOHelper.DAOFactory.IsInitialized (operationSlot)) {
          operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (operationSlot.Id, cycle.Machine);
        }
        OperationSlotValue v;
        var operationSlot2 = (OperationSlot)operationSlot;
        if (!m_operationSlotAccumulator.TryGetValue (operationSlot2, out v)) {
          m_operationSlotAccumulator[operationSlot2] = new OperationSlotValue (cycle, increment);
        }
        else {
          v.Add (cycle, increment);
        }
      }
    }
    #endregion // IOperationCycleAccumulator

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      OperationSlotAccumulatorStore ();

      // Re-adjust cycles
      ReadjustCyclesBegin ();
      ReadjustCyclesEnd ();

      OperationCycleAccumulatorStore ();
    }

    bool IsMachineExcluded (IMachine machine)
    {
      Debug.Assert (null != machine);

      IMonitoredMachine monitoredMachine =
        ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machine.Id);
      if (null == monitoredMachine) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("IsMachineExcluded: " +
                          "machine {0} is not a monitored one",
                          machine.Id);
        }
        return true;
      }
      else {
        return false;
      }
    }

    bool IsExcluded (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);

      if (IsMachineExcluded (operationSlot.Machine)) {
        return true;
      }

      if (Bound.Compare<DateTime> (DateTime.UtcNow, operationSlot.BeginDateTime) <= 0) { // In the future
        return true;
      }

      return false;
    }

    bool IsExcluded (KeyValuePair<IOperationSlot, IOperationSlot> keyValue)
    {
      Debug.Assert (null != keyValue.Key);

      if (IsMachineExcluded (keyValue.Key.Machine)) {
        return true;
      }

      if (Bound.Compare<DateTime> (DateTime.UtcNow, keyValue.Key.BeginDateTime) <= 0) { // In the future
        if ((null == keyValue.Value) || (Bound.Compare<DateTime> (DateTime.UtcNow, keyValue.Value.BeginDateTime) <= 0)) {
          return true;
        }
      }

      return false;
    }

    void OperationSlotAccumulatorStore ()
    {
      // .. removed
      foreach (var removedOperationSlot in m_removedOperationSlots.Where (operationSlot => !IsExcluded (operationSlot))) {
        SetActive ();
        RemoveAllCyclesFromOperationSlot (removedOperationSlot);
      }
      // .. updated
      foreach (var updatedOperationSlot in m_updatedOperationSlots.Where (keyValue => !IsExcluded (keyValue))) {
        SetActive ();
        IOperationSlot before = updatedOperationSlot.Value;
        IOperationSlot after = updatedOperationSlot.Key;
        if (null == before) { // New operation slot
          AssociateCyclesMatchingOperationSlot (after);
        }
        else if (!before.DateTimeRange.Overlaps (after.DateTimeRange)) {
          RemoveAllCyclesFromOperationSlot (after);
          AssociateCyclesMatchingOperationSlot (after);
        }
        else { // Intersection
          if (!after.DateTimeRange.ContainsRange (before.DateTimeRange)) { // So cycles may be removed
            // reduced on the left
            if (before.DateTimeRange.Lower.HasValue
                && (Bound.Compare<DateTime> (before.DateTimeRange.Lower, after.DateTimeRange.Lower) < 0)) {
              IOperationCycle leftOperationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindWithEndEqualTo (before.Machine, before.DateTimeRange.Lower.Value);
              if ((null != leftOperationCycle) && !leftOperationCycle.HasRealEnd ()) {
                m_adjustEndCycles.Add (leftOperationCycle);
              }
            }
            // reduced on the right
            if (before.DateTimeRange.Upper.HasValue
                && (Bound.Compare<DateTime> (after.DateTimeRange.Upper, before.DateTimeRange.Upper) < 0)) {
              IOperationCycle rightOperationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindWithBeginEqualTo (before.Machine, before.DateTimeRange.Upper.Value);
              if ((null != rightOperationCycle) && !rightOperationCycle.HasRealBegin ()) {
                if (!m_adjustBeginCycles.ContainsKey (rightOperationCycle)) {
                  m_adjustBeginCycles[rightOperationCycle] = null;
                }
              }
            }
            RemoveSomeCyclesFromOperationSlot (after);
          }
          if (!before.DateTimeRange.ContainsRange (after.DateTimeRange)) { // New periods
            // extended on the left
            if (Bound.Compare<DateTime> (after.DateTimeRange.Lower, before.DateTimeRange.Lower) < 0) {
              IOperationCycle firstOperationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindWithBeginEqualTo (before.Machine, before.DateTimeRange.Lower.Value);
              if ((null != firstOperationCycle) && (!firstOperationCycle.HasRealBegin ())) {
                if (!m_adjustBeginCycles.ContainsKey (firstOperationCycle)) {
                  m_adjustBeginCycles[firstOperationCycle] = null;
                }
              }
              UtcDateTimeRange range = new UtcDateTimeRange (after.DateTimeRange.Lower,
                                                             before.DateTimeRange.Lower.Value);
              AssociateCyclesMatchingSlotPeriod (after, range);
            }
            // extended on the right
            if (Bound.Compare<DateTime> (before.DateTimeRange.Upper, after.DateTimeRange.Upper) < 0) {
              IOperationCycle lastOperationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindWithEndEqualTo (before.Machine, before.DateTimeRange.Upper.Value);
              if ((null != lastOperationCycle) && !lastOperationCycle.HasRealEnd ()) {
                m_adjustEndCycles.Add (lastOperationCycle);
              }
              UtcDateTimeRange range = new UtcDateTimeRange (before.DateTimeRange.Upper.Value,
                                                             after.DateTimeRange.Upper);
              AssociateCyclesMatchingSlotPeriod (after, range);
            }
          }
        }
      }

      m_removedOperationSlots.Clear ();
      m_updatedOperationSlots.Clear ();
    }

    void RemoveAllCyclesFromOperationSlot (IOperationSlot operationSlot)
    {
      Debug.Assert (0 != operationSlot.Id);

      IList<IOperationCycle> operationCycles =
        ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindAllWithOperationSlot (operationSlot);
      IOperationCycle previousCycle = null;
      foreach (var operationCycle in operationCycles) {
        SetActive ();
        operationCycle.OperationSlot = null;
        if (!operationCycle.HasRealBegin ()) {
          if (null != previousCycle) {
            m_adjustBeginCycles[operationCycle] = previousCycle;
          }
          else if (!m_adjustBeginCycles.ContainsKey (operationCycle)) {
            m_adjustBeginCycles[operationCycle] = null;
          }
        }
        if (!operationCycle.HasRealEnd ()) {
          m_adjustEndCycles.Add (operationCycle);
        }
        previousCycle = operationCycle;
      }
    }

    void RemoveSomeCyclesFromOperationSlot (IOperationSlot operationSlot)
    {
      Debug.Assert (0 != operationSlot.Id);

      IList<IOperationCycle> operationCycles =
        ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindAllWithOperationSlot (operationSlot);
      IOperationCycle previousCycle = null;
      foreach (var operationCycle in operationCycles) {
        SetActive ();
        OperationCycle.OperationSlotCompatibility compatibility =
          ((OperationCycle)operationCycle).IsCompatibleWithOperationSlot (operationSlot);
        if (!compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.Compatible)) {
          if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.Split)) {
            previousCycle = SplitCycle (operationCycle, operationSlot, compatibility);
            continue;
          }
          else {
            operationCycle.OperationSlot = null;
            if (!operationCycle.HasRealBegin ()) {
              m_adjustBeginCycles[operationCycle] = null;
            }
            if (!operationCycle.HasRealEnd ()) {
              m_adjustEndCycles.Add (operationCycle);
            }
          }
        }
        if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.AdjustEstimatedBegin)) {
          if (null != previousCycle) {
            m_adjustBeginCycles[operationCycle] = previousCycle;
          }
          else if (!m_adjustBeginCycles.ContainsKey (operationCycle)) {
            m_adjustBeginCycles[operationCycle] = null;
          }
        }
        else if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.AdjustEstimatedEnd)) {
          m_adjustEndCycles.Add (operationCycle);
        }
        previousCycle = operationCycle;
      }
    }

    void AssociateCyclesMatchingOperationSlot (IOperationSlot operationSlot)
    {
      AssociateCyclesMatchingSlotPeriod (operationSlot,
                                         operationSlot.DateTimeRange);
    }

    void AssociateCyclesMatchingSlotPeriod (IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      Debug.Assert (null != operationSlot);

      IMachine machine = operationSlot.Machine;

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} AssociateCyclesMatchingSlotPeriod: range={1} " +
                         "check no new operation cycle " +
                         "may be associated to the new period",
                         operationSlot.Machine.Id, range);
      }
      // fetch all cycles that intersect range
      // in ascending order of cycles
      IEnumerable<IOperationCycle> operationCycles;
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_STEP_KEY, USE_STEP_DEFAULT)) {
        var step = operationSlot.TotalCycles + operationSlot.PartialCycles + 3;
        operationCycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindOverlapsRangeAscending (machine, range, step)
          .LoadOperationSlots ();
      }
      else {
        operationCycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllInRange (machine, range);
      }

      IOperationCycle previousCycle = null;
      bool isPreviousCycleKnown = false;

      foreach (IOperationCycle operationCycle in operationCycles) {
        SetActive ();
        // if current cycle ends before slot begin, do not consider it
        if (operationCycle.HasRealEnd ()
            && (Bound.Compare<DateTime> (operationCycle.End.Value, range.Lower) <= 0)) {
          previousCycle = operationCycle;
          continue;
        }

        OperationCycle.OperationSlotCompatibility compatibility = ((OperationCycle)operationCycle).IsCompatibleWithOperationSlot (operationSlot);


        if (!compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.Compatible)) { // Not compatible
          if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.Split)) {
            Debug.Assert (operationCycle.HasRealBegin ());
            Debug.Assert (operationCycle.HasRealEnd ());
            log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: operationCycle={operationCycle.Id} SplitCycle operationSlot={operationSlot.Id}");
            IOperationCycle lastOperationCycle = SplitCycle (operationCycle, operationSlot, compatibility);
            Debug.Assert (lastOperationCycle.HasRealEnd ());
            previousCycle = lastOperationCycle;
            continue;
          }

          if (Bound.Compare<DateTime> (range.Upper, operationCycle.DateTime) < 0) { // On the right
            log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: operationCycle={operationCycle.Id}  On the right");
            if (!operationCycle.HasRealBegin ()) {
              CheckPreviousCycle (ref previousCycle, ref isPreviousCycleKnown, operationSlot, range);
              if ((null != previousCycle) && !previousCycle.HasRealEnd ()) {
                var lastNotRealEndCycle = previousCycle;
                Debug.Assert (operationSlot.Equals (lastNotRealEndCycle.OperationSlot));
                Debug.Assert (lastNotRealEndCycle.HasRealBegin ());
                Debug.Assert (!lastNotRealEndCycle.HasRealEnd ());
                Debug.Assert (lastNotRealEndCycle.Begin.HasValue);
                Debug.Assert (operationSlot.EndDateTime.HasValue);
                if ((Bound.Compare<DateTime> (operationSlot.BeginDateTime, lastNotRealEndCycle.Begin.Value) <= 0)
                    && (Bound.Compare<DateTime> (operationSlot.EndDateTime.Value
                                                 .Subtract (AnalysisConfigHelper.OperationCycleAssociationMargin),
                                                 lastNotRealEndCycle.Begin.Value) <= 0)) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat (".{0} AssociateCyclesMatchingSlotPeriod: " +
                                     "merge {1} and {2}",
                                     operationSlot.Machine.Id,
                                     lastNotRealEndCycle, operationCycle.Id);
                  }
                  log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: operationCycle={operationCycle.Id}  MergeCycles lastNotRealEndCycle={lastNotRealEndCycle.Id}");
                  MergeCycles (lastNotRealEndCycle, operationCycle);
                }
              }
              if (!operationCycle.HasRealBegin ()) {
                CheckPreviousCycle (ref previousCycle, ref isPreviousCycleKnown, operationSlot, range);
                if (null != previousCycle) {
                  m_adjustBeginCycles[operationCycle] = previousCycle;
                }
                else if (!m_adjustBeginCycles.ContainsKey (operationCycle)) {
                  m_adjustBeginCycles[operationCycle] = null;
                }
              }
            }
            previousCycle = null;
            break;
          }
          previousCycle = operationCycle;
          continue;
        }
        else { // operationCycle compatible with operationSlot
          CheckPreviousCycle (ref previousCycle, ref isPreviousCycleKnown, operationSlot, range);
          if ((null != previousCycle) && !previousCycle.HasRealEnd ()) {
            var lastNotRealEndCycle = previousCycle;
            // if last cycle has no end and current cycle has no begin,
            // current cycle should have an end and both cycles are merged
            if (!operationCycle.Begin.HasValue
              || operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated)) { // With no begin
              // .. Merge lastNotFullCycle with operationCycle
              log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: merge lastNotFullCycle with operationCycle with no begin");
              MergeCycles (previousCycle, operationCycle);
            }
            // if last cycle is partial, has not been merged yet (current cycle has a begin),
            // and can be associated to this slot, do it now
            else {
              var lastNotFullCycleCompatibility =
                ((OperationCycle)lastNotRealEndCycle).IsCompatibleWithOperationSlot (operationSlot);
              if (lastNotFullCycleCompatibility.HasFlag (OperationCycle.OperationSlotCompatibility.Compatible)) {
                // ... Add the previous partial cycle
                Debug.Assert (lastNotRealEndCycle.Begin.HasValue);
                Debug.Assert (!lastNotRealEndCycle.End.HasValue
                              || lastNotRealEndCycle.Status.HasFlag (OperationCycleStatus.EndEstimated));
                if (log.IsDebugEnabled) {
                  log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: associate the previous partial cycle {lastNotRealEndCycle.Begin.Value}, because the current operation cycle has a begin");
                }
                if (operationCycle.Begin.HasValue) {
                  Debug.Assert (!operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated));
                  lastNotRealEndCycle.SetEstimatedEnd (operationCycle.Begin.Value);
                }
                if (!operationSlot.Equals (lastNotRealEndCycle.OperationSlot)) {
                  /* Caused a bug when OperationSlot.Equals was not overridden
                     since operationCycle.OperationSlot can be proxied */
                  if (log.IsDebugEnabled) {
                    log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: associate operation cycle {lastNotRealEndCycle} to operation slot {operationSlot.Id}");
                  }
                  lastNotRealEndCycle.OperationSlot = operationSlot;
                }
              }
            }
          }

          if (operationCycle.HasRealEnd ()) {
            // With a real end
            // associate it to slot immediately
            if (!operationCycle.Begin.HasValue
                || operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated)) {
              CheckPreviousCycle (ref previousCycle, ref isPreviousCycleKnown, operationSlot, range);
              if (previousCycle is null) { // First cycle in slot: take the operation slot begin date/time as an estimated begin
                Debug.Assert (Bound.Compare<DateTime> (operationSlot.BeginDateTime, operationCycle.End.Value) <= 0);
                if (operationSlot.BeginDateTime.HasValue) {
                  operationCycle.SetEstimatedBegin (operationSlot.BeginDateTime.Value);
                }
              }
              else if (previousCycle.End.HasValue
                       && !previousCycle.Status.HasFlag (OperationCycleStatus.EndEstimated)) {
                // && null != previousCycle
                Debug.Assert (previousCycle.End.HasValue);
                operationCycle.SetEstimatedBegin (previousCycle.End.Value);
              }
              else { // previous cycle is partial, the begin is really estimated
                Debug.Assert (previousCycle.Begin.HasValue);
                operationCycle.SetEstimatedBegin (previousCycle.Begin.Value);
              }
            }
          }

          if (!operationSlot.Equals (operationCycle.OperationSlot)) {
            /* Caused a bug when OperationSlot.Equals was not overridden
               since operationCycle.OperationSlot can be proxied */
            if (log.IsDebugEnabled) {
              log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: associate operation cycle {operationCycle} to operation slot {operationSlot}");
            }
            operationCycle.OperationSlot = operationSlot;
          }
        } // else compatible
        previousCycle = operationCycle;
        isPreviousCycleKnown = true;
      } // End loop operationCycles

      // treat case when last cycle is partial (thus has not been fully processed yet)
      if ((null != previousCycle)
        && !previousCycle.HasRealEnd ()
        && (Bound.Compare<DateTime> (operationSlot.BeginDateTime, previousCycle.Begin.Value) <= 0)) {
        var lastNotRealEndCycle = previousCycle;
        if (log.IsDebugEnabled) {
          log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: one remaining partial cycle to process");
        }
        Debug.Assert (lastNotRealEndCycle.Begin.HasValue);
        if (operationSlot.EndDateTime.HasValue) {
          Debug.Assert (lastNotRealEndCycle.Begin.Value <= operationSlot.EndDateTime.Value);
          // set estimated end of partial cycle to end of slot
          lastNotRealEndCycle.SetEstimatedEnd (operationSlot.EndDateTime.Value);
        }

        if (!operationSlot.Equals (lastNotRealEndCycle.OperationSlot)) {
          /* Caused a bug when OperationSlot.Equals was not overridden
             since operationCycle.OperationSlot can be proxied */
          if (log.IsDebugEnabled) {
            log.Debug ($".{operationSlot.Machine.Id} AssociateCyclesMatchingSlotPeriod: associate operation cycle {lastNotRealEndCycle} to operation slot {operationSlot}");
          }
          lastNotRealEndCycle.OperationSlot = operationSlot;
        }
      }
    }

    void CheckPreviousCycle (ref IOperationCycle previousCycle, ref bool isPreviousCycleKnown, IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      if (!isPreviousCycleKnown && (previousCycle is null)) {
        previousCycle = GetPreviousCycle (operationSlot, range);
        isPreviousCycleKnown = true;
      }
    }

    IOperationCycle GetPreviousCycle (IOperationSlot operationSlot, UtcDateTimeRange range)
    {
      Debug.Assert (null != operationSlot);

      var previousCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
        .GetFirstStrictlyBefore (operationSlot.Machine, range.Lower);
      if (previousCycle is null) {
        return null;
      }
      else {
        if (previousCycle.OperationSlot is null) {
          return null;
        }
        else if (previousCycle.OperationSlot?.Id == operationSlot.Id) {
          return previousCycle;
        }
        else if (IsPartialCloseToOperationSlotStart (previousCycle, operationSlot)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPreviousCycle: previous cycle partial and close to {operationSlot.Id}");
          }
          return previousCycle;
        }
        else { // Different operation slot
          var previousCycleOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (previousCycle.OperationSlot.Id, operationSlot.Machine);
          if (IsPartialCloseToOperationSlotStart (previousCycle, operationSlot)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPreviousCycle: previous cycle partial and close to {operationSlot.Id}");
            }
            return previousCycle;
          }
          else if (previousCycleOperationSlot.Operation?.Id != operationSlot.Operation?.Id) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetPreviousCycle: different operation {previousCycle.OperationSlot?.Operation?.Id} VS {operationSlot.Operation?.Id} => return null");
            }
            return null;
          }
          else if (Bound.Equals<DateTime> (previousCycleOperationSlot.DateTimeRange.Upper, operationSlot.DateTimeRange.Lower)) {
            return previousCycle;
          }
          else {
            var isContinousOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .IsContinuousOperationInRange (operationSlot.Machine,
                                           new UtcDateTimeRange (previousCycleOperationSlot.DateTimeRange.Upper.Value,
                                                                 operationSlot.DateTimeRange.Lower.Value),
                                           operationSlot.Operation);
            if (isContinousOperation) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPreviousCycle: same operation in [{previousCycleOperationSlot.DateTimeRange.Upper.Value},{operationSlot.DateTimeRange.Lower.Value})");
              }
              return previousCycle;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetPreviousCycle: there is a different operation in [{previousCycleOperationSlot.DateTimeRange.Upper.Value},{operationSlot.DateTimeRange.Lower.Value})");
              }
              return null;
            }
          }
        }
      }
    }

    bool IsPartialCloseToOperationSlotStart (IOperationCycle operationCycle, IOperationSlot operationSlot)
    {
      if (operationCycle.HasRealEnd ()) {
        return false;
      }
      Debug.Assert (operationCycle.Begin.HasValue);
      var operationCycleAssociationMargin = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationCycleAssociationMargin),
                               TimeSpan.FromSeconds (0));
      return (null != operationSlot)
        && (Bound.Compare<DateTime> (operationCycle.Begin.Value, operationSlot.DateTimeRange.Lower) < 0)
        && (operationSlot.BeginDateTime.Value.Subtract (operationCycle.Begin.Value) < operationCycleAssociationMargin);
    }

    /// <summary>
    /// Split a cycle
    /// </summary>
    /// <param name="operationCycle">cycle to split</param>
    /// <param name="operationSlot">Referenced operation slot</param>
    /// <param name="compatibility">Compatibility between both</param>
    /// <returns>Last operation cycle</returns>
    IOperationCycle SplitCycle (IOperationCycle operationCycle, IOperationSlot operationSlot,
                              OperationCycle.OperationSlotCompatibility compatibility)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($".{operationCycle?.Machine?.Id} SplitCycle: ");
      }
      if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.BeginOnly)) {
        Debug.Assert (operationCycle.HasRealEnd ()); // Because of Split + BeginOnly
        Debug.Assert (operationCycle.End.HasValue);
        IOperationCycle newOperationCycle = ModelDAOHelper.ModelFactory
          .CreateOperationCycle (operationCycle.Machine);
        newOperationCycle.SetRealEnd (operationCycle.End.Value);
        Debug.Assert (operationSlot.EndDateTime.HasValue); // Else it won't be BeginOnly
                                                           // Estimated begin / end
        newOperationCycle.SetEstimatedBegin (operationSlot.EndDateTime.Value);
        operationCycle.SetEstimatedEnd (operationSlot.EndDateTime.Value);
        m_adjustEndCycles.Add (operationCycle);
        m_adjustBeginCycles[newOperationCycle] = operationCycle;
        // OperationSlot
        operationCycle.OperationSlot = operationSlot;
        var newOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (newOperationCycle.Machine, newOperationCycle.End.Value);
        newOperationCycle.OperationSlot = newOperationSlot;
        // Full ?
        operationCycle.Full = IsFull (operationCycle);
        newOperationCycle.Full = IsFull (newOperationCycle);
        ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (newOperationCycle);
        return newOperationCycle;
      }
      else if (compatibility.HasFlag (OperationCycle.OperationSlotCompatibility.EndOnly)) {
        Debug.Assert (operationCycle.HasRealBegin ()); // Because of Split + EndOnly
        Debug.Assert (operationCycle.Begin.HasValue);
        IOperationCycle newOperationCycle = ModelDAOHelper.ModelFactory
          .CreateOperationCycle (operationCycle.Machine);
        newOperationCycle.SetRealBegin (operationCycle.Begin.Value);
        Debug.Assert (operationSlot.BeginDateTime.HasValue); // Else it won't be EndOnly
                                                             // Estmated begin  / end
        newOperationCycle.SetEstimatedEnd (operationSlot.BeginDateTime.Value);
        operationCycle.SetEstimatedBegin (operationSlot.BeginDateTime.Value);
        m_adjustEndCycles.Add (newOperationCycle);
        m_adjustBeginCycles[operationCycle] = newOperationCycle;
        // OperationSlot
        operationCycle.OperationSlot = operationSlot;
        var newOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (newOperationCycle.Machine, newOperationCycle.Begin.Value);
        newOperationCycle.OperationSlot = newOperationSlot;
        // Full ?
        operationCycle.Full = IsFull (operationCycle);
        newOperationCycle.Full = IsFull (newOperationCycle);
        ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (newOperationCycle);
        return operationCycle;
      }
      else {
        log.FatalFormat (".{0} AssociateCyclesMatchingSlotPeriod: " +
                         "compatibility={1} but in the same time split and neither BeginOnly nor EndOnly",
                         operationSlot.Machine.Id,
                         compatibility);
        return operationCycle;
      }
    }

    void MergeCycles (IOperationCycle lastNotRealEndCycle, IOperationCycle operationCycle)
    {
      Debug.Assert (null != lastNotRealEndCycle);
      Debug.Assert (null != operationCycle);
      Debug.Assert (lastNotRealEndCycle.HasRealBegin ());
      Debug.Assert (operationCycle.HasRealEnd ());

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} MergeCycles: " +
                         "merge the partial cycle {1} with " +
                         "the operation cycle ending at {2}",
                         lastNotRealEndCycle.Machine.Id,
                         lastNotRealEndCycle.Begin.Value,
                         operationCycle.End.Value);
      }

      operationCycle.Begin = lastNotRealEndCycle.Begin.Value;
      operationCycle.Status = operationCycle.Status.Remove (OperationCycleStatus.BeginEstimated);
      if (!operationCycle.Full) {
        operationCycle.Full = IsFull (operationCycle);
      }
      m_adjustBeginCycles.Remove (operationCycle);

      var previousBetweenCycles = ModelDAOHelper.DAOFactory.BetweenCyclesDAO
        .FindWithNextCycle (lastNotRealEndCycle);
      if (null != previousBetweenCycles) { // Re-assign it
        ((BetweenCycles)previousBetweenCycles).NextCycle = operationCycle;
      }
      lastNotRealEndCycle.OperationSlot = null;
      // Re-associate the serial number to the new full cycle
      MergeDeliverablePieces (lastNotRealEndCycle, operationCycle);
      ModelDAOHelper.DAOFactory.OperationCycleDAO
        .MakeTransient (lastNotRealEndCycle);
    }

    /// <summary>
    /// Merge the deliverable pieces when partialCycle and fullCycle are merged into a new full cycle
    /// </summary>
    /// <param name="partialCycle"></param>
    /// <param name="fullCycle"></param>
    void MergeDeliverablePieces (IOperationCycle partialCycle, IOperationCycle fullCycle)
    {
      IList<IOperationCycleDeliverablePiece> partialDeliverablePieces =
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle (partialCycle);
      foreach (IOperationCycleDeliverablePiece partialDeliverablePiece in partialDeliverablePieces) {
        MergePartialDeliverablePiece (partialDeliverablePiece, fullCycle);
      }
    }

    /// <summary>
    /// Merge a partial deliverable piece into the new full cycle before removing it
    /// </summary>
    /// <param name="partialDeliverablePiece"></param>
    /// <param name="fullCycle"></param>
    void MergePartialDeliverablePiece (IOperationCycleDeliverablePiece partialDeliverablePiece, IOperationCycle fullCycle)
    {
      Debug.Assert (null != partialDeliverablePiece);

      // - Check the full operation cycle is not already associated to a deliverable piece that may correspond
      //   to this partial deliverable piece
      IOperationCycleDeliverablePiece fullDeliverablePiece =
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO
        .FindWithOperationCycleDeliverablePiece (fullCycle, partialDeliverablePiece.DeliverablePiece);
      if (null != fullDeliverablePiece) {
        // The same deliverable is already registered with operation cycle
        // => just keep fullDeliverablePiece
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MergePartialDeliverablePiece: " +
                           "the deliverable piece {0} is already registered with the full cycle" +
                           "=> just keep fullDeliverablePiece",
                           fullDeliverablePiece.DeliverablePiece);
        }
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakeTransient (partialDeliverablePiece);
        return;
      }
      // - Re-associate the deliverable piece to the new operation cycle
      partialDeliverablePiece.OperationCycle = fullCycle;
      ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent (partialDeliverablePiece);
    }

    void ReadjustCyclesBegin ()
    {
      foreach (var operationCyclePrevious in m_adjustBeginCycles) {
        SetActive ();

        IOperationCycle operationCycle = operationCyclePrevious.Key;
        IOperationCycle previousCycle = operationCyclePrevious.Value;

        Debug.Assert (!operationCycle.HasRealBegin ());
        Debug.Assert (operationCycle.HasRealEnd ());
        Debug.Assert (operationCycle.End.HasValue);

        DateTime? newBegin = null;

        if (null == previousCycle) {
          previousCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetFirstStrictlyBefore (operationCycle.Machine, operationCycle.End.Value);
        }

        if (null != previousCycle) {
          // Check previousCycle is for the same operation
          if (null == previousCycle.OperationSlot) {
            if (null == operationCycle.OperationSlot) {
              UtcDateTimeRange range = new UtcDateTimeRange (previousCycle.DateTime,
                                                             operationCycle.End.Value);
              bool existsOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .ExistsInRange (operationCycle.Machine, range);
              if (existsOperationSlot) {
                previousCycle = null;
              }
            }
            else {
              previousCycle = null;
            }
          }
          else if (null == operationCycle.OperationSlot) {
            previousCycle = null;
          }
          else { // null != previousCycle.OperationSlot && null != operationCycle.OperationSlot
            if (previousCycle.OperationSlot.Id != operationCycle.OperationSlot.Id) {
              var operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
              Debug.Assert (null != operationSlot);
              var cycleOperation = operationSlot.Operation;
              var previousOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindById (previousCycle.OperationSlot.Id, previousCycle.Machine);
              Debug.Assert (null != previousOperationSlot);
              var previousCycleOperation = previousOperationSlot.Operation;
              if (!object.Equals (cycleOperation, previousCycleOperation)) {
                previousCycle = null;
              }
              else {
                Debug.Assert (previousOperationSlot.EndDateTime.HasValue);
                Debug.Assert (operationSlot.BeginDateTime.HasValue);
                bool isContinuousOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .IsContinuousOperationInRange (operationSlot.Machine,
                                                 new UtcDateTimeRange (previousOperationSlot.EndDateTime.Value,
                                                                       operationSlot.BeginDateTime.Value),
                                                 cycleOperation);
                if (!isContinuousOperation) {
                  previousCycle = null;
                }
              }
            }
          }
        }

        if (null != previousCycle) {
          if (previousCycle.End.HasValue) {
            newBegin = previousCycle.End.Value;
          }
          else {
            log.ErrorFormat ("ReadjustCyclesBegin: " +
                             "previous cycle {0} of {1} has no end",
                             previousCycle, operationCycle);
          }
        }
        else if (null != operationCycle.OperationSlot) { // Consider the start time of the operation slot
          var operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
          if (operationSlot.BeginDateTime.HasValue) {
            newBegin = operationSlot.BeginDateTime.Value;
          }
        }

        operationCycle.Status = operationCycle.Status.Add (OperationCycleStatus.BeginEstimated);

        if (newBegin.HasValue) {
          if (!operationCycle.Begin.HasValue || !object.Equals (operationCycle.Begin.Value, newBegin.Value)) {
            operationCycle.SetEstimatedBegin (newBegin.Value);
            operationCycle.Full = IsFull (operationCycle);
          }
        }
        else if (operationCycle.Begin.HasValue) {
          log.ErrorFormat ("ReadjustCyclesBegin: " +
                           "no estimated begin was determined (null)");
          operationCycle.Begin = null;
          operationCycle.Full = IsFull (operationCycle);
        }
      }

      m_adjustBeginCycles.Clear ();
    }

    void ReadjustCyclesEnd ()
    {
      foreach (var operationCycle in m_adjustEndCycles) {
        SetActive ();

        Debug.Assert (!operationCycle.HasRealEnd ());
        Debug.Assert (operationCycle.HasRealBegin ());
        Debug.Assert (operationCycle.Begin.HasValue);
        DateTime? newEnd = null;

        IOperationCycle nextCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .GetFirstStrictlyAfter (operationCycle.Machine, operationCycle.Begin.Value);

        if (null != nextCycle) {
          // Check nextCycle is for the same operation
          if (null == nextCycle.OperationSlot) {
            if (null == operationCycle.OperationSlot) {
              UtcDateTimeRange range = new UtcDateTimeRange (operationCycle.Begin.Value,
                                                             nextCycle.DateTime);
              bool existsOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .ExistsInRange (operationCycle.Machine, range);
              if (existsOperationSlot) {
                nextCycle = null;
              }
            }
            else {
              nextCycle = null;
            }
          }
          else if (null == operationCycle.OperationSlot) {
            nextCycle = null;
          }
          else { // null != previousCycle.OperationSlot && null != operationCycle.OperationSlot
            if (nextCycle.OperationSlot.Id != operationCycle.OperationSlot.Id) {
              var operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
              Debug.Assert (null != operationSlot);
              var cycleOperation = operationSlot.Operation;
              var nextOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindById (nextCycle.OperationSlot.Id, nextCycle.Machine);
              Debug.Assert (null != nextOperationSlot);
              var nextCycleOperation = nextOperationSlot.Operation;
              if (!object.Equals (cycleOperation, nextCycleOperation)) {
                nextCycle = null;
              }
              else {
                Debug.Assert (nextOperationSlot.BeginDateTime.HasValue);
                Debug.Assert (operationSlot.EndDateTime.HasValue);
                bool isContinuousOperation = ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .IsContinuousOperationInRange (operationSlot.Machine,
                                                 new UtcDateTimeRange (operationSlot.EndDateTime.Value,
                                                                       nextOperationSlot.BeginDateTime.Value),
                                                 cycleOperation);
                if (!isContinuousOperation) {
                  nextCycle = null;
                }
              }
            }
          }
        }

        if (null != nextCycle) {
          if (nextCycle.Begin.HasValue) {
            newEnd = nextCycle.Begin.Value;
          }
          else {
            log.ErrorFormat ("ReadjustCyclesEnd: " +
                             "next cycle {0} of {1} has no begin",
                             nextCycle, operationCycle);
          }
        }
        else if (null != operationCycle.OperationSlot) { // Consider the end time of the operation slot
          var operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
          if (operationSlot.EndDateTime.HasValue) {
            newEnd = operationSlot.EndDateTime.Value;
          }
        }

        operationCycle.Status = operationCycle.Status.Add (OperationCycleStatus.EndEstimated);

        if (newEnd.HasValue) {
          if (!operationCycle.End.HasValue || !object.Equals (operationCycle.End.Value, newEnd.Value)) {
            operationCycle.SetEstimatedEnd (newEnd.Value);
            operationCycle.Full = IsFull (operationCycle);
          }
        }
        else if (operationCycle.End.HasValue) {
          log.ErrorFormat ("ReadjustCyclesEnd: " +
                           "no estimated end was determined (null)");
          operationCycle.SetEstimatedEnd (null);
          operationCycle.Full = IsFull (operationCycle);
        }
      }

      m_adjustEndCycles.Clear ();
    }

    void OperationCycleAccumulatorStore ()
    {
      foreach (KeyValuePair<OperationSlot, OperationSlotValue> data
               in m_operationSlotAccumulator) {
        SetActive ();
        if (!data.Value.IsEmpty ()) {
          data.Key.TotalCycles += data.Value.TotalCycles;
          data.Key.AdjustedCycles += data.Value.AdjustedCycles;
          data.Key.AdjustedQuantity += data.Value.AdjustedQuantity;
          data.Key.PartialCycles += data.Value.PartialCycles;
          data.Key.UpdateAverageCycleTime ();
        }
      }
      m_operationSlotAccumulator.Clear ();
    }

    /// <summary>
    /// Get the extensions of type IOperationCycleDetectionExtension
    /// </summary>
    /// <returns></returns>
    IEnumerable<Pulse.Extensions.Database.IOperationCycleFullExtension> GetFullExtensions ()
    {
      if (null == m_operationCycleFullExtensions) {
        m_operationCycleFullExtensions = Lemoine.Business.ServiceProvider
          .Get<IEnumerable<IOperationCycleFullExtension>> (new Lemoine.Business.Extension.GlobalExtensions<IOperationCycleFullExtension> ());
      }
      return m_operationCycleFullExtensions;
    }

    bool IsFull (IOperationCycle operationCycle)
    {
      var fullExtensions = GetFullExtensions ();
      if (!fullExtensions.Any ()) {
        if (log.IsWarnEnabled) {
          log.Warn ("IsFull: no OperationCycleFullExtension was registered");
        }
      }

      foreach (var fullExtension in fullExtensions) {
        SetActive ();

        var extensionResult = fullExtension.IsFull (operationCycle);
        if (extensionResult.HasValue) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("IsFull: " +
                             "{0} from extension {1}",
                             extensionResult.Value, fullExtension);
          }
          return extensionResult.Value;
        }
      }

      log.ErrorFormat ("IsFull: no extension is reporting if the operation cycle {0} is full or not",
        operationCycle.Id);
      return false;
    }
  }
}
