// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventLongPeriod
  /// </summary>
  [Serializable]
  public class EventLongPeriod: EventUnion, IEventLongPeriod
  {
    #region Members
    IMonitoredMachine m_monitoredMachine;
    IMachineMode m_machineMode;
    IMachineObservationState m_machineObservationState;
    IEventLongPeriodConfig m_config;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (EventLongPeriod).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventLongPeriod ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="monitoredMachine"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="triggerDuration"></param>
    public EventLongPeriod(IEventLevel level, DateTime dateTime, IMonitoredMachine monitoredMachine, IMachineMode machineMode, IMachineObservationState machineObservationState, TimeSpan triggerDuration)
      : base (level, dateTime)
    {
      this.MonitoredMachine = monitoredMachine;
      this.MachineMode = machineMode;
      this.MachineObservationState = machineObservationState;
      this.TriggerDuration = triggerDuration;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Monitored machine that is associated to the event
    /// </summary>
    [XmlIgnore]
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_monitoredMachine; }
      protected set {
        if (null == value) {
          log.FatalFormat ("MonitoredMachine can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null MonitoredMachine in EventLongPeriod");
        } else {
          m_monitoredMachine = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Monitored machine for XML serialization
    /// </summary>
    [XmlElement("MonitoredMachine")]
    public virtual MonitoredMachine XmlSerializationMonitoredMachine {
      get { return this.MonitoredMachine as MonitoredMachine; }
      set { this.MonitoredMachine = value; }
    }
    
    /// <summary>
    /// Machine that is associated to the event
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return this.MonitoredMachine; }
    }

    /// <summary>
    /// Machine mode that is associated to the event
    /// </summary>
    [XmlIgnore]
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
      protected set
      {
        if (null == value) {
          log.FatalFormat ("MachineMode can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null MachineMode in EventLongPeriod");
        }
        else {
          m_machineMode = value;
        }
      }
    }

    /// <summary>
    /// Machine mode for XML serialization
    /// </summary>
    [XmlElement("MachineMode")]
    public virtual MachineMode XmlSerializationMachineMode {
      get { return this.MachineMode as MachineMode; }
      set { this.MachineMode = value; }
    }
    
    /// <summary>
    /// Machine observation state that is associated to the event
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      protected set
      {
        if (null == value) {
          log.FatalFormat ("MachineObservationState can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null MachineObservationState in EventLongPeriod");
        }
        else {
          m_machineObservationState = value;
        }
      }
    }
    
    /// <summary>
    /// Machine observation state for XML serialization
    /// </summary>
    [XmlElement("MachineObservationState")]
    public virtual MachineObservationState XmlSerializationMachineObservationState {
      get { return this.MachineObservationState as MachineObservationState; }
      set { this.MachineObservationState = value; }
    }
    
    /// <summary>
    /// Duration that triggered the event
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan TriggerDuration { get; protected set; }
    
    /// <summary>
    /// Duration that triggered the event for XML serialization
    /// </summary>
    [XmlAttribute ("TriggerDuration")]
    public virtual string XmlSerializationTriggerDuration {
      get { return this.TriggerDuration.ToString (); }
      set { this.TriggerDuration = TimeSpan.Parse (value); }
    }

    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    [XmlIgnore]
    public virtual IEventLongPeriodConfig Config {
      get { return m_config; }
      set { m_config = value; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMonitoredMachine> (ref m_monitoredMachine);
      NHibernateHelper.Unproxy<IMachineMode> (ref m_machineMode);
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
    }
    #endregion // Methods
  }
}
