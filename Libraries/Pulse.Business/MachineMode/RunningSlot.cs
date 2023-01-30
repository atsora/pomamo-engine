// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Running only slot
  /// </summary>
  [Serializable]
  public class RunningSlot: IRunningSlot
  {
    ILog log = LogManager.GetLogger(typeof (RunningSlot).FullName);

    #region Members
    IMachine m_machine;
    bool m_running;
    bool m_notRunning;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected RunningSlot ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="running"></param>
    /// <param name="notRunning"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal RunningSlot (IMachine machine,
                                    bool running,
                                    bool notRunning,
                                    UtcDateTimeRange range,
                                    DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.FatalFormat ("RunningSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
      m_running = running;
      m_notRunning = notRunning;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="running"></param>
    /// <param name="notRunning"></param>
    /// <param name="range"></param>
    protected internal RunningSlot (IMachine machine,
                                    bool running,
                                    bool notRunning,
                                    UtcDateTimeRange range)
      : this (machine, running, notRunning, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }
    
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="reasonSlot"></param>
    protected internal RunningSlot (IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.Running, reasonSlot.NotRunning, reasonSlot.DateTimeRange, reasonSlot.DayRange)
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
    }

    /// <summary>
    /// Running
    /// </summary>
    public virtual bool Running {
      get { return m_running; }
    }
    
    /// <summary>
    /// Running
    /// </summary>
    public virtual bool NotRunning {
      get { return m_notRunning; }
    }
    
    /// <summary>
    /// <see cref="IRunningSlot" />
    /// </summary>
    public virtual bool? NullableRunning {
      get
      {
        if (m_running) {
          return true;
        }
        else if (m_notRunning) {
          return false;
        }
        else {
          return null;
        }
      }
    }
    
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange {
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
    public virtual DayRange DayRange {
      get { return m_dayRange; }
    }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration {
      get { return m_dateTimeRange.Duration; }
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
    public virtual bool ReferenceDataEquals (IRunningSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals(this.Machine, other.Machine)
        && object.Equals(this.Running, other.Running)
        && object.Equals(this.NotRunning, other.NotRunning);
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo(object obj)
    {
      if (obj is RunningSlot) {
        IRunningSlot other = (IRunningSlot) obj;
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
    public virtual int CompareTo(IRunningSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare RunningSlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of RunningSlots from different machines");
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
    public virtual bool Equals(IRunningSlot other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      RunningSlot other = obj as RunningSlot;
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
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode();
        hashCode += 1000000009 * DateTimeRange.GetHashCode();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("[RunningSlot Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }
    
    /// <summary>
    /// Clone the running slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IRunningSlot Clone (UtcDateTimeRange range)
    {
      IRunningSlot clone = new RunningSlot (this.Machine,
                                            this.Running,
                                            this.NotRunning,
                                            range);
      return clone;
    }
  }
}
