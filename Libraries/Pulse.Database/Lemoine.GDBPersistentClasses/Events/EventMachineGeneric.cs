// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventMachineGeneric
  /// </summary>
  [Serializable,
   XmlInclude(typeof(EventMachineMessage))]
  public class EventMachineGeneric: EventGeneric, IEventMachine
  {
    #region Members
    IMachine m_machine;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventMachineGeneric).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated machine
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return m_machine; }
    }
    
    /// <summary>
    /// Monitored machine for XML serialization
    /// </summary>
    [XmlElement("Machine")]
    public virtual Machine XmlSerializationMachine {
      get { return this.Machine as Machine; }
      set { throw new InvalidOperationException (); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventMachineGeneric ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="machine">not null</param>
    protected EventMachineGeneric (IEventLevel level, DateTime dateTime, IMachine machine)
      : base (level, dateTime)
    {
      Debug.Assert (null != machine);
      
      m_machine = machine;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Unproxy the entity for XML serialization
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
    }
    #endregion // Methods
  }
}
