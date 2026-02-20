// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Reason overwrite required slot
  /// </summary>
  [Serializable]
  public class ReasonOverwriteRequiredSlot
    : IReasonOverwriteRequiredSlot
    , IWithRange
    , IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger (typeof (ReasonOverwriteRequiredSlot).FullName);

    IMachine m_machine;
    string m_color;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;

    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ReasonOverwriteRequiredSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="color"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal ReasonOverwriteRequiredSlot (IMachine machine,
                                                     string color,
                                                     UtcDateTimeRange range,
                                                     DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (machine is null) {
        log.Fatal ("ReasonOverwriteRequiredSlot: null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      m_color = color;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="color"></param>
    /// <param name="range"></param>
    protected internal ReasonOverwriteRequiredSlot (IMachine machine,
                                                     string color,
                                                     UtcDateTimeRange range)
      : this (machine, color, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }

    /// <summary>
    /// Alternative constructor from ReasonSlot
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    protected internal ReasonOverwriteRequiredSlot (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      if (reasonSlot is null) {
        log.Fatal ("ReasonOverwriteRequiredSlot: null value");
        throw new ArgumentNullException ();
      }

      m_machine = reasonSlot.Machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{m_machine.Id}");

      m_color = reasonSlot.Reason?.Color ?? string.Empty;
      m_dateTimeRange = reasonSlot.DateTimeRange;
      m_dayRange = reasonSlot.DayRange;
    }

    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Reference to the reason color
    /// 
    /// An empty string may be returned
    /// </summary>
    public virtual string Color
    {
      get { return m_color; }
    }

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

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IReasonOverwriteRequiredSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Color, other.Color);
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
      if (obj is ReasonOverwriteRequiredSlot) {
        IReasonOverwriteRequiredSlot other = (IReasonOverwriteRequiredSlot)obj;
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
    public virtual int CompareTo (IReasonOverwriteRequiredSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.Error ($"CompareTo: trying to compare ReasonOverwriteRequiredSlots for different machines {this} {other}");
      throw new ArgumentException ("Comparison of ReasonOverwriteRequiredSlots from different machines");
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
        return $"[ReasonOverwriteRequiredSlot {this.DateTimeRange}]";
      }
      else {
        return $"[ReasonOverwriteRequiredSlot {this.Machine.Id} {this.DateTimeRange}]";
      }
    }
    #endregion // IWithRange implementation

    /// <summary>
    /// <see cref="IReasonOverwriteRequiredSlot"/>
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IReasonOverwriteRequiredSlot Clone (UtcDateTimeRange range)
    {
      IReasonOverwriteRequiredSlot clone = new ReasonOverwriteRequiredSlot (this.Machine,
                                                                             this.Color,
                                                                             range);
      return clone;
    }
  }
}
