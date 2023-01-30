// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.SharedData;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventToolLife
  /// </summary>
  [Serializable]
  public partial class EventToolLife: EventUnion, IEventToolLife
  {
    #region Members
    IMachineModule m_machineModule;
    IMachineObservationState m_machineObservationState;
    IEventToolLifeConfig m_config;
    string m_message = "";
    int? m_oldMagazineNumber = null;
    int? m_newMagazineNumber = null;
    int? m_oldPotNumber = null;
    int? m_newPotNumber = null;
    EventToolLifeType m_eventType = EventToolLifeType.Unknown;
    IUnit m_unit = null;
    double? m_oldValue = null, m_newValue = null;
    double? m_oldWarning = null, m_newWarning = null;
    double? m_oldLimit = null, m_newLimit = null;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (EventToolLife).FullName);

    #region Constructor
    /// <summary>
    /// Default constructor is forbidden (only used by NHibernate)
    /// </summary>
    protected EventToolLife() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="type"></param>
    /// <param name="dateTime"></param>
    /// <param name="machineModule"></param>
    public EventToolLife(IEventLevel level, EventToolLifeType type,
                         DateTime dateTime, IMachineModule machineModule) :
      base(level, dateTime)
    {
      MachineModule = machineModule;
      EventType = type;
      Message = "";
    }
    #endregion // Constructor
    
    #region Getters / Setters
    /// <summary>
    /// Machine module that is associated to the event
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set {
        Debug.Assert (null != value);
        if (value == null) {
          log.Fatal("MachineModule cannot be null");
          throw new ArgumentNullException("MachineModule");
        }
        
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format("{0}.{1}", this.GetType().FullName, value.Id));
      }
    }
    
    /// <summary>
    /// Monitored machine that is associated to the event
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_machineModule.MonitoredMachine; }
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
    /// Type of the event that has occured during the life of the tool
    /// </summary>
    [XmlIgnore]
    public virtual EventToolLifeType EventType {
      get { return m_eventType; }
      protected set { m_eventType = value; }
    }
    
    /// <summary>
    /// EventToolLife name
    /// </summary>
    [XmlElement("Message")]
    public virtual string Message {
      get { return m_message ?? ""; }
      set { m_message = (value ?? ""); }
    }
    
    /// <summary>
    /// Old magazine number, may be null
    /// </summary>
    [XmlIgnore]
    public virtual int? OldMagazine {
      get { return m_oldMagazineNumber; }
      set { m_oldMagazineNumber = value; }
    }
    
    /// <summary>
    /// New magazine number, may be null
    /// </summary>
    [XmlIgnore]
    public virtual int? NewMagazine {
      get { return m_newMagazineNumber; }
      set { m_newMagazineNumber = value; }
    }
    
    /// <summary>
    /// Old pot number, may be null
    /// </summary>
    [XmlIgnore]
    public virtual int? OldPot {
      get { return m_oldPotNumber; }
      set { m_oldPotNumber = value; }
    }
    
    /// <summary>
    /// New pot number, may be null
    /// </summary>
    [XmlIgnore]
    public virtual int? NewPot {
      get { return m_newPotNumber; }
      set { m_newPotNumber = value; }
    }
    
    /// <summary>
    /// Old tool state
    /// </summary>
    [XmlElement("OldToolState")]
    public virtual ToolState OldToolState { get; set; }
    
    /// <summary>
    /// New tool state
    /// </summary>
    [XmlElement("NewToolState")]
    public virtual ToolState NewToolState { get; set; }
    
    /// <summary>
    /// Tool number
    /// </summary>
    [XmlElement("ToolNumber")]
    public virtual string ToolNumber { get; set; }
    
    /// <summary>
    /// Tool id
    /// </summary>
    [XmlElement("ToolId")]
    public virtual string ToolId { get; set; }
    
    /// <summary>
    /// Different way to count the life of a tool
    /// </summary>
    [XmlElement("Direction")]
    public virtual ToolLifeDirection Direction { get; set; }
    
    /// <summary>
    /// Unit characterizing the life
    /// </summary>
    [XmlIgnore]
    public virtual IUnit Unit {
      get { return m_unit; }
      set { m_unit = value; }
    }
    
    /// <summary>
    /// Old life value
    /// </summary>
    [XmlIgnore]
    public virtual double? OldValue {
      get { return m_oldValue; }
      set { m_oldValue = value; }
    }
    
    /// <summary>
    /// New life type
    /// </summary>
    [XmlIgnore]
    public virtual double? NewValue {
      get { return m_newValue; }
      set { m_newValue = value; }
    }
    
    /// <summary>
    /// Old warning value
    /// </summary>
    [XmlIgnore]
    public virtual double? OldWarning {
      get { return m_oldWarning; }
      set { m_oldWarning = value; }
    }
    
    /// <summary>
    /// New warning value
    /// </summary>
    [XmlIgnore]
    public virtual double? NewWarning {
      get { return m_newWarning; }
      set { m_newWarning = value; }
    }
    
    /// <summary>
    /// Old limit value
    /// </summary>
    [XmlIgnore]
    public virtual double? OldLimit {
      get { return m_oldLimit; }
      set { m_oldLimit = value; }
    }
    
    /// <summary>
    /// New limit value
    /// </summary>
    [XmlIgnore]
    public virtual double? NewLimit {
      get { return m_newLimit; }
      set { m_newLimit = value; }
    }
    
    /// <summary>
    /// Elapsed time between the last tool life data and the current tool life data, in milliseconds
    /// </summary>
    [XmlElement("ElapsedTime")]
    public virtual int ElapsedTime { get; set; }
    
    /// <summary>
    /// Machine observation state that is associated to the event
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }
    
    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    [XmlIgnore]
    public virtual IEventToolLifeConfig Config {
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
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
      NHibernateHelper.Unproxy<IEventToolLifeConfig> (ref m_config);
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
      NHibernateHelper.Unproxy<IUnit> (ref m_unit);
    }
    
    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      var other = obj as ToolPosition;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[EventToolLife {this.Id} {this.MachineModule?.ToStringIfInitialized ()} DateTime={this.DateTime} Type={this.EventType}]";
      }
      else {
        return $"[EventToolLife {this.Id}]";
      }
    }
    #endregion // Methods
  }
}
