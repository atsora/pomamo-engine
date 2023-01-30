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
  public sealed class ReasonSummaryAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 102;

    public bool Initialize () { return true; }

    public IAccumulator Create ()
    {
      return new ReasonSummaryAccumulator ();
    }
  }

  internal sealed class ReasonSummaryKey
  {
    readonly IMachine m_machine;
    readonly IMachineObservationState m_machineObservationState;
    readonly IReason m_reason;
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
    /// Reason
    /// </summary>
    public IReason Reason
    {
      get { return m_reason; }
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
    /// <param name="reason">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    public ReasonSummaryKey (IMachine machine, IMachineObservationState machineObservationState, IReason reason, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != reason);

      m_machine = machine;
      m_machineObservationState = machineObservationState;
      m_reason = reason;
      m_day = day;
      m_shift = shift;
      m_hashCode = ComputeHashCode ();
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="day"></param>
    /// <param name="reasonSlot"></param>
    public ReasonSummaryKey (DateTime day, IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.MachineObservationState, reasonSlot.Reason, day, reasonSlot.Shift)
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

      ReasonSummaryKey other = obj as ReasonSummaryKey;
      if (null == other) {
        return false;
      }

      if (this.GetHashCode () != other.GetHashCode ()) {
        return false;
      }

      return m_machine.Equals (other.m_machine)
        && m_machineObservationState.Equals (other.m_machineObservationState)
        && m_reason.Equals (other.m_reason)
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
        hashCode += 1000000011 * m_reason.GetHashCode ();
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
  /// Accumulator for the ReasonSummary analysis table
  /// </summary>
  public sealed class ReasonSummaryAccumulator
    : Accumulator
    , IReasonSlotAccumulator
  {
    sealed class ReasonSummaryValue
    {
      readonly TimeSpan m_duration;
      readonly int m_number;

      /// <summary>
      /// Duration
      /// </summary>
      public TimeSpan Duration
      {
        get { return m_duration; }
      }

      /// <summary>
      /// Number
      /// </summary>
      public int Number
      {
        get { return m_number; }
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="duration"></param>
      /// <param name="number"></param>
      public ReasonSummaryValue (TimeSpan duration,
                                 int number)
      {
        m_duration = duration;
        m_number = number;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="duration"></param>
      public ReasonSummaryValue (TimeSpan duration)
      {
        m_duration = duration;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="number"></param>
      public ReasonSummaryValue (int number)
      {
        m_number = number;
      }

      /// <summary>
      /// Add a duration
      /// </summary>
      /// <param name="duration"></param>
      /// <returns></returns>
      public ReasonSummaryValue AddDuration (TimeSpan duration)
      {
        return new ReasonSummaryValue (m_duration.Add (duration),
                                       m_number);
      }

      /// <summary>
      /// Add a number
      /// </summary>
      /// <param name="offset"></param>
      /// <returns></returns>
      public ReasonSummaryValue AddNumber (int offset)
      {
        return new ReasonSummaryValue (m_duration,
                                       m_number + offset);
      }
    };

    #region Members
    readonly IDictionary<ReasonSummaryKey, ReasonSummaryValue> m_reasonSummaryAccumulator =
      new Dictionary<ReasonSummaryKey, ReasonSummaryValue> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSummaryAccumulator).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonSummaryAccumulator ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add some new data in the Summary accumulator
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    /// <param name="day"></param>
    /// <param name="durationChange"></param>
    public void AddReasonSlotDuration (IReasonSlot reasonSlot,
                                       DateTime day,
                                       TimeSpan durationChange)
    {
      Debug.Assert (null != reasonSlot);

      if (null == reasonSlot.Reason) { // Not yet consolidated... skip it
        Debug.Assert (false);
        log.Fatal ($"AddReasonSlotDuration: no reason in reason slot with id {reasonSlot?.Id} at {reasonSlot?.DateTimeRange}");
        return;
      }

      if ((int)ReasonId.Processing == reasonSlot.Reason.Id) {
        if (log.IsDebugEnabled) {
          log.Debug ($".{reasonSlot.Machine?.Id} AddReasonSlotDuration: processing at {reasonSlot?.DateTimeRange} => skip it");
        }
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($".{reasonSlot.Machine?.Id} AddReasonSlotDuration: change={durationChange} day={day} newRange={reasonSlot.DateTimeRange} reasonId={reasonSlot.Reason?.Id} {System.Environment.StackTrace}");
      }

      ReasonSummaryKey reasonSummaryKey =
        new ReasonSummaryKey (day, reasonSlot);
      if (!m_reasonSummaryAccumulator.TryGetValue (reasonSummaryKey, out var currentReasonSummaryValue)) {
        if (TimeSpan.FromHours (25) < durationChange) {
          log.Error ($"AddReasonSlotDuration: new duration {durationChange} is not valid for machine {reasonSlot.Machine.Id} day={day} reason={reasonSlot.Reason?.Id} {System.Environment.StackTrace}");
        }
        m_reasonSummaryAccumulator[reasonSummaryKey] = new ReasonSummaryValue (durationChange);
      }
      else {
        var newValue = currentReasonSummaryValue.AddDuration (durationChange);
        if (TimeSpan.FromHours (25) < newValue.Duration) {
          log.Error ($"AddReasonSlotDuration: new duration {newValue.Duration} (change is {durationChange}) is not valid for machine {reasonSlot.Machine.Id} day={day} reason={reasonSlot.Reason?.Id} {System.Environment.StackTrace}");
        }
        m_reasonSummaryAccumulator[reasonSummaryKey] = newValue;
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
      if (null == reasonSlot.Reason) { // Not yet consolidated... skip it
        Debug.Assert (false);
        log.Fatal ($"AddReasonSlotNumber: no reason in  reason slot id={reasonSlot?.Id} at {reasonSlot?.DateTimeRange}");
        return;
      }

      if ((int)ReasonId.Processing == reasonSlot.Reason.Id) {
        if (log.IsDebugEnabled) {
          log.Debug ($".{reasonSlot.Machine?.Id} AddReasonSlotDuration: processing at {reasonSlot?.DateTimeRange} => skip it");
        }
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($".{reasonSlot.Machine?.Id} AddReasonSlotNumber: number={number} day={day} newRange={reasonSlot.DateTimeRange} reasonId={reasonSlot.Reason?.Id} {System.Environment.StackTrace}");
      }

      ReasonSummaryKey reasonSummaryKey =
        new ReasonSummaryKey (day, reasonSlot);
      ReasonSummaryValue currentReasonSummaryValue;
      if (!m_reasonSummaryAccumulator.TryGetValue (reasonSummaryKey, out currentReasonSummaryValue)) {
        m_reasonSummaryAccumulator[reasonSummaryKey] = new ReasonSummaryValue (number);
      }
      else {
        m_reasonSummaryAccumulator[reasonSummaryKey] =
          currentReasonSummaryValue.AddNumber (number);
      }
    }

    /// <summary>
    /// Save the data in the database and reset the internal values
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      var withChangeItems = m_reasonSummaryAccumulator
        .Where (d => (0 != d.Value.Duration.Ticks) || (0 != d.Value.Number));
      foreach (KeyValuePair<ReasonSummaryKey, ReasonSummaryValue> reasonSummaryData
               in withChangeItems) {
        ReasonSummaryKey key = reasonSummaryData.Key;
        if ((int)ReasonId.Processing == key.Reason.Id) {
          log.Fatal ($".{key.Machine?.Id} Store: unexpected processing reason id");
        }
        else {
          (new ReasonSummaryDAO ())
          .UpdateDay (transactionName,
                      key.Machine,
                      key.MachineObservationState,
                      key.Reason,
                      key.Day,
                      key.Shift,
                      reasonSummaryData.Value.Duration,
                      reasonSummaryData.Value.Number);
        }
      }
      m_reasonSummaryAccumulator.Clear ();
    }
    #endregion // Methods
  }
}
