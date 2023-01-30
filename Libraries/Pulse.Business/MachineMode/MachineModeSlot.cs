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

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Machine mode slot
  /// </summary>
  [Serializable]
  public class MachineModeSlot: IMachineModeSlot
  {
    ILog log = LogManager.GetLogger(typeof (MachineModeSlot).FullName);

    #region Members
    IMachine m_machine;
    IMachineMode m_machineMode;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    IList<IReasonSubSlot> m_reasonSlots = new List<IReasonSubSlot> ();
    IList<IMachineObservationStateSubSlot> m_machineObservationStateSlots = new List<IMachineObservationStateSubSlot> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineModeSlot ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal MachineModeSlot (IMachine machine,
                                        IMachineMode machineMode,
                                        UtcDateTimeRange range,
                                        DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.FatalFormat ("MachineModeSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
      m_machineMode = machineMode;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineMode"></param>
    /// <param name="range"></param>
    protected internal MachineModeSlot (IMachine machine,
                                        IMachineMode machineMode,
                                        UtcDateTimeRange range)
      : this (machine, machineMode, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }
    
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="reasonSlot"></param>
    protected internal MachineModeSlot (IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.MachineMode, reasonSlot.DateTimeRange, reasonSlot.DayRange)
    {
      {
        IReasonSubSlot subSlot = new ReasonSubSlot (reasonSlot.Reason,
                                                    reasonSlot.ReasonDetails,
                                                    reasonSlot.DefaultReason,
                                                    reasonSlot.DateTimeRange,
                                                    reasonSlot.DayRange);
        m_reasonSlots.Add (subSlot);
      }
      {
        IMachineObservationStateSubSlot subSlot = new MachineObservationStateSubSlot (reasonSlot.MachineObservationState,
                                                                                      reasonSlot.DateTimeRange,
                                                                                      reasonSlot.DayRange);
        m_machineObservationStateSlots.Add (subSlot);
      }
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
    /// Reference to the machine mode
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
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
    
    /// <summary>
    /// Sub-slots
    /// </summary>
    public virtual IList<IReasonSubSlot> ReasonSlots {
      get { return m_reasonSlots; }
    }

    /// <summary>
    /// Sub-slots
    /// </summary>
    public virtual IList<IMachineObservationStateSubSlot> MachineObservationStateSlots {
      get { return m_machineObservationStateSlots; }
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
    public virtual bool ReferenceDataEquals (IMachineModeSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals(this.Machine, other.Machine)
        && object.Equals(this.MachineMode, other.MachineMode);
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
      if (obj is MachineModeSlot) {
        IMachineModeSlot other = (IMachineModeSlot) obj;
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
    public virtual int CompareTo(IMachineModeSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare ReasonOnlySlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of ReasonOnlySlots from different machines");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      Debug.Assert (null != this.MachineMode);
      
      return false;
    }
    #endregion // IWithRange implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineModeSlot other)
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
      MachineModeSlot other = obj as MachineModeSlot;
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
      return string.Format("[MachineModeSlot Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }
  }
}
