// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Common;

namespace Lemoine.Business.MachineState
{
  /// <summary>
  /// Machine mode slot
  /// </summary>
  [Serializable]
  public class MachineShiftSlot : IMachineShiftSlot
  {
    ILog log = LogManager.GetLogger<MachineShiftSlot> ();

    #region Members
    IMachine m_machine;
    IShift m_shift;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineShiftSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal MachineShiftSlot (IMachine machine,
                                         IShift shift,
                                         UtcDateTimeRange range,
                                         DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.Fatal ("MachineShiftSlot: null machine");
        throw new ArgumentNullException ("machine");
      }
      m_machine = machine;
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");
      m_shift = shift;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    protected internal MachineShiftSlot (IMachine machine,
                                        IShift shift,
                                        UtcDateTimeRange range)
      : this (machine, shift, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="observationStateSlot"></param>
    protected internal MachineShiftSlot (IObservationStateSlot observationStateSlot)
      : this (observationStateSlot.Machine, observationStateSlot.Shift, observationStateSlot.DateTimeRange, observationStateSlot.DayRange)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    protected internal MachineShiftSlot (IObservationStateSlot firstSlot, IObservationStateSlot lastSlot)
      : this (firstSlot.Machine, firstSlot.Shift, new UtcDateTimeRange (firstSlot.DateTimeRange.Lower, lastSlot.DateTimeRange.Upper, firstSlot.DateTimeRange.LowerInclusive, lastSlot.DateTimeRange.UpperInclusive), new DayRange (firstSlot.DayRange.Lower, lastSlot.DayRange.Upper, firstSlot.DayRange.LowerInclusive, lastSlot.DayRange.UpperInclusive))
    {
      Debug.Assert (firstSlot.Machine.Id == lastSlot.Machine.Id);
      // And same shift
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Reference to the shift
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
    }

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      protected set
      {
        m_dateTimeRange = value;
        m_dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (m_dateTimeRange));
      }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange
    {
      get { return m_dayRange; }
    }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get { return m_dateTimeRange.Duration; }
    }
    #endregion // Getters / Setters

    #region IMergeableItem<IMachineShiftSlot> implementation
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange"></param>
    /// <param name="newDayRange"></param>
    /// <returns></returns>
    public virtual IMachineShiftSlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return new MachineShiftSlot (this.Machine,
                                   this.Shift,
                                   newRange,
                                   newDayRange);
    }

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IMachineShiftSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Shift, other.Shift);
    }
    #endregion // IMergeableItem<IMachineShiftSlot>

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is MachineShiftSlot) {
        IMachineShiftSlot other = (IMachineShiftSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.Error ($"CompareTo: trying to compare slots for different machines {this.Machine} VS {other.Machine}");
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }

      log.Error ($"CompareTo: object {obj} of invalid type");
      throw new ArgumentException ("object is not the right slot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IMachineShiftSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.Error ($"CompareTo: trying to compare ReasonOnlySlots for different machines {this.Machine} VS {other.Machine}");
      throw new ArgumentException ("Comparison of ReasonOnlySlots from different machines");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false; // Consider a period with a null shift is a valid period
    }
    #endregion // IWithRange implementation

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IMachineShiftSlot other)
    {
      return this.Equals ((object)other);
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

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      MachineShiftSlot other = obj as MachineShiftSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode ();
        hashCode += 1000000009 * DateTimeRange.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("[MachineShiftSlot Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }
  }
}
