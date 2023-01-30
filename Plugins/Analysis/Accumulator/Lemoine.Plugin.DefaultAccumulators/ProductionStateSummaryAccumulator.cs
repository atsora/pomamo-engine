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
  public sealed class ProductionStateSummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 102;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new ProductionStateSummaryAccumulator ();
    }
  }

  internal sealed class ProductionStateSummaryKey
  {
    readonly IMachine m_machine;
    readonly IMachineObservationState m_machineObservationState;
    readonly IProductionState m_productionState;
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
    /// ProductionState
    /// </summary>
    public IProductionState ProductionState
    {
      get { return m_productionState; }
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
    /// <param name="productionState">recommended not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    public ProductionStateSummaryKey (IMachine machine, IMachineObservationState machineObservationState, IProductionState productionState, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_machineObservationState = machineObservationState;
      m_productionState = productionState;
      m_day = day;
      m_shift = shift;
      m_hashCode = ComputeHashCode ();
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="day"></param>
    /// <param name="reasonSlot"></param>
    public ProductionStateSummaryKey (DateTime day, IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.MachineObservationState, reasonSlot.ProductionState, day, reasonSlot.Shift)
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

      ProductionStateSummaryKey other = obj as ProductionStateSummaryKey;
      if (null == other) {
        return false;
      }

      if (this.GetHashCode () != other.GetHashCode ()) {
        return false;
      }

      return m_machine.Equals (other.m_machine)
        && m_machineObservationState.Equals (other.m_machineObservationState)
        && object.Equals (m_productionState, other.m_productionState)
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
        if (null != m_productionState) {
          hashCode += 1000000011 * m_productionState.GetHashCode ();
        }
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
  /// Accumulator for the ProductionStateSummary analysis table
  /// </summary>
  public sealed class ProductionStateSummaryAccumulator
    : Accumulator
    , IReasonSlotAccumulator
  {
    #region Members
    readonly IDictionary<ProductionStateSummaryKey, TimeSpan> m_productionStateSummaryAccumulator =
      new Dictionary<ProductionStateSummaryKey, TimeSpan> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ProductionStateSummaryAccumulator ()
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
      if (reasonSlot.ProductionState is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddReasonSlotDuration: no production state in {reasonSlot}");
        }
        return;
      }

      ProductionStateSummaryKey productionStateSummaryKey =
        new ProductionStateSummaryKey (reasonSlot.Machine, reasonSlot.MachineObservationState, reasonSlot.ProductionState, day, reasonSlot.Shift);
      if (!m_productionStateSummaryAccumulator.TryGetValue (productionStateSummaryKey, out TimeSpan currentProductionStateSummaryValue)) {
        m_productionStateSummaryAccumulator[productionStateSummaryKey] = durationChange;
      }
      else {
        m_productionStateSummaryAccumulator[productionStateSummaryKey] =
          currentProductionStateSummaryValue.Add (durationChange);
      }
    }

    /// <summary>
    /// Add a positive or negative number of slots for the specified productionState slot and day
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
      var withChangeItems = m_productionStateSummaryAccumulator
        .Where (d => 0 != d.Value.Ticks);
      foreach (var productionStateSummaryData in withChangeItems) {
        ProductionStateSummaryKey key = productionStateSummaryData.Key;
        (new ProductionStateSummaryDAO ())
          .UpdateDay (transactionName,
                      key.Machine,
                      key.MachineObservationState,
                      key.ProductionState,
                      key.Day,
                      key.Shift,
                      productionStateSummaryData.Value);
      }
      m_productionStateSummaryAccumulator.Clear ();
    }
    #endregion // Methods
  }
}
