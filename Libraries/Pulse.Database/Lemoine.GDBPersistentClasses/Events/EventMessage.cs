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
  /// Event with a single message
  /// </summary>
  [Serializable]
  public class EventMessage: EventGeneric, IEventMessage
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventMessage).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal protected EventMessage (IEventLevel level, DateTime dateTime, string message)
      : base (level, dateTime)
    {
      SetData ("message", message);
    }
    
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventMessage ()
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Associated message
    /// </summary>
    public virtual string Message {
      get { return this.Data["message"].ToString (); }
      set { this.Data["message"] = value; }
    }
    #endregion // Getters / Setters
  }
}
