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
  /// Persistent class of table OperationSlotSplit
  /// </summary>
  [Serializable]
  public class OperationSlotSplit: IOperationSlotSplit
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_end = DateTime.UtcNow;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (OperationSlotSplit).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected OperationSlotSplit ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public OperationSlotSplit(IMachine machine)
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
    /// Date/time up to which the operation slots are split
    /// </summary>
    public virtual DateTime End {
      get { return m_end; }
      set { m_end = value; }
    }
    #endregion // Getters / Setters
  }
}
