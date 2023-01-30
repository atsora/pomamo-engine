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
  /// Persistent class of table EventMachineModuleGeneric
  /// </summary>
  [Serializable]
  public class EventMachineModuleGeneric: EventGeneric, IEventMachineModule
  {
    #region Members
    IMachineModule m_machineModule;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventMachineModuleGeneric).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated machineModule
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
    }
    
    /// <summary>
    /// Associated monitored machine
    /// </summary>
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_machineModule.MonitoredMachine; }
    }
    
    /// <summary>
    /// Associated machine
    /// </summary>
    public virtual IMachine Machine {
      get { return this.MonitoredMachine; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventMachineModuleGeneric ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="machineModule">not null</param>
    protected EventMachineModuleGeneric (IEventLevel level, DateTime dateTime, IMachineModule machineModule)
      : base (level, dateTime)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Unproxy the entity for XML serialization
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
      m_machineModule.Unproxy ();
    }
    #endregion // Methods
  }
}
