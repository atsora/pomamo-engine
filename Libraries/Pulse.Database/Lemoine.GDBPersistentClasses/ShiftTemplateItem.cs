// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
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
  /// Persistent class of table ShiftTemplateItem
  /// </summary>
  [Serializable]
  public class ShiftTemplateItem: IShiftTemplateItem, IVersionable
  {
    int m_id = 0;
    int m_version = 0;
    IShift m_shift;
    IShiftTemplate m_subShiftTemplate;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimePeriodOfDay m_timePeriod;
    DateTime? m_day = null;
    int? m_weekYear = null;
    int? m_weekNumber = null;
    int? m_weekFrequency = null;

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateItem).FullName);

    /// <summary>
    /// ShiftTemplateItem Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id => this.m_id;

    /// <summary>
    /// ShiftTemplateItem Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => this.m_version;

    /// <summary>
    /// Reference to a shift
    ///
    /// It may only be null when <see cref="SubShiftTemplate"/> is set
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
    /// used to serialize Shift only when not null
    /// </summary>
    public virtual bool XmlSerializationShiftSpecified => null != this.Shift;

    /// <summary>
    /// Reference to a shift template that is applied recursively
    ///
    /// nullable
    ///
    /// <see cref="IShiftTemplateItem"/>
    /// </summary>
    [XmlIgnore]
    public virtual IShiftTemplate SubShiftTemplate
    {
      get { return m_subShiftTemplate; }
      set { m_subShiftTemplate = value; }
    }

    /// <summary>
    /// Reference to the recursively applied shift template
    /// for Xml Serialization
    /// </summary>
    [XmlElement ("SubShiftTemplate")]
    public virtual ShiftTemplate XmlSerializationSubShiftTemplate
    {
      get { return this.SubShiftTemplate as ShiftTemplate; }
      set { this.SubShiftTemplate = value; }
    }

    /// <summary>
    /// used to serialize SubShiftTemplate only when not null
    /// </summary>
    public virtual bool XmlSerializationSubShiftTemplateSpecified => null != this.SubShiftTemplate;

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

    /// <summary>
    /// Year of the applicable specific week
    ///
    /// <see cref="IWeekRestrictedItem"/>
    /// </summary>
    [XmlIgnore]
    public virtual int? WeekYear
    {
      get { return m_weekYear; }
      set { m_weekYear = value; }
    }

    /// <summary>
    /// Year of the applicable specific week, for Xml serialization
    /// </summary>
    [XmlAttribute ("WeekYear")]
    public virtual int XmlWeekYear
    {
      get { return m_weekYear.Value; }
      set { m_weekYear = value; }
    }

    /// <summary>
    /// used to serialize WeekYear only when not null
    /// </summary>
    public virtual bool XmlWeekYearSpecified => m_weekYear.HasValue;

    /// <summary>
    /// Number of the applicable specific week
    ///
    /// <see cref="IWeekRestrictedItem"/>
    /// </summary>
    [XmlIgnore]
    public virtual int? WeekNumber
    {
      get { return m_weekNumber; }
      set
      {
        if (value.HasValue && ((value.Value < 1) || (53 < value.Value))) {
          log.Fatal ($"WeekNumber.set: invalid week number {value.Value}");
          throw new ArgumentOutOfRangeException (nameof (value), "Week number must be between 1 and 53");
        }
        m_weekNumber = value;
      }
    }

    /// <summary>
    /// Number of the applicable specific week, for Xml serialization
    /// </summary>
    [XmlAttribute ("WeekNumber")]
    public virtual int XmlWeekNumber
    {
      get { return m_weekNumber.Value; }
      set { this.WeekNumber = value; }
    }

    /// <summary>
    /// used to serialize WeekNumber only when not null
    /// </summary>
    public virtual bool XmlWeekNumberSpecified => m_weekNumber.HasValue;

    /// <summary>
    /// Repeat the item every x weeks from the reference week
    ///
    /// <see cref="IWeekRestrictedItem"/>
    /// </summary>
    [XmlIgnore]
    public virtual int? WeekFrequency
    {
      get { return m_weekFrequency; }
      set
      {
        if (value.HasValue && (value.Value < 1)) {
          log.Fatal ($"WeekFrequency.set: invalid week frequency {value.Value}");
          throw new ArgumentOutOfRangeException (nameof (value), "Week frequency must be strictly positive");
        }
        m_weekFrequency = value;
      }
    }

    /// <summary>
    /// Repeat the item every x weeks from the reference week, for Xml serialization
    /// </summary>
    [XmlAttribute ("WeekFrequency")]
    public virtual int XmlWeekFrequency
    {
      get { return m_weekFrequency.Value; }
      set { this.WeekFrequency = value; }
    }

    /// <summary>
    /// used to serialize WeekFrequency only when not null
    /// </summary>
    public virtual bool XmlWeekFrequencySpecified => m_weekFrequency.HasValue;

    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected ShiftTemplateItem ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shift">Can be null. This is required when a new line is added in the configuration DataGridView</param>
    internal protected ShiftTemplateItem (IShift shift)
    {
      m_shift = shift;
    }

    /// <summary>
    /// Constructor for an item that applies recursively another shift template
    /// </summary>
    /// <param name="subShiftTemplate">not null</param>
    internal protected ShiftTemplateItem (IShiftTemplate subShiftTemplate)
    {
      Debug.Assert (null != subShiftTemplate);
      if (subShiftTemplate is null) {
        log.Fatal ("ShiftTemplateItem: null sub shift template");
        throw new ArgumentNullException (nameof (subShiftTemplate));
      }
      m_subShiftTemplate = subShiftTemplate;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
      NHibernateHelper.Unproxy<IShiftTemplate> (ref m_subShiftTemplate);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ShiftTemplateItem {this.Id} Shift={this.Shift?.ToStringIfInitialized ()} SubShiftTemplate={this.SubShiftTemplate?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[ShiftTemplateItem {this.Id}]";
      }
    }
  }
}
