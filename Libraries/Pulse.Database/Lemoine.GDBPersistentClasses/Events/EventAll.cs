// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Event
  /// </summary>
  public class EventAll: IEvent
  {
    #region Members
    int m_id = 0;
    IEvent m_event;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Event).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventAll ()
    { }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// EventLongPeriod Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }
    
    /// <summary>
    /// Associated event
    /// </summary>
    public virtual IEvent Event
    {
      get { return this.m_event; }
      set { m_event = value; }
    }
    
    /// <summary>
    /// Event level
    /// </summary>
    public virtual IEventLevel Level {
      get
      {
        if (null == m_event) {
          log.FatalFormat ("Level: m_event is null");
          Debug.Assert (null != m_event);
          return null;
        }
        else {
          return m_event.Level;
        }
      }
    }
    
    /// <summary>
    /// Date/time of the event
    /// </summary>
    public virtual DateTime DateTime {
      get
      {
        if (null == m_event) {
          log.FatalFormat ("DateTime: m_event is null");
          Debug.Assert (null != m_event);
          return DateTime.UtcNow;
        }
        else {
          return m_event.DateTime;
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// Unproxy the entity for XML serialization
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IEvent> (ref m_event);
    }
    #endregion // Methods
  }
}
