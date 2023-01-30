// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.DefaultAccumulators
{
  public sealed class ProductionRateSummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 102;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new ProductionRateSummaryAccumulator ();
    }
  }

  internal sealed class ProductionRateSummaryKey
  {
    readonly IMachine m_machine;
    readonly IMachineObservationState m_machineObservationState;
    readonly DateTime m_day;
    readonly IShift m_shift;
    readonly int m_hashCode;

    public IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Machine observation state
    /// </summary>
    public IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
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
    /// 
    /// nullable
    /// </summary>
    public IShift Shift
    {
      get { return m_shift; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineObservationState"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    public ProductionRateSummaryKey (IMachine machine, IMachineObservationState machineObservationState, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_machineObservationState = machineObservationState;
      m_day = day;
      m_shift = shift;
      m_hashCode = ComputeHashCode ();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="day"></param>
    /// <param name="reasonSlot"></param>
    public ProductionRateSummaryKey (DateTime day, IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.MachineObservationState, day, reasonSlot.Shift)
    { 
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

      ProductionRateSummaryKey other = obj as ProductionRateSummaryKey;
      if (null == other) {
        return false;
      }

      if (this.GetHashCode () != other.GetHashCode ()) {
        return false;
      }

      return m_machine.Equals (other.m_machine)
        && m_machineObservationState.Equals (other.m_machineObservationState)
        && m_day.Equals (other.m_day)
        && object.Equals (m_shift, other.m_shift);
    }

    /// <summary>
    /// Compute a hash code
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    int ComputeHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * m_machine.GetHashCode ();
        hashCode += 1000000009 * m_machineObservationState.GetHashCode ();
        hashCode += 1000000013 * m_day.GetHashCode ();
        if (null != m_shift) {
          hashCode += 1000000015 * m_shift.GetHashCode ();
        }
      }
      return hashCode;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      return m_hashCode;
    }
  };

  /// <summary>
  /// Accumulator for the ProductionRateSummary analysis table
  /// </summary>
  public sealed class ProductionRateSummaryAccumulator
    : Accumulator
    , IReasonSlotAccumulator
  {
    #region Members
    readonly IDictionary<ProductionRateSummaryKey, (double, TimeSpan)> m_productionRateSummaryAccumulator =
      new Dictionary<ProductionRateSummaryKey, (double, TimeSpan)> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionRateSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionRateSummaryAccumulator ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add some new data in the Summary accumulator
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="day"></param>
    /// <param name="durationChange"></param>
    public void AddReasonSlotDuration (IReasonSlot reasonSlot,
                                       DateTime day,
                                       TimeSpan durationChange)
    {
      if (!reasonSlot.ProductionRate.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddReasonSlotDuration: no production state in {reasonSlot}");
        }
        return;
      }

      ProductionRateSummaryKey productionRateSummaryKey =
        new ProductionRateSummaryKey (reasonSlot.Machine, reasonSlot.MachineObservationState, day, reasonSlot.Shift);
      if (!m_productionRateSummaryAccumulator.TryGetValue (productionRateSummaryKey, out (double, TimeSpan) currentProductionRateSummaryValue)) {
        m_productionRateSummaryAccumulator[productionRateSummaryKey] = (reasonSlot.ProductionRate.Value, durationChange);
      }
      else {
        var oldDuration = currentProductionRateSummaryValue.Item2;
        var newDuration = oldDuration.Add (durationChange);
        if (log.IsDebugEnabled) {
          log.Debug ($"AddReasonSlot: old={oldDuration} change={durationChange} new={newDuration}");
        }
        if (0 == newDuration.TotalSeconds) {
          m_productionRateSummaryAccumulator.Remove (productionRateSummaryKey);
        }
        else { // 0 != newDuration
          var oldRate = currentProductionRateSummaryValue.Item1;
          var newRate = ((oldRate * oldDuration.TotalSeconds) + (reasonSlot.ProductionRate.Value * durationChange.TotalSeconds)) / newDuration.TotalSeconds;
          m_productionRateSummaryAccumulator[productionRateSummaryKey] = (newRate, newDuration);
        }
      }
    }

    /// <summary>
    /// Add a positive or negative number of slots for the specified productionRate slot and day
    /// 
    /// Do nothing here
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="day"></param>
    /// <param name="number"></param>
    public void AddReasonSlotNumber (IReasonSlot reasonSlot,
                                     DateTime day,
                                     int number)
    {
      // Do nothing here
    }

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_productionRateSummaryAccumulator
        .Where (d => 0 != d.Value.Item2.Ticks);
      foreach (var productionRateSummaryData in withChangeItems) {
        var key = productionRateSummaryData.Key;
        var v = productionRateSummaryData.Value;
        (new ProductionRateSummaryDAO ())
          .UpdateDay (transactionName,
                      key.Machine,
                      key.MachineObservationState,
                      key.Day,
                      key.Shift,
                      v.Item2,
                      v.Item1);
      }
      m_productionRateSummaryAccumulator.Clear ();
    }
    #endregion // Methods
  }
}
