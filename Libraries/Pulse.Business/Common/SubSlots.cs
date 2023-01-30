// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.Common
{
  /// <summary>
  /// Machine mode sub-slot for IReasonOnlySlot
  /// </summary>
  public class MachineModeSubSlot: IMachineModeSubSlot
  {
    ILog log = LogManager.GetLogger(typeof (MachineModeSubSlot).FullName);
    
    #region Members
    IMachineMode m_machineMode;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineModeSubSlot ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal MachineModeSubSlot (IMachineMode machineMode,
                                           UtcDateTimeRange range,
                                           DayRange dayRange)
    {
      Debug.Assert (null != machineMode);
      if (null == machineMode) {
        log.FatalFormat ("MachineModeSubSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machineMode = machineMode;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    #endregion // Constructors
    
    #region Getters / Setters
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
    public virtual IMachineModeSubSlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return new MachineModeSubSlot (this.MachineMode,
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
    public virtual bool ReferenceDataEquals (IMachineModeSubSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals(this.MachineMode, other.MachineMode);
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
      if (obj is MachineModeSubSlot) {
        IMachineModeSubSlot other = (IMachineModeSubSlot) obj;
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
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
    public virtual int CompareTo(IMachineModeSubSlot other)
    {
      return this.DateTimeRange.CompareTo (other.DateTimeRange);
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
    #endregion // Slot implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineModeSubSlot other)
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
      MachineModeSubSlot other = obj as MachineModeSubSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
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
      return string.Format("[ReasonOnlyMachineMode Machine={0} Range={1}]",
                           this.MachineMode.Id, this.DateTimeRange);
    }
  }
  
  /// <summary>
  /// Machine observation state sub-slot for IReasonOnlySlot
  /// </summary>
  public class MachineObservationStateSubSlot: IMachineObservationStateSubSlot
  {
    ILog log = LogManager.GetLogger(typeof (MachineObservationStateSubSlot).FullName);
    
    #region Members
    IMachineObservationState m_machineObservationState;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineObservationStateSubSlot ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineObservationState">not null</param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal MachineObservationStateSubSlot (IMachineObservationState machineObservationState,
                                                       UtcDateTimeRange range,
                                                       DayRange dayRange)
    {
      Debug.Assert (null != machineObservationState);
      if (null == machineObservationState) {
        log.FatalFormat ("MachineObservationStateSubSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machineObservationState = machineObservationState;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the machine mode
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
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
    
    #region IMergeableItem implementation
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange"></param>
    /// <param name="newDayRange"></param>
    /// <returns></returns>
    public virtual IMachineObservationStateSubSlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return new MachineObservationStateSubSlot (this.MachineObservationState,
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
    public virtual bool ReferenceDataEquals (IMachineObservationStateSubSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals(this.MachineObservationState, other.MachineObservationState);
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
      if (obj is MachineObservationStateSubSlot) {
        IMachineObservationStateSubSlot other = (IMachineObservationStateSubSlot) obj;
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
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
    public virtual int CompareTo(IMachineObservationStateSubSlot other)
    {
      return this.DateTimeRange.CompareTo (other.DateTimeRange);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      Debug.Assert (null != this.MachineObservationState);
      
      return false;
    }
    #endregion // Slot implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineObservationStateSubSlot other)
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
      MachineObservationStateSubSlot other = obj as MachineObservationStateSubSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
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
      return string.Format("[ReasonOnlySubSlot Machine={0} Range={1}]",
                           this.MachineObservationState.Id, this.DateTimeRange);
    }
  }

  /// <summary>
  /// Reason sub-slot for IMachineModeSlot
  /// </summary>
  public class ReasonSubSlot: IReasonSubSlot
  {
    ILog log = LogManager.GetLogger(typeof (ReasonSubSlot).FullName);
    
    #region Members
    IReason m_reason;
    string m_reasonDetails;
    bool m_defaultReason;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ReasonSubSlot ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="reasonDetails"></param>
    /// <param name="defaultReason"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal ReasonSubSlot (IReason reason,
                                      string reasonDetails,
                                      bool defaultReason,
                                      UtcDateTimeRange range,
                                      DayRange dayRange)
    {
      Debug.Assert (null != reason);
      if (null == reason) {
        log.FatalFormat ("MachineModeReason: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_reason = reason;
      m_reasonDetails = reasonDetails;
      m_defaultReason = defaultReason;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the reason
    /// </summary>
    public virtual IReason Reason {
      get { return m_reason; }
    }
    
    /// <summary>
    /// Reference to the reason details
    /// </summary>
    public virtual string ReasonDetails {
      get { return m_reasonDetails; }
    }
    
    /// <summary>
    /// Reference to the default reason property
    /// </summary>
    public virtual bool DefaultReason {
      get { return m_defaultReason; }
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
    
    #region IMergeableItem implementation
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange"></param>
    /// <param name="newDayRange"></param>
    /// <returns></returns>
    public virtual IReasonSubSlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return new ReasonSubSlot (this.Reason,
                                this.ReasonDetails,
                                this.DefaultReason,
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
    public virtual bool ReferenceDataEquals (IReasonSubSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Reason, other.Reason)
        && object.Equals (this.ReasonDetails, other.ReasonDetails)
        && object.Equals (this.DefaultReason, other.DefaultReason);
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
      if (obj is ReasonSubSlot) {
        IReasonSubSlot other = (IReasonSubSlot) obj;
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
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
    public virtual int CompareTo(IReasonSubSlot other)
    {
      return this.DateTimeRange.CompareTo (other.DateTimeRange);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      Debug.Assert (null != this.Reason);
      
      return false;
    }
    #endregion // Slot implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IReasonSubSlot other)
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
      ReasonSubSlot other = obj as ReasonSubSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
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
      return string.Format("[ReasonSubSlot Reason={0} Range={1}]",
                           this.Reason.Id, this.DateTimeRange);
    }
  }
}
