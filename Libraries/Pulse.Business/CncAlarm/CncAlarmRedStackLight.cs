// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.CncAlarm
{
  /// <summary>
  /// Cnc value color slot
  /// </summary>
  [Serializable]
  public class CncAlarmRedStackLight
  {
    ILog log = LogManager.GetLogger (typeof (CncAlarmRedStackLight).FullName);

    #region Members
    IMonitoredMachine m_machine;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    TimeSpan? m_duration;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected CncAlarmRedStackLight ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    /// <param name="duration"></param>
    protected internal CncAlarmRedStackLight (IMonitoredMachine machine,
                                              UtcDateTimeRange range,
                                              DayRange dayRange,
                                              TimeSpan? duration)
    {
      Debug.Assert (null != machine);

      if (null == machine) {
        log.FatalFormat ("CncAlarmRedStackLight: " +
                         "null machine module value");
        throw new ArgumentNullException ("machine");
      }
      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));

      m_dateTimeRange = range;
      m_dayRange = dayRange;
      m_duration = duration;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="duration"></param>
    protected internal CncAlarmRedStackLight (IMonitoredMachine machine,
                                              UtcDateTimeRange range,
                                              TimeSpan? duration)
      : this (machine, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)), duration)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTimeRange"></param>
    protected internal CncAlarmRedStackLight (IMonitoredMachine machine, UtcDateTimeRange dateTimeRange)
      : this (machine, dateTimeRange, dateTimeRange.Duration)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="cncAlarm"></param>
    /// <param name="redStackLight"></param>
    public CncAlarmRedStackLight (ICncAlarm cncAlarm, ICncValue redStackLight)
      : this (cncAlarm.MachineModule.MonitoredMachine, new UtcDateTimeRange (cncAlarm.DateTimeRange.Intersects (redStackLight.DateTimeRange)))
    {
      Debug.Assert (null != cncAlarm);
      Debug.Assert (null != redStackLight);
      Debug.Assert (cncAlarm.MachineModule.MonitoredMachine.Id == redStackLight.MachineModule.MonitoredMachine.Id);
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the monitored machine
    /// </summary>
    public virtual IMonitoredMachine Machine
    {
      get { return m_machine; }
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
      get { return m_duration; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (CncAlarmRedStackLight other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine);
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
      if (obj is CncAlarmRedStackLight) {
        CncAlarmRedStackLight other = (CncAlarmRedStackLight)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare slots " +
                           "for different machines {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not the right slot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (CncAlarmRedStackLight other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare CncAlarmRedStackLight " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of CncAlarmRedStackLight from different machines");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false;
    }
    #endregion // IWithRange implementation

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (CncAlarmRedStackLight other)
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
      CncAlarmRedStackLight other = obj as CncAlarmRedStackLight;
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
      return string.Format ("[CncAlarmRedStackLight Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }

    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public CncAlarmRedStackLight Clone (UtcDateTimeRange range, TimeSpan? duration)
    {
      CncAlarmRedStackLight clone = new CncAlarmRedStackLight (this.Machine,
                                                               range,
                                                               duration);
      return clone;
    }
  }
}
