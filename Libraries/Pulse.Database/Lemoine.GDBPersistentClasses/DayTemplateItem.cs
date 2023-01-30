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
  /// Persistent class of table DayTemplateItem
  /// </summary>
  [Serializable]
  public class DayTemplateItem: IDayTemplateItem, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimeSpan m_cutOff;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateItem).FullName);

    #region Getters / Setters
    /// <summary>
    /// DayTemplateItem Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// DayTemplateItem Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Applicable cut-off
    /// 
    /// Fraction of the day that has elapsed since local midnight
    /// 
    /// Positive or Negative
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan CutOff {
      get { return m_cutOff; }
      set { m_cutOff = value; }
    }
    
    /// <summary>
    /// Applicable cut-off for XML serialization
    /// </summary>
    [XmlAttribute("CutOff")]
    public virtual string XmlCutOff {
      get
      {
        return this.CutOff.ToString ();
      }
      set
      {
        this.CutOff = TimeSpan.Parse (value);
      }
    }

    /// <summary>
    /// Applicable week days
    /// </summary>
    [XmlIgnore]
    public virtual WeekDay WeekDays
    {
      get { return m_weekDays; }
      set { m_weekDays = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected DayTemplateItem ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cutOff"></param>
    /// <param name="weekDays"></param>
    internal protected DayTemplateItem (TimeSpan cutOff, WeekDay weekDays)
    {
      this.CutOff = cutOff;
      this.WeekDays = weekDays;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
    }
  }
}
