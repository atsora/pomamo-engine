// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EmailConfig
  /// </summary>
  public class EmailConfig: IEmailConfig, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_dataType;
    string m_freeFilter;
    string m_editor;
    int m_maxLevelPriority = 1000;
    IEventLevel m_eventLevel;
    IMachine m_machine;
    IMachineFilter m_machineFilter;
    string m_to;
    string m_cc;
    string m_bcc;
    bool m_active = true;
    WeekDay m_weekDays = (WeekDay) Int32.MaxValue;
    TimePeriodOfDay m_timePeriod;
    DateTime? m_beginDateTime = null;
    DateTime? m_endDateTime = null;
    bool m_autoPurge = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EmailConfig).FullName);

    #region Getters / Setters
    /// <summary>
    /// EmailConfig Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// EmailConfig Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Config name
    /// </summary>
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Data type:
    /// <item>EventLongPeriod</item>
    /// <item>EventCncValue</item>
    /// <item>Log</item>
    /// </summary>
    public virtual string DataType
    {
      get { return m_dataType; }
      set { m_dataType = value; }
    }
    
    /// <summary>
    /// A free filter to be really flexible
    /// </summary>
    public virtual string FreeFilter
    {
      get { return m_freeFilter; }
      set { m_freeFilter = value; }
    }
    
    /// <summary>
    /// A free filter to be really flexible
    /// </summary>
    public virtual string Editor
    {
      get { return m_editor; }
      set { m_editor = value; }
    }
    
    /// <summary>
    /// Maximum level priority to take into account (positive)
    /// 
    /// Default is 1000 (all)
    /// </summary>
    public virtual int MaxLevelPriority
    {
      get { return m_maxLevelPriority; }
      set
      {
        if (value < 0) {
          log.ErrorFormat ("MaxLevelPriority.set: " +
                           "invalid value {0}",
                          value);
          throw new ArgumentOutOfRangeException ("Positive value expected");
        }
        m_maxLevelPriority = value;
      }
    }
    
    /// <summary>
    /// Event level filter
    /// 
    /// Default is null: no filter
    /// </summary>
    public virtual IEventLevel EventLevel
    {
      get { return m_eventLevel; }
      set { m_eventLevel = value; }
    }
    
    /// <summary>
    /// Machine filter
    /// 
    /// Default is null: no filter
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
      set { m_machine = value; }
    }
    
    /// <summary>
    /// MachineFilter filter
    /// 
    /// Default is null: no filter
    /// </summary>
    public virtual IMachineFilter MachineFilter
    {
      get { return m_machineFilter; }
      set { m_machineFilter = value; }
    }
    
    /// <summary>
    /// To: addresses
    /// </summary>
    public virtual string To
    {
      get { return m_to; }
      set { m_to = value; }
    }
    
    /// <summary>
    /// Cc: addresses
    /// </summary>
    public virtual string Cc
    {
      get { return m_cc; }
      set { m_cc = value; }
    }
    
    /// <summary>
    /// Bcc: addresses
    /// </summary>
    public virtual string Bcc
    {
      get { return m_bcc; }
      set { m_bcc = value; }
    }
    
    /// <summary>
    /// Is the configuration active ?
    /// </summary>
    public virtual bool Active {
      get { return m_active; }
      set { m_active = value; }
    }
    
    /// <summary>
    /// Applicable week days (local)
    /// </summary>
    public virtual WeekDay WeekDays
    {
      get { return m_weekDays; }
      set { m_weekDays = value; }
    }
    
    /// <summary>
    /// Applicable time period of day (local)
    /// </summary>
    public virtual TimePeriodOfDay TimePeriod
    {
      get { return m_timePeriod; }
      set { m_timePeriod = value; }
    }
    
    /// <summary>
    /// Only applicable from the specified UTC date/time
    /// 
    /// If null, applicable from now
    /// </summary>
    public virtual DateTime? BeginDateTime
    {
      get { return m_beginDateTime; }
      set { m_beginDateTime = value; }
    }
    
    /// <summary>
    /// Only applicable from the specified local date/time
    /// 
    /// If null, applicable from now
    /// </summary>
    public virtual DateTime? LocalBegin
    {
      get
      {
        if (m_beginDateTime.HasValue) {
          return m_beginDateTime.Value.ToLocalTime ();
        }
        else {
          return null;
        }
      }
      set
      {
        if (!value.HasValue) {
          m_beginDateTime = null;
        }
        else {
          DateTime local = value.Value;
          if (DateTimeKind.Unspecified == local.Kind) {
            local = DateTime.SpecifyKind (local, DateTimeKind.Local);
          }
          m_beginDateTime = local.ToUniversalTime ();
        }
      }
    }
    
    /// <summary>
    /// Only applicable from the specified local date/time
    /// 
    /// If empty, applicable from now
    /// 
    /// The format "g" is used
    /// </summary>
    public virtual string LocalBeginString
    {
      get
      {
        if (!m_beginDateTime.HasValue) {
          return "";
        }
        else {
          return this.LocalBegin.Value.ToString ("g");
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          m_beginDateTime = null;
        }
        else {
          this.LocalBegin = DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// Only applicable to the specified UTC date/time
    /// 
    /// If null, no limit
    /// </summary>
    public virtual DateTime? EndDateTime
    {
      get { return m_endDateTime; }
      set { m_endDateTime = value; }
    }
    
    /// <summary>
    /// Only applicable to the specified local date/time
    /// 
    /// If null, no limit
    /// </summary>
    public virtual DateTime? LocalEnd
    {
      get
      {
        if (m_endDateTime.HasValue) {
          return m_endDateTime.Value.ToLocalTime ();
        }
        else {
          return null;
        }
      }
      set
      {
        if (!value.HasValue) {
          m_endDateTime = null;
        }
        else {
          DateTime local = value.Value;
          if (DateTimeKind.Unspecified == local.Kind) {
            local = DateTime.SpecifyKind (local, DateTimeKind.Local);
          }
          m_endDateTime = local.ToUniversalTime ();
        }
      }
    }
    
    /// <summary>
    /// Only applicable to the specified local date/time
    /// 
    /// If empty, no limit
    /// 
    /// The format "g" is used
    /// </summary>
    public virtual string LocalEndString
    {
      get
      {
        if (!m_endDateTime.HasValue) {
          return "";
        }
        else {
          return this.LocalEnd.Value.ToString ("g");
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          m_endDateTime = null;
        }
        else {
          this.LocalEnd = DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// Purge the automatically the item if EndDateTime was reached
    /// 
    /// Default is false
    /// </summary>
    public virtual bool AutoPurge
    {
      get { return m_autoPurge; }
      set { m_autoPurge = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal protected EmailConfig ()
    {
    }
    #endregion // Constructors
  }
}
