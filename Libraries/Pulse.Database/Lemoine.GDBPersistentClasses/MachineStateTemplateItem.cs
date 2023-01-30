// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineStateTemplateItem
  /// </summary>
  [Serializable]
  public class MachineStateTemplateItem: IMachineStateTemplateItem, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    int m_order = 0;
    IMachineObservationState m_machineObservationState;
    IShift m_shift;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimePeriodOfDay m_timePeriod;
    DateTime? m_day = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateItem).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineStateTemplateItem Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Order
    /// </summary>
    [XmlIgnore]
    public virtual int Order
    {
      get { return this.m_order; }
    }

    /// <summary>
    /// MachineStateTemplateItem Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the MachineObservationState (not null)
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("MachineObservationState.set: " +
                           "null value");
          throw new ArgumentNullException ("MachineStateTemplateItem.MachineObservationState");
        }
        m_machineObservationState = value;
      }
    }
    
    /// <summary>
    /// Reference to the MachineObservationState
    /// for Xml Serialization
    /// 
    /// It can't be null
    /// </summary>
    [XmlElement("MachineObservationState")]
    public virtual MachineObservationState XmlSerializationMachineObservationState {
      get { return this.MachineObservationState as MachineObservationState; }
      set { this.MachineObservationState = value; }
    }

    /// <summary>
    /// Reference to a shift
    /// 
    /// nullable
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Reference to a Shift
    /// for Xml Serialization
    /// </summary>
    [XmlElement("Shift")]
    public virtual Shift XmlSerializationShift {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }

    /// <summary>
    /// Applicable week days
    /// </summary>
    public virtual WeekDay WeekDays
    {
      get { return m_weekDays; }
      set { m_weekDays = value; }
    }
    
    /// <summary>
    /// Applicable time period of day
    /// </summary>
    [XmlIgnore]
    public virtual TimePeriodOfDay TimePeriod {
      get { return m_timePeriod; }
      set { m_timePeriod = value; }
    }
    
    /// <summary>
    /// Applicable time period of day for XML serialization
    /// </summary>
    [XmlAttribute("TimePeriod")]
    public virtual string XmlTimePeriod {
      get
      {
        if (this.TimePeriod.IsFullDay ()) {
          return "";
        }
        else {
          return this.TimePeriod.ToString ();
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.TimePeriod = new TimePeriodOfDay ();
        }
        else {
          this.TimePeriod = new TimePeriodOfDay (value);
        }
      }
    }
    
    /// <summary>
    /// Applicable specific day
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? Day
    {
      get { return m_day; }
      set { m_day = value; }
    }
    
    /// <summary>
    /// Applicable specific day for Xml serialization
    /// </summary>
    [XmlAttribute("Day")]
    public virtual string XmlDay
    {
      get
      {
        if (!this.Day.HasValue) {
          return "";
        }
        else {
          return this.Day.Value.ToString("yyyy-MM-dd");
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Day = null;
        }
        else {
          this.Day = DateTime.Parse (value);
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineStateTemplateItem ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineObservationState"></param>
    internal protected MachineStateTemplateItem (IMachineObservationState machineObservationState)
    {
      if(machineObservationState != null) { // For the configuration only. This is required when a new line is added in the DataGridView
        this.MachineObservationState = machineObservationState;
      }
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
