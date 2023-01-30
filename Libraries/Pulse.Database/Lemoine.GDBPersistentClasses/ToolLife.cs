// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.SharedData;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ToolLife.
  /// </summary>
  public class ToolLife: IToolLife
  {
    #region Members
    int m_id = 0; // Needed by NHibernate
    int m_version = 0; // Needed by NHibernate
    IMachineModule m_machineModule;
    IToolPosition m_position;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof(ToolLife).FullName);

    #region Getters / Setters
    /// <summary>
    /// ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version {
      get { return m_version; }
    }
    
    /// <summary>
    /// Machine module
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
    /// Machine module for XML serialization
    /// </summary>
    [XmlElement("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule {
      get { return this.MachineModule as MachineModule; }
      protected set { this.MachineModule = value; }
    }
    
    /// <summary>
    /// Monitored machine
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_machineModule.MonitoredMachine; }
    }
    
    /// <summary>
    /// Monitored machine for XML serialization
    /// </summary>
    [XmlElement("MonitoredMachine")]
    public virtual MonitoredMachine XmlSerializationMonitoredMachine {
      get { return this.MonitoredMachine as MonitoredMachine; }
      set { throw new InvalidOperationException (); }
    }
    
    /// <summary>
    /// Position
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IToolPosition Position {
      get { return m_position; }
      protected set {
        Debug.Assert(value != null, "Position cannot be null");
        if (value == null) {
          log.Fatal("ToolLife.Position cannot be null");
          throw new ArgumentNullException("Position");
        }
        m_position = value;
      }
    }
    
    /// <summary>
    /// Position for XML serialization
    /// </summary>
    [XmlAttribute("Position")]
    public virtual ToolPosition XmlSerializationToolPosition {
      get { return this.Position as ToolPosition; }
      protected set { this.Position = value; }
    }
    
    /// <summary>
    /// Different way to count the life of the tool
    /// </summary>
    [XmlAttribute("Direction")]
    public virtual ToolLifeDirection Direction { get; protected set; }
    
    /// <summary>
    /// Current value of the tool life
    /// </summary>
    [XmlAttribute("Value")]
    public virtual double Value { get; set; }
    
    /// <summary>
    /// Value above or below which a warning is raised,
    /// announcing the imminent expiration.
    /// 
    /// May be null if the warning is not configured
    /// </summary>
    [XmlAttribute("Warning")]
    public virtual double? Warning { get; set; }
    
    /// <summary>
    /// Value above or below which the expiration occurs.
    /// 
    /// May be null if the limit is not configured.
    /// </summary>
    [XmlAttribute("Limit")]
    public virtual double? Limit { get; set; }
    
    /// <summary>
    /// Unit of the values (type of life)
    /// </summary>
    [XmlAttribute("Unit")]
    public virtual IUnit Unit { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor only accessible for NHibernate
    /// </summary>
    protected internal ToolLife() {}
    
    /// <summary>
    /// Constructor accessible for everyone
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="position"></param>
    /// <param name="unit"></param>
    /// <param name="direction"></param>
    public ToolLife(IMachineModule machineModule, IToolPosition position, IUnit unit, ToolLifeDirection direction)
    {
      MachineModule = machineModule;
      Position = position;
      Unit = unit;
      Direction = direction;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Determines whether the specified Object is equal to the current Object
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
      var other = obj as ToolLife;
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
        return $"[ToolLife {this.Id} {this.MachineModule?.ToStringIfInitialized ()} {this.Position?.ToStringIfInitialized ()} {this.Unit?.ToStringIfInitialized ()} Direction={this.Direction}]";
      }
      else {
        return $"[ToolLife {this.Id}]";
      }
    }
    #endregion // Methods
  }
}
