// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineStateTemplateStop
  /// </summary>
  [Serializable]
  public class MachineStateTemplateStop: IMachineStateTemplateStop, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimeSpan? m_localTime = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateStop).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineStateTemplateStop Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// MachineStateTemplateStop Version
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
    /// 
    /// Fraction of the day that has elapsed since local midnight
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? LocalTime {
      get { return m_localTime; }
      set { m_localTime = value; }
    }
    
    /// <summary>
    /// Applicable time period of day for XML serialization
    /// </summary>
    [XmlAttribute("LocalTime")]
    public virtual string XmlLocalTime {
      get
      {
        if (!this.LocalTime.HasValue) {
          return "";
        }
        else {
          return this.LocalTime.Value.ToString ();
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.LocalTime = null;
        }
        else {
          this.LocalTime = TimeSpan.Parse (value);
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal protected MachineStateTemplateStop ()
    { }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }
  }
}
