// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.SharedData;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ToolPosition
  /// </summary>
  [Serializable]
  public class ToolPosition: IToolPosition
  {
    #region Members
    int m_id = 0; // Needed by NHibernate
    int m_version = 0; // Needed by NHibernate
    IMachineModule m_machineModule;
    IDictionary<string, object> m_properties = new Dictionary<string, object>();
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (ToolPosition).FullName);

    #region Getters / Setters
    /// <summary>
    /// Alarm properties
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, object> Properties {
      get {
        if (m_properties == null) {
          m_properties = new Dictionary<string, object>();
        }

        return m_properties;
      }
    }
    
    /// <summary>
    /// ToolPosition Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// ToolPosition Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version {
      get { return m_version; }
    }

    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    public virtual string Display
    {
      get; set;
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
    /// Magazine (first level)
    /// </summary>
    [XmlAttribute("Magazine")]
    public virtual int? Magazine { get; set; }
    
    /// <summary>
    /// Pot (second level)
    /// </summary>
    [XmlAttribute("Pot")]
    public virtual int? Pot { get; set; }
    
    /// <summary>
    /// Tool number (T)
    /// </summary>
    [XmlAttribute("ToolNumber")]
    public virtual string ToolNumber { get; set; }
    
    /// <summary>
    /// Tool id (unique identifier because 2 tools may have the same T)
    /// </summary>
    [XmlAttribute("ToolId")]
    public virtual string ToolId { get; protected set; }
    
    /// <summary>
    /// Current tool state
    /// </summary>
    [XmlAttribute("ToolState")]
    public virtual ToolState ToolState { get; set; }

    /// <summary>
    /// Datetime when the tool disappeared (if it is not considered as a removal)
    /// </summary>
    [XmlAttribute ("LeftDateTime")]
    public virtual DateTime? LeftDateTime { get; set; }

    /// <summary>
    /// Datetime when the tool life changed for the last time
    /// </summary>
    [XmlAttribute ("LifeChangedDateTime")]
    public virtual DateTime? LifeChangedDateTime { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor only accessible for NHibernate
    /// </summary>
    protected internal ToolPosition() {}
    
    /// <summary>
    /// Constructor accessible for everyone
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="toolid"></param>
    public ToolPosition(IMachineModule machineModule, string toolid)
    {
      MachineModule = machineModule;
      ToolId = toolid;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      if (string.IsNullOrEmpty (variant)) {
        return this.Display;
      }
      else {
        log.FatalFormat ("GetDisplay: not implemented for variant {0}",
                         variant);
        throw new NotImplementedException ("ToolPosition.GetDisplay with variant " + variant);
      }
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
      return (this.Id != 0) && (other.Id == this.Id);
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
        return $"[ToolPosition {this.Id} {this.MachineModule?.ToStringIfInitialized ()} Magazine={this.Magazine} Pot={this.Pot} ToolId={this.ToolId}]";
      }
      else {
        return $"[ToolPosition {this.Id}]";
      }
    }
    
    /// <summary>
    /// Get a property from the json column in the database
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns>can be null</returns>
    public virtual object GetProperty(string propertyName)
    {
      return (m_properties != null && m_properties.ContainsKey(propertyName)) ? m_properties[propertyName] : null;
    }
    
    /// <summary>
    /// Set a property into the json column in the database
    /// List of known properties
    /// * LengthCompensation (double)
    /// * CutterCompensation (double)
    /// * GeometryUnit (IUnit)
    /// * ATCSpeed (int: 0=normal, 1=slow, 2=middle)
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public virtual void SetProperty(string propertyName, object value)
    {
      if (m_properties == null) {
        m_properties = new Dictionary<string, object>();
      }

      m_properties[propertyName] = value;
    }
    #endregion // Methods
  }
}
