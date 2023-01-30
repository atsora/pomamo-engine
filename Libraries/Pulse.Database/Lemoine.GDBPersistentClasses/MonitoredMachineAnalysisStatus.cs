// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineModuleStatus
  /// 
  /// This new table stores the current status of a machine module
  /// </summary>
  [Serializable]
  public class MonitoredMachineAnalysisStatus: IMonitoredMachineAnalysisStatus
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_monitoredMachine;
    DateTime m_activityAnalysisDateTime;
    int m_activityAnalysisCount;
    #endregion // Members

    static readonly ILog staticLog = LogManager.GetLogger (typeof (MonitoredMachineAnalysisStatus).FullName);
    ILog log = LogManager.GetLogger(typeof (MonitoredMachineAnalysisStatus).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MonitoredMachineAnalysisStatus ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine"></param>
    public MonitoredMachineAnalysisStatus(IMonitoredMachine monitoredMachine)
    {
      this.MonitoredMachine = monitoredMachine;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID of the reference monitored machine
    /// </summary>
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version {
      get { return m_version; }
    }
    
    /// <summary>
    /// Reference to the Machine Module
    /// </summary>
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_monitoredMachine; }
      set
      {
        if (null == value) {
          log.Fatal ("MonitoredMachine can't be null");
          throw new ArgumentNullException ();
        }
        else {
          m_monitoredMachine = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Activity analysis date/time
    /// </summary>
    public virtual DateTime ActivityAnalysisDateTime {
      get { return m_activityAnalysisDateTime; }
      set { m_activityAnalysisDateTime = value; }
    }
    
    /// <summary>
    /// Activity analysis count
    /// </summary>
    public virtual int ActivityAnalysisCount {
      get { return m_activityAnalysisCount; }
      set { m_activityAnalysisCount = value; }
    }

    #endregion // Getters / Setters
  }
}
