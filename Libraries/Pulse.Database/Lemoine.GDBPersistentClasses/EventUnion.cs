// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Event
  /// </summary>
  [Serializable,
   XmlInclude(typeof(EventLongPeriod)),
   XmlInclude(typeof(EventCncValue)),
   XmlInclude(typeof(EventToolLife))]
  public abstract class EventUnion: Event, IEvent
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Event).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventUnion ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level">not null</param>
    /// <param name="dateTime"></param>
    protected EventUnion(IEventLevel level, DateTime dateTime)
      : base (level, dateTime)
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    #endregion // Getters / Setters
    
    #region Methods
    #endregion // Methods
  }
}
