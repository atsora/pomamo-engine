// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventCncValueConfig
  /// </summary>
  [Serializable]
  public class EventCncValueConfig: IEventCncValueConfig
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_message;
    IField m_field;
    IMachineFilter m_machineFilter = null;
    string m_condition;
    TimeSpan m_minDuration = TimeSpan.FromTicks (0);
    IEventLevel m_level;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (EventCncValueConfig).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventCncValueConfig ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="field"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="condition"></param>
    public EventCncValueConfig(string name, IField field, IEventLevel level, string message, string condition)
    {
      this.Name = name;
      this.Field = field;
      this.Level = level;
      this.Message = message;
      this.Condition = condition;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// EventCncValueConfig Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// EventCncValueConfig Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// Name: not null or empty
    /// </summary>
    public virtual string Name
    {
      get { return m_name; }
      set
      {
        if (null == value) {
          log.FatalFormat ("Name can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Name in EventCncValueConfig");
        }
        else {
          if (string.IsNullOrEmpty (value)) {
            log.FatalFormat ("Name can't be empty");
            Debug.Assert (null != value);
            throw new ArgumentException ("empty Name in EventCncValueConfig");
          }
          m_name = value;
        }
      }
    }
    
    /// <summary>
    /// Message: not null
    /// </summary>
    public virtual string Message
    {
      get { return m_message; }
      set
      {
        if (null == value) {
          log.FatalFormat ("Message can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Message in EventCncValueConfig");
        }
        else {
          m_message = value;
        }
      }
    }
    
    /// <summary>
    /// Associated field
    ///
    /// not null
    /// </summary>
    public virtual IField Field {
      get { return m_field; }
      protected set
      {
        if (null == value) {
          log.FatalFormat ("Field can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Field in EventCncValueConfig");
        }
        else {
          m_field = value;
        }
      }
    }
    
    /// <summary>
    /// Associated machine filter
    /// 
    /// This may be null (whichever machine)
    /// </summary>
    public virtual IMachineFilter MachineFilter {
      get { return m_machineFilter; }
      set { m_machineFilter = value; }
    }
    
    /// <summary>
    /// Condition (not null)
    /// </summary>
    public virtual string Condition {
      get { return m_condition; }
      set
      {
        if (null == value) {
          log.FatalFormat ("Condition can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Condition in EventCncValueConfig");
        }
        else {
          m_condition = value;
        }
      }
    }
    
    /// <summary>
    /// Minimum duration of the period that triggers the event
    /// </summary>
    public virtual TimeSpan MinDuration {
      get { return m_minDuration; }
      set { m_minDuration = value; }
    }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    public virtual IEventLevel Level {
      get { return m_level; }
      set
      {
        if (null == value) {
          log.FatalFormat ("EventLevel can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Level in EventCncValueConfig");
        }
        else {
          m_level = value;
        }
      }
    }
    #endregion // Getters / Setters
  }
}
