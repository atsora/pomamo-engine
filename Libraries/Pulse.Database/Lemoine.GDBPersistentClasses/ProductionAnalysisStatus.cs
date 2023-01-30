// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProductionAnalysisStatus
  /// </summary>
  [Serializable]
  public class ProductionAnalysisStatus: IProductionAnalysisStatus
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_analysisDateTime = DateTime.UtcNow;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (ProductionAnalysisStatus).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ProductionAnalysisStatus ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public ProductionAnalysisStatus(IMachine machine)
    {
      this.Machine = machine;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID of the referenced machine
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
    /// Reference to the Machine
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
      protected set
      {
        m_machine = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }

    /// <summary>
    /// Date/time up to which the analysis of the production is completed
    /// </summary>
    public virtual DateTime AnalysisDateTime {
      get { return m_analysisDateTime; }
      set { m_analysisDateTime = value; }
    }
    #endregion // Getters / Setters
  }
}
