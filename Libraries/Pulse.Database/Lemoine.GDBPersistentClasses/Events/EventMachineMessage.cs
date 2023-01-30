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
  /// Event with a single message for a specific machine
  /// </summary>
  [Serializable]
  public class EventMachineMessage: EventMachineGeneric, IEventMachineMessage
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventMachineMessage).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="machine"></param>
    /// <param name="message"></param>
    internal protected EventMachineMessage (IEventLevel level, DateTime dateTime, IMachine machine, string message)
      : base (level, dateTime, machine)
    {
      SetData ("message", message);
    }
    
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventMachineMessage ()
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Associated message
    /// </summary>
    public virtual string Message {
      get { return this.Data["message"].ToString (); }
    }
    #endregion // Getters / Setters
  }
}
