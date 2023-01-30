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
  /// Persistent class of table EventGeneric
  /// </summary>
  [Serializable,
   XmlInclude(typeof(EventMachineGeneric)),
   XmlInclude(typeof(EventMessage))]
  public abstract class EventGeneric: Event, IEvent
  {
    #region Members
    IDictionary<string, object> m_data = new Dictionary<string, object> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventGeneric).FullName);

    #region Getters / Setters
    /// <summary>
    /// Event data
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, object> Data {
      get { return m_data; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventGeneric ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    protected EventGeneric (IEventLevel level, DateTime dateTime)
      : base (level, dateTime)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    protected void SetData (string key, object v)
    {
      m_data[key] = v;
    }
    #endregion // Methods
  }
}
