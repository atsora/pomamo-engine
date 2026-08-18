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
  /// Persistent class of table MachineStateTemplateItem
  /// </summary>
  [Serializable]
  public class MachineStateTemplateItem: IMachineStateTemplateItem, IVersionable
  {
    int m_id = 0;
    int m_version = 0;
    int m_order = 0;
    IMachineObservationState m_machineObservationState;
    IMachineStateTemplate m_subMachineStateTemplate;
    IShift m_shift;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimePeriodOfDay m_timePeriod;
    DateTime? m_day = null;
    int? m_weekYear = null;
    int? m_weekNumber = null;
    int? m_weekFrequency = null;
    bool m_yearlyRepeat = false;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateItem).FullName);

    /// <summary>
    /// MachineStateTemplateItem Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id => m_id;

    /// <summary>
    /// Order
    /// </summary>
    [XmlIgnore]
    public virtual int Order => m_order;

    /// <summary>
    /// MachineStateTemplateItem Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_version;

    /// <summary>
    /// Reference to the MachineObservationState
    ///
    /// It may only be null when <see cref="SubMachineStateTemplate"/> is set
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }

    /// <summary>
    /// Reference to the MachineObservationState
    /// for Xml Serialization
    /// </summary>
    [XmlElement("MachineObservationState")]
    public virtual MachineObservationState XmlSerializationMachineObservationState {
      get { return this.MachineObservationState as MachineObservationState; }
      set { this.MachineObservationState = value; }
    }

    /// <summary>
    /// used to serialize MachineObservationState only when not null
    /// </summary>
    public virtual bool XmlSerializationMachineObservationStateSpecified => null != this.MachineObservationState;

    /// <summary>
    /// Reference to a machine state template that is applied recursively
    ///
    /// nullable
    ///
    /// <see cref="IMachineStateTemplateItem"/>
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate SubMachineStateTemplate
    {
      get { return m_subMachineStateTemplate; }
      set { m_subMachineStateTemplate = value; }
    }

    /// <summary>
    /// Reference to the recursively applied machine state template
    /// for Xml Serialization
    /// </summary>
    [XmlElement ("SubMachineStateTemplate")]
    public virtual MachineStateTemplate XmlSerializationSubMachineStateTemplate
    {
      get { return this.SubMachineStateTemplate as MachineStateTemplate; }
      set { this.SubMachineStateTemplate = value; }
    }

    /// <summary>
    /// used to serialize SubMachineStateTemplate only when not null
    /// </summary>
    public virtual bool XmlSerializationSubMachineStateTemplateSpecified => null != this.SubMachineStateTemplate;

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

    /// <summary>
    /// Year of the applicable specific week
    ///
    /// <see cref="IMachineStateTemplateItem"/>
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
    /// <see cref="IMachineStateTemplateItem"/>
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
    /// <see cref="IMachineStateTemplateItem"/>
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
    /// Repeat the item every year
    ///
    /// <see cref="IMachineStateTemplateItem"/>
    /// </summary>
    [XmlAttribute ("YearlyRepeat")]
    public virtual bool YearlyRepeat
    {
      get { return m_yearlyRepeat; }
      set { m_yearlyRepeat = value; }
    }

    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineStateTemplateItem ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineObservationState">Can be null. This is required when a new line is added in the configuration DataGridView</param>
    internal protected MachineStateTemplateItem (IMachineObservationState machineObservationState)
    {
      m_machineObservationState = machineObservationState;
    }

    /// <summary>
    /// Constructor for an item that applies recursively another machine state template
    /// </summary>
    /// <param name="subMachineStateTemplate">not null</param>
    internal protected MachineStateTemplateItem (IMachineStateTemplate subMachineStateTemplate)
    {
      Debug.Assert (null != subMachineStateTemplate);
      if (subMachineStateTemplate is null) {
        log.Fatal ("MachineStateTemplateItem: null sub machine state template");
        throw new ArgumentNullException (nameof (subMachineStateTemplate));
      }
      m_subMachineStateTemplate = subMachineStateTemplate;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_subMachineStateTemplate);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineStateTemplateItem {this.Id} Order={this.Order}]";
      }
      else {
        return $"[MachineStateTemplateItem {this.Id}]";
      }
    }
  }
}
