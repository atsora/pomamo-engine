// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ShiftTemplateBreak
  /// </summary>
  [Serializable]
  public class ShiftTemplateBreak: IShiftTemplateBreak, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimePeriodOfDay m_timePeriod;
    DateTime? m_day = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateBreak).FullName);

    #region Getters / Setters
    /// <summary>
    /// ShiftTemplateBreak Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// ShiftTemplateBreak Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
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
    internal protected ShiftTemplateBreak ()
    { }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
    }
  }
}
