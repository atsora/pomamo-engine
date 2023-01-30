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
  /// Persistent class of table MachineActivitySummary
  /// 
  /// Analysis table in which are stored by day and
  /// Machine Observation State the time spent
  /// in each machine mode for a given machine.
  /// 
  /// It allows for example to get the total run time.
  /// 
  /// The associated table is partioned by monitored machine
  /// although here it is set as partioned by machine because of the getter/setter use
  /// </summary>
  public class MachineActivitySummary: IMachineActivitySummary
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_day;
    IMachineObservationState m_machineObservationState;
    IMachineMode m_machineMode;
    IShift m_shift;
    TimeSpan m_time;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (MachineActivitySummary).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineActivitySummary()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    internal MachineActivitySummary(IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode)
    {
      this.m_machine = machine;
      this.m_day = day;
      this.m_machineObservationState = machineObservationState;
      this.m_machineMode = machineMode;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <param name="shift"></param>
    internal MachineActivitySummary(IMachine machine, DateTime day, IMachineObservationState machineObservationState, IMachineMode machineMode, IShift shift)
    {
      this.m_machine = machine;
      this.m_day = day;
      this.m_machineObservationState = machineObservationState;
      this.m_machineMode = machineMode;
      this.m_shift = shift;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Summary Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Summary Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the monitored machine
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
    /// Day of the analysis (from cut-off time)
    /// </summary>
    public virtual DateTime Day {
      get { return m_day; }
    }
    
    /// <summary>
    /// Reference to the MachineObservationState
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
    }
    
    /// <summary>
    /// Reference to the machine mode
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
    }
    
    /// <summary>
    /// Reference to the shift
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
    }

    /// <summary>
    /// Total time of the machine mode during the period in seconds
    /// </summary>
    public virtual TimeSpan Time {
      get { return m_time; }
      set { m_time = value; }
    }
    #endregion // Getters / Setters    
  }
}
