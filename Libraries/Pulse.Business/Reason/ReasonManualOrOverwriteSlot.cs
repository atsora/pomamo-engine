// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Business.Reason;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Reason manual or overwrite slot
  /// </summary>
  [Serializable]
  public class ReasonManualOrOverwriteSlot
    : IReasonManualOrOverwriteSlot
    , IWithRange
    , IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger (typeof (ReasonManualOrOverwriteSlot).FullName);

    IMachine m_machine;
    IReason m_reason;
    string m_jsonData;
    bool m_overwriteRequired;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;

    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ReasonManualOrOverwriteSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    protected internal ReasonManualOrOverwriteSlot (IMachine machine,
                                                     IReason reason,
                                                     string jsonData,
                                                     bool overwriteRequired,
                                                     UtcDateTimeRange range,
                                                     DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (machine is null) {
        log.Fatal ("ReasonManualOrOverwriteSlot: null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      m_reason = reason;
      m_jsonData = jsonData;
      m_overwriteRequired = overwriteRequired;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="reason"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="range"></param>
    protected internal ReasonManualOrOverwriteSlot (IMachine machine,
                                                     IReason reason,
                                                     string jsonData,
                                                     bool overwriteRequired,
                                                     UtcDateTimeRange range)
      : this (machine, reason, jsonData, overwriteRequired, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }

    /// <summary>
    /// Alternative constructor from ReasonSlot
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    protected internal ReasonManualOrOverwriteSlot (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      if (reasonSlot is null) {
        log.Fatal ("ReasonManualOrOverwriteSlot: null value");
        throw new ArgumentNullException ();
      }

      m_machine = reasonSlot.Machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{m_machine.Id}");

      m_reason = reasonSlot.Reason;
      m_jsonData = reasonSlot.JsonData;
      m_overwriteRequired = reasonSlot.OverwriteRequired;
      m_dateTimeRange = reasonSlot.DateTimeRange;
      m_dayRange = reasonSlot.DayRange;
    }

    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine => m_machine;

    /// <summary>
    /// Reference to the reason
    /// </summary>
    public virtual IReason Reason => m_reason;

    /// <summary>
    /// Json data
    /// </summary>
    public virtual string JsonData => m_jsonData;

    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    public virtual bool OverwriteRequired => m_overwriteRequired;

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      protected set {
        m_dateTimeRange = value;
        m_dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (m_dateTimeRange));
      }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange => m_dayRange;

    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration => m_dateTimeRange.Duration;

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IReasonManualOrOverwriteSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Reason, other.Reason)
        && ReasonData.AreJsonEqual (this.JsonData, other.JsonData)
        // TODO: comparing the display would be probably better if the two Json are not equal
        && object.Equals (this.OverwriteRequired, other.OverwriteRequired);
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is ReasonManualOrOverwriteSlot) {
        var other = (IReasonManualOrOverwriteSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.Error ($"CompareTo: trying to compare slots for different machines {this} {other}");
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
    public virtual int CompareTo (IReasonManualOrOverwriteSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.Error ($"CompareTo: trying to compare ReasonManualOrOverwriteSlots for different machines {this} {other}");
      throw new ArgumentException ("Comparison of ReasonManualOrOverwriteSlots from different machines");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (this.Machine == null) {
        return $"[ReasonManualOrOverwriteSlot {this.DateTimeRange}]";
      }
      else {
        return $"[ReasonManualOrOverwriteSlot {this.Machine.Id} {this.DateTimeRange}]";
      }
    }
    #endregion // IWithRange implementation

    /// <summary>
    /// <see cref="IReasonManualOrOverwriteSlot"/>
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IReasonManualOrOverwriteSlot Clone (UtcDateTimeRange range)
    {
      var clone = new ReasonManualOrOverwriteSlot (this.Machine, this.Reason, this.JsonData, this.OverwriteRequired, range);
      return clone;
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IReasonManualOrOverwriteSlot other)
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
      var other = obj as ReasonOnlySlot;
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
  }
}
