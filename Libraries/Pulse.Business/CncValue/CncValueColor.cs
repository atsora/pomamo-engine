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

namespace Lemoine.Business.CncValue
{
  /// <summary>
  /// Cnc value color slot
  /// </summary>
  [Serializable]
  public class CncValueColor : ICncValueColor
  {
    ILog log = LogManager.GetLogger (typeof (CncValueColor).FullName);

    #region Members
    IMachineModule m_machineModule;
    IField m_field;
    string m_color;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    TimeSpan? m_duration;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected CncValueColor ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="color"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    /// <param name="duration"></param>
    protected internal CncValueColor (IMachineModule machineModule,
                                      IField field,
                                      string color,
                                      UtcDateTimeRange range,
                                      DayRange dayRange,
                                      TimeSpan? duration)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      if (null == machineModule) {
        log.Fatal ("CncValueColor: null machine module value");
        throw new ArgumentNullException ("machineModule");
      }
      m_machineModule = machineModule;
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machineModule.Id}");

      if (null == field) {
        log.Fatal ($"CncValueColor: null field value");
        throw new ArgumentNullException ("field");
      }
      m_field = field;

      m_color = color;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
      m_duration = duration;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="color"></param>
    /// <param name="range"></param>
    /// <param name="duration"></param>
    protected internal CncValueColor (IMachineModule machineModule,
                                      IField field,
                                      string color,
                                      UtcDateTimeRange range,
                                      TimeSpan? duration)
      : this (machineModule, field, color, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)), duration)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="cncValue">not null</param>
    protected internal CncValueColor (ICncValue cncValue)
      : this (cncValue.MachineModule, cncValue.Field, GetColor (cncValue),
              cncValue.DateTimeRange, cncValue.DateTimeRange.Duration)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="cncValue">not null</param>
    protected internal CncValueColor (IField field, ICncValue cncValue)
      : this (cncValue.MachineModule, field, GetColor (field, cncValue),
              cncValue.DateTimeRange, cncValue.DateTimeRange.Duration)
    {
    }

    static string GetColor (ICncValue cncValue)
    {
      Debug.Assert (null != cncValue);

      return Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.Field.FieldValueColor (cncValue));
    }

    static string GetColor (IField field, ICncValue cncValue)
    {
      Debug.Assert (null != field);
      Debug.Assert (null != cncValue);

      return Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.Field.FieldValueColor (field, cncValue.Value));
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public virtual IMachineModule MachineModule
    {
      get { return m_machineModule; }
    }

    /// <summary>
    /// Reference to the field
    /// </summary>
    public virtual IField Field
    {
      get { return m_field; }
    }

    /// <summary>
    /// Reference to the reason color
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
    public virtual bool ReferenceDataEquals (ICncValueColor other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.MachineModule, other.MachineModule)
        && object.Equals (this.Field, other.Field)
        && string.Equals (this.Color, other.Color, StringComparison.InvariantCultureIgnoreCase);
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
      if (obj is CncValueColor) {
        ICncValueColor other = (ICncValueColor)obj;
        if (other.MachineModule.Equals (this.MachineModule)) {
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
    public virtual int CompareTo (ICncValueColor other)
    {
      if (other.MachineModule.Equals (this.MachineModule)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare CncValueColor " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of CncValueColor from different machines");
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
    public virtual bool Equals (ICncValueColor other)
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
      CncValueColor other = obj as CncValueColor;
      if (null == other) {
        return false;
      }
      return object.Equals (this.MachineModule, other.MachineModule)
        && object.Equals (this.Field, other.Field)
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
        hashCode += 1000000007 * MachineModule.GetHashCode ();
        hashCode += 1000000009 * Field.GetHashCode ();
        hashCode += 1000000011 * DateTimeRange.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("[CncValueColor MachineModule={0} Field={1} Range={2}]",
                           this.MachineModule.Id, this.Field.Id, this.DateTimeRange);
    }

    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public ICncValueColor Clone (UtcDateTimeRange range, TimeSpan? duration)
    {
      if (UtcDateTimeRange.Equals (range, this.DateTimeRange)) {
        ICncValueColor clone = new CncValueColor (this.MachineModule,
                                                  this.Field,
                                                  this.Color,
                                                  this.DateTimeRange,
                                                  this.DayRange,
                                                  duration);
        return clone;
      }
      else {
        ICncValueColor clone = new CncValueColor (this.MachineModule,
                                                  this.Field,
                                                  this.Color,
                                                  range,
                                                  duration);
        return clone;
      }
    }
  }
}
