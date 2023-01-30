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
  /// Persistent class of table EventLongPeriodConfig
  /// </summary>
  [Serializable]
  public class EventLongPeriodConfig: IEventLongPeriodConfig
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_monitoredMachine;
    IMachineMode m_machineMode;
    IMachineObservationState m_machineObservationState;
    TimeSpan m_triggerDuration;
    IEventLevel m_level;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (EventLongPeriodConfig).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventLongPeriodConfig ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="triggerDuration"></param>
    /// <param name="level"></param>
    public EventLongPeriodConfig(TimeSpan triggerDuration, IEventLevel level)
    {
      this.TriggerDuration = triggerDuration;
      this.Level = level;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// EventLongPeriodConfig Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// EventLongPeriodConfig Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// Associated monitored machine
    /// 
    /// This may be null (whichever machine)
    /// </summary>
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_monitoredMachine; }
      set
      {
        m_monitoredMachine = value;
        if (null == value) {
          log = LogManager.GetLogger(string.Format ("{0}",
                                                    this.GetType ().FullName));
        }
        else {
          log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Associated machine mode
    /// 
    /// It may be null (whichever machine mode)
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
      set { m_machineMode = value; }
    }
    
    /// <summary>
    /// Associated machine observation state
    /// 
    /// It may be null (whichever machine observation state)
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }
    
    /// <summary>
    /// Minimum duration of the period that triggers the event
    /// </summary>
    public virtual TimeSpan TriggerDuration {
      get { return m_triggerDuration; }
      set { m_triggerDuration = value; }
    }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    public virtual IEventLevel Level {
      get { return m_level; }
      set
      {
        if (null == value) {
          log.FatalFormat ("EventLevel can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Level in EventLongPeriodConfig");
        }
        else {
          m_level = value;
        }
      }
    }
    #endregion // Getters / Setters
  }
}
