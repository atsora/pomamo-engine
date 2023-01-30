// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineModuleAnalysisStatus
  /// 
  /// This new table stores the current analysis status of a machine module
  /// </summary>
  [Serializable]
  public class MachineModuleAnalysisStatus: IMachineModuleAnalysisStatus
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    int m_lastMachineModuleDetectionId = 0;
    DateTime m_autoSequenceAnalysisDateTime = DateTime.UtcNow;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (MachineModuleAnalysisStatus).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineModuleAnalysisStatus ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public MachineModuleAnalysisStatus(IMachineModule machineModule)
    {
      this.MachineModule = machineModule;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID of the reference machine module
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
    /// Reference to the MachineModule
    /// 
    /// not null
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    
    /// <summary>
    /// Id of the last analyzed MachineModuleDetection
    /// </summary>
    public virtual int LastMachineModuleDetectionId {
      get { return m_lastMachineModuleDetectionId; }
      set { m_lastMachineModuleDetectionId = value; }
    }
    
    /// <summary>
    /// UTC Date/time of the auto-sequence analysis
    /// </summary>
    public virtual DateTime AutoSequenceAnalysisDateTime {
      get { return m_autoSequenceAnalysisDateTime; }
      set { m_autoSequenceAnalysisDateTime = value; }
    }
    #endregion // Getters / Setters
  }
}
