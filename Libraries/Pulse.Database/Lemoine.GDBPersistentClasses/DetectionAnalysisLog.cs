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
  /// Persistent class of table DetectionAnalysisLog
  /// </summary>
  [Serializable]
  public class DetectionAnalysisLog : Log, IDetectionAnalysisLog
  {
    #region Members
    IMachine m_machine;
    IMachineModule m_machineModule;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DetectionAnalysisLog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Referenced Machine (not null)
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Referenced MonitoredMachine (not null) for XML serialization
    /// </summary>
    [XmlElement ("Machine")]
    public virtual Machine XmlSerializationMachine
    {
      get { return this.Machine as Machine; }
      set
      {
        // only XML serialization is supported: do nothing
      }
    }

    /// <summary>
    /// Referenced MachineModule
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MachineModule
    {
      get { return m_machineModule; }
    }

    /// <summary>
    /// Referenced MachineModule for XML serialization
    /// </summary>
    [XmlElement ("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule
    {
      get { return this.MachineModule as MachineModule; }
      set
      {
        // only XML serialization is supported: do nothing
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="machine">can't be null</param>
    /// <param name="machineModule"></param>
    internal DetectionAnalysisLog (LogLevel level,
                                 string message,
                                 IMachine machine,
                                 IMachineModule machineModule)
      : base (level, message)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_machineModule = machineModule;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected DetectionAnalysisLog ()
    {
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
    }
  }
}
