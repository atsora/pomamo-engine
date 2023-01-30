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
  [Serializable,
   XmlInclude (typeof (EventGeneric)),
   XmlInclude (typeof (EventUnion))]
  public abstract class Event : IEvent
  {
    #region Members
    int m_id = 0;
    /// <summary>
    /// Level
    /// </summary>
    protected IEventLevel m_level;
    /// <summary>
    /// UTC Date/time
    /// </summary>
    protected DateTime m_dateTime;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Event).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected Event ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level">not null</param>
    /// <param name="dateTime"></param>
    protected Event (IEventLevel level, DateTime dateTime)
    {
      Debug.Assert (null != level);

      m_level = level;
      m_dateTime = dateTime;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// EventLongPeriod Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
      protected internal set { m_id = value; }
    }
    
    /// <summary>
    /// Get the event type: the name of the class
    /// </summary>
    public virtual string Type {
      get { return this.GetType ().Name; }
    }
    
    /// <summary>
    /// Event level
    /// </summary>
    [XmlIgnore]
    public virtual IEventLevel Level {
      get { return m_level; }
      internal protected set // internal protected for Unproxy
      {
        if (null == value) {
          log.FatalFormat ("Level can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException("value", "null Level in EventLongPeriod");
        }
        else {
          m_level = value;
        }
      }
    }
    
    /// <summary>
    /// Event level for XML serialization
    /// </summary>
    [XmlElement("Level")]
    public virtual EventLevel XmlSerializationLevel {
      get { return this.Level as EventLevel; }
      set { this.Level = value; }
    }
    
    /// <summary>
    /// Date/time of the event
    /// </summary>
    [XmlIgnore]
    public virtual DateTime DateTime {
      get { return m_dateTime; }
    }
    
    /// <summary>
    /// Date/time of the modification in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("DateTime")]
    public virtual string SqlDateTime {
      get
      {
        return this.DateTime.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set
      {
        if (String.IsNullOrEmpty(value)) {
          m_dateTime = System.DateTime.UtcNow;
        }
        else {
          m_dateTime = System.DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// Local date/time string of the event
    /// </summary>
    [XmlAttribute("LocalDateTimeString")]
    public virtual string LocalDateTimeString {
      get { return this.DateTime.ToLocalTime ().ToString (); }
      set { m_dateTime = DateTime.SpecifyKind (DateTime.Parse (value), DateTimeKind.Local).ToUniversalTime (); }
    }

    /// <summary>
    /// Local date/time string of the event in G format
    /// 8/18/2015 1:31:17 PM
    /// </summary>
    [XmlAttribute ("LocalDateTimeG")]
    public virtual string LocalDateTimeG
    {
      get { return this.DateTime.ToLocalTime ().ToString ("G"); }
      set { throw new InvalidOperationException ("LocalDateTimeG - deserialization not supported"); }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// Unproxy the entity for XML serialization
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IEventLevel> (ref m_level);
    }
    #endregion // Methods
  }
}
