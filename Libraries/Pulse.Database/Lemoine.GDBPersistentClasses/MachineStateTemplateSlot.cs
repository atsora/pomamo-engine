// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Machine state template slot
  /// </summary>
  [Serializable]
  public class MachineStateTemplateSlot: IMachineStateTemplateSlot, IWithRange
  {
    ILog log = LogManager.GetLogger(typeof (MachineStateTemplateSlot).FullName);

    #region Members
    string m_uuid = new System.Guid ().ToString ();
    IMachine m_machine;
    IMachineStateTemplate m_machineStateTemplate;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineStateTemplateSlot ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal MachineStateTemplateSlot (IMachine machine,
                                                 IMachineStateTemplate machineStateTemplate,
                                                 UtcDateTimeRange range,
                                                 DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.FatalFormat ("MachineStateTemplateSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
      m_machineStateTemplate = machineStateTemplate;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="range"></param>
    protected internal MachineStateTemplateSlot (IMachine machine,
                                                 IMachineStateTemplate machineStateTemplate,
                                                 UtcDateTimeRange range)
      : this (machine, machineStateTemplate, range,
              ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (range))
    {
    }
    
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="observationStateSlot"></param>
    protected internal MachineStateTemplateSlot (IObservationStateSlot observationStateSlot)
      : this (observationStateSlot.Machine, observationStateSlot.MachineStateTemplate,
              observationStateSlot.DateTimeRange, observationStateSlot.DayRange)
    {  }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// UUID
    /// </summary>
    protected virtual string Uuid {
      get { return m_uuid; }
    }
    
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
      protected set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat ("Machine.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machine = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }

    /// <summary>
    /// Reference to the Machine State Template
    /// </summary>
    public virtual IMachineStateTemplate MachineStateTemplate {
      get { return m_machineStateTemplate; }
    }
    
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange {
      get { return m_dateTimeRange; }
      protected set
      {
        m_dateTimeRange = value;
        m_dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (m_dateTimeRange);
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
    
    #region IMergeableItem implementation
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange"></param>
    /// <param name="newDayRange"></param>
    /// <returns></returns>
    public virtual IMachineStateTemplateSlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return new MachineStateTemplateSlot (this.Machine,
                                           this.MachineStateTemplate,
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
    public virtual bool ReferenceDataEquals (IMachineStateTemplateSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals(this.Machine, other.Machine)
        && object.Equals(this.MachineStateTemplate, other.MachineStateTemplate);
    }
    #endregion // IMergeble implementation

    #region Slot implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo(object obj)
    {
      if (obj is MachineStateTemplateSlot) {
        IMachineStateTemplateSlot other = (IMachineStateTemplateSlot) obj;
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
    public virtual int CompareTo(IMachineStateTemplateSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare MachineStateTemplateSlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of MachineStateTemplateSlots from different machines");
    }
    
    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      Debug.Assert (null != this.MachineStateTemplate);
      
      return false;
    }
    #endregion // Slot implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineStateTemplateSlot other)
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
      MachineStateTemplateSlot other = obj as MachineStateTemplateSlot;
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
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineStateTemplateSlot {this.Machine?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[MachineStateTemplateSlot]";
      }
    }
  }
}
