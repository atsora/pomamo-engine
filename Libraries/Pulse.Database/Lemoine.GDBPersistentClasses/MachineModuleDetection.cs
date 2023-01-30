// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineModuleDetection
  /// </summary>
  [Serializable]
  public class MachineModuleDetection: IMachineModuleDetection
  {
    #region Members
    int m_id = 0;
    IMachineModule m_machineModule;
    DateTime m_dateTime;
    string m_name = null;
    IStamp m_stamp = null;
    bool m_stopNCProgram = false;
    bool m_startCycle = false;
    bool m_stopCycle = false;
    int? m_quantity = null;
    string m_operationCode = null;
    string m_customType = null;
    string m_customValue = null;
    TimeSpan? m_sequenceMilestone;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (MachineModuleDetection).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineModuleDetection ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    public MachineModuleDetection (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      
      this.MachineModule = machineModule;
      this.DateTime = dateTime;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// MachineModuleDetection Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Reference to the machine module
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        Debug.Assert(null != value);
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    
    /// <summary>
    /// Date/time of the machine module detection
    /// </summary>
    public virtual DateTime DateTime {
      get { return m_dateTime; }
      protected set
      {
        switch (value.Kind) {
          case DateTimeKind.Unspecified:
            log.WarnFormat ("DateTime.set: " +
                            "unspecified DateTimeKind => suppose it is a universal time");
            m_dateTime = new DateTime (value.Ticks, DateTimeKind.Utc);
            break;
          case DateTimeKind.Utc:
            m_dateTime = value;
            break;
          case DateTimeKind.Local:
            m_dateTime = value.ToUniversalTime ();
            break;
          default:
            throw new Exception("Invalid value for DateTimeKind");
        }
      }
    }
    
    /// <summary>
    /// Name of the detection
    /// 
    /// nullable
    /// </summary>
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Reference to a stamp
    /// 
    /// nullable
    /// </summary>
    public virtual IStamp Stamp {
      get { return m_stamp; }
      set { m_stamp = value; }
    }
    
    /// <summary>
    /// Stop a NC Program
    /// </summary>
    public virtual bool StopNcProgram {
      get { return m_stopNCProgram; }
      set { m_stopNCProgram = value; }
    }
    
    /// <summary>
    /// Start a cycle
    /// </summary>
    public virtual bool StartCycle {
      get { return m_startCycle; }
      set { m_startCycle = value; }
    }
    
    /// <summary>
    /// Stop a cycle
    /// </summary>
    public virtual bool StopCycle {
      get { return m_stopCycle; }
      set { m_stopCycle = value; }
    }

    /// <summary>
    /// Part quantity for the cycle
    /// </summary>
    public virtual int? Quantity {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    
    /// <summary>
    /// Operation code
    /// </summary>
    public virtual string OperationCode
    {
      get { return m_operationCode; }
      set { m_operationCode = value; }
    }

    /// <summary>
    /// Custom type
    /// </summary>
    public virtual string CustomType
    {
      get { return m_customType; }
      set { m_customType = value; }
    }

    /// <summary>
    /// Custom type
    /// </summary>
    public virtual string CustomValue
    {
      get { return m_customValue; }
      set { m_customValue = value; }
    }

    /// <summary>
    /// Sequence milestone
    /// </summary>
    public virtual TimeSpan? SequenceMilestone
    {
      get { return m_sequenceMilestone; }
      set { m_sequenceMilestone = value; }
    }
    #endregion // Getters / Setters
  }
}
