// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.DefaultAccumulators
{
  public class MachineActivitySummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 101;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new MachineActivitySummaryAccumulator ();
    }
  }

  internal sealed class MachineActivitySummaryKey
  {
    readonly IMachine m_machine;
    readonly IMachineObservationState m_machineObservationState;
    readonly IMachineMode m_machineMode;
    readonly IShift m_shift;
    readonly DateTime m_day;
    readonly int m_hashCode;

    /// <summary>
    /// Machine
    /// </summary>
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
    /// Machine mode
    /// </summary>
    public IMachineMode MachineMode
    {
      get { return m_machineMode; }
    }

    /// <summary>
    /// Shift
    /// </summary>
    public IShift Shift
    {
      get { return m_shift; }
    }

    /// <summary>
    /// Day
    /// </summary>
    public DateTime Day
    {
      get { return m_day; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">Not null</param>
    /// <param name="machineObservationState">Not null</param>
    /// <param name="machineMode">Not null</param>
    /// <param name="shift">Nullable</param>
    /// <param name="day"></param>
    public MachineActivitySummaryKey (IMachine machine, IMachineObservationState machineObservationState, IMachineMode machineMode, IShift shift, DateTime day)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != machineMode);

      m_machine = machine;
      m_machineObservationState = machineObservationState;
      m_machineMode = machineMode;
      m_shift = shift;
      m_day = day;
      m_hashCode = ComputeHashCode ();
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="day"></param>
    /// <param name="reasonSlot"></param>
    public MachineActivitySummaryKey (DateTime day, IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.MachineObservationState, reasonSlot.MachineMode, reasonSlot.Shift, day)
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

      MachineActivitySummaryKey other = obj as MachineActivitySummaryKey;
      if (null == other) {
        return false;
      }

      if (this.GetHashCode () != other.GetHashCode ()) {
        return false;
      }

      return m_machine.Equals (other.m_machine)
        && m_machineObservationState.Equals (other.m_machineObservationState)
        && m_machineMode.Equals (other.m_machineMode)
        && object.Equals (m_shift, other.m_shift) // Because m_shift may be null
        && m_day.Equals (other.m_day);
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
        hashCode += 1000000011 * m_machineMode.GetHashCode ();
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
  /// Accumulator for the MachineActivitySummary analysis table
  /// </summary>
  public sealed class MachineActivitySummaryAccumulator
    : Accumulator
    , IAccumulator
    , IReasonSlotAccumulator
  {
    static readonly string CHECK_CONSISTENCY_KEY = "DefaultAccumulators.MachineActivity.CheckConsistency";
    static readonly bool CHECK_CONSISTENCY_DEFAULT = false;

    #region Members
    IDictionary<MachineActivitySummaryKey, TimeSpan> m_machineActivitySummaryAccumulator =
      new Dictionary<MachineActivitySummaryKey, TimeSpan> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineActivitySummaryAccumulator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public MachineActivitySummaryAccumulator ()
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
      MachineActivitySummaryKey machineActivitySummaryKey =
        new MachineActivitySummaryKey (reasonSlot.Machine, reasonSlot.MachineObservationState, reasonSlot.MachineMode,
                                       reasonSlot.Shift, day);
      if (!m_machineActivitySummaryAccumulator.TryGetValue (machineActivitySummaryKey, out var currentMachineActivityDuration)) {
        if (log.IsDebugEnabled) {
          log.Debug ($".{reasonSlot.Machine.Id} AddReasonSlotDuration: new={durationChange} at {day} machineMode={reasonSlot.MachineMode.Id} slotId={reasonSlot.Id} {System.Environment.StackTrace}");
        }
        m_machineActivitySummaryAccumulator[machineActivitySummaryKey] = durationChange;
      }
      else {
        var newDuration = currentMachineActivityDuration.Add (durationChange);
        if (TimeSpan.FromHours (25) < newDuration) {
          log.Error ($"AddReasonSlotDuration: new duration {newDuration} is not valid for machine {reasonSlot.Machine.Id} day={day} reason={reasonSlot.Reason?.Id} {System.Environment.StackTrace}");
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($".{reasonSlot.Machine.Id} AddReasonSlotDuration: change={durationChange} previous={currentMachineActivityDuration} at {day} machineMode={reasonSlot.MachineMode.Id} slotId={reasonSlot.Id} {System.Environment.StackTrace}");
        }
        m_machineActivitySummaryAccumulator[machineActivitySummaryKey] = newDuration;
      }
    }

    /// <summary>
    /// Add a positive or negative number of slots for the specified reason slot and day
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
      // Nothing to do
    }

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_machineActivitySummaryAccumulator
        .Where (d => 0 != d.Value.Ticks);

      if (Lemoine.Info.ConfigSet.LoadAndGet (CHECK_CONSISTENCY_KEY, CHECK_CONSISTENCY_DEFAULT)) {
        if (1 < withChangeItems.GroupBy (x => x.Key.Machine.Id).Count ()) {
          log.Error ($".{transactionName} Store: more than one machine to process, which is unexpected");
        }
        foreach (var groupByDay in withChangeItems.GroupBy (x => x.Key.Day)) {
          var deltaPerDay = groupByDay.Sum (x => x.Value.TotalSeconds);
          if (deltaPerDay < 0.0) {
            log.Fatal ($".{transactionName} Store: for day={groupByDay.Key}, delta={deltaPerDay}s which is unexpected except if there is a manual machine mode change to unknown {System.Environment.StackTrace}");
            foreach (var dataPerDay in groupByDay) {
              log.Info ($".{transactionName}.{dataPerDay.Key.Machine.Id} Store: individual delta={dataPerDay.Value} for day={dataPerDay.Key.Day} machineMode={dataPerDay.Key.MachineMode.Id}");
            }
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($".{transactionName} Store: delta={deltaPerDay} for day={groupByDay.Key.Day}");
          }
        }
      }

      try {
        foreach (KeyValuePair<MachineActivitySummaryKey, TimeSpan> machineActivitySummaryData
                 in withChangeItems) {
          MachineActivitySummaryKey key = machineActivitySummaryData.Key;
          try {
            (new MachineActivitySummaryDAO ())
              .UpdateDay (transactionName,
                          key.Machine,
                          key.MachineObservationState,
                          key.MachineMode,
                          key.Shift,
                          key.Day,
                          machineActivitySummaryData.Value);
          }
          catch (Exception ex1) {
            log.Error ($".{transactionName}.{key.Machine.Id} Store: UpdateDay for day={key.Day} failed", ex1);
            throw;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($".{transactionName} Store: exception in one of the UpdateDay", ex);
        throw;
      }
      finally {
        m_machineActivitySummaryAccumulator.Clear ();
      }
    }
    #endregion // Methods
  }
}
