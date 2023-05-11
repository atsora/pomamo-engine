// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.GDBMigration;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.Type;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineModule
  /// 
  /// A monitored machine may be made of several independent modules.
  /// Each module reports its own set of data.
  /// A module can represent for example a Machine Path.
  /// </summary>
  [Serializable]
  public class MachineModule: DataWithDisplayFunction, IMachineModule, IVersionable, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    ICncAcquisition m_cncAcquisition;
    string m_configPrefix = "";
    string m_configParameters;
    IMonitoredMachine m_monitoredMachine;
    string m_sequenceVariable = "";
    string m_milestoneVariable = "";
    string m_cycleVariable = "";
    string m_startCycleVariable = "";
    StartCycleDetectionMethod m_startCycleDetectionMethod = StartCycleDetectionMethod.Stamp;
    CycleDetectionMethod m_cycleDetectionMethod = CycleDetectionMethod.Stamp;
    SequenceDetectionMethod m_sequenceDetectionMethod = SequenceDetectionMethod.Stamp;
    string m_detectionMethodVariable = "";
    MachineModuleAutoSequenceActivity m_autoSequenceActivity = MachineModuleAutoSequenceActivity.Machine;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModule).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "ExternalCode", "Code", "Name"}; }
    }

    /// <summary>
    /// ID of the machine module
    /// </summary>
    [XmlIgnore]
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// ID of the machine module for XML serialization
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int XmlSerializationId {
      get { return this.Id; }
      set { m_id = value; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Name of the module
    /// 
    /// Unique for a given associated monitored machine
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Code of the module
    /// 
    /// Unique for a given associated monitored machine
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing our data with an external database
    /// 
    /// Unique
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    
    /// <summary>
    /// Cnc acquisition module that drives this machine module
    /// </summary>
    [XmlIgnore]
    public virtual ICncAcquisition CncAcquisition {
      get { return m_cncAcquisition; }
      set
      {
        if (object.Equals (m_cncAcquisition, value)) {
          // Nothing to do
          return;
        }
        if (null != m_cncAcquisition) {
          // Remove the module from the previous machine
          (m_cncAcquisition as CncAcquisition).RemoveMachineModuleForInternalUse (this);
        }
        m_cncAcquisition = value;
        if (null != m_cncAcquisition) {
          // Add the module to the new machine
          (m_cncAcquisition as CncAcquisition).AddMachineModuleForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Cnc acquisition module that drives this machine module for Xml Serialization
    /// </summary>
    [XmlElement("CncAcquisition")]
    public virtual CncAcquisition XmlSerializationCncAcquisition {
      get { return this.CncAcquisition as CncAcquisition; }
      set { this.CncAcquisition = value; }
    }
    
    /// <summary>
    /// Configuration prefix found in the XML configuration file
    /// 
    /// The default value is the empty string. It can't be null.
    /// </summary>
    [XmlAttribute("ConfigPrefix")]
    public virtual string ConfigPrefix {
      get { return m_configPrefix; }
      set
      {
        if (null == value) {
          log.WarnFormat ("ConfigPrefix.set: " +
                          "null argument " +
                          "=> consider this is an empty string");
          m_configPrefix = "";
        }
        else {
          m_configPrefix = value;
        }
      }
    }
    
    /// <summary>
    /// Parameters to use in the CNC service
    /// 
    /// The first character is the separators that is used to separate the different parameters.
    /// 
    /// The parameters are made in the XML configuration file
    /// by a config prefix and a parameter number
    /// </summary>
    [XmlAttribute("ConfigParameters")]
    public virtual string ConfigParameters {
      get { return m_configParameters; }
      set { m_configParameters = value; }
    }
    
    /// <summary>
    /// Reference to the associated monitored machine
    /// </summary>
    [XmlIgnore]
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_monitoredMachine; }
      set
      {
        if (object.Equals (m_monitoredMachine, value)) {
          // Nothing to do
          return;
        }
        if (null != m_monitoredMachine) {
          // Remove the module from the previous machine
          (m_monitoredMachine as MonitoredMachine).RemoveMachineModuleForInternalUse (this);
        }
        m_monitoredMachine = value;
        if (null != m_monitoredMachine) {
          // Add the module to the new machine
          (m_monitoredMachine as MonitoredMachine).AddMachineModuleForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Reference to the associated monitored machine
    /// for XML serialization
    /// </summary>
    [XmlElement("MonitoredMachine")]
    public virtual MonitoredMachine XmlSerializationMonitoredMachine {
      get { return this.MonitoredMachine as MonitoredMachine; }
      set { this.MonitoredMachine = value; }
    }
    
    /// <summary>
    /// Sequence variable/macro
    /// </summary>
    [XmlIgnore]
    public virtual string SequenceVariable {
      get { return m_sequenceVariable; }
      set { m_sequenceVariable = value; }
    }

    /// <summary>
    /// Sequence milestone variable/macro
    /// </summary>
    [XmlIgnore]
    public virtual string MilestoneVariable
    {
      get { return m_milestoneVariable; }
      set { m_milestoneVariable = value; }
    }

    /// <summary>
    /// Cycle variable/macro
    /// </summary>
    [XmlIgnore]
    public virtual string CycleVariable {
      get { return m_cycleVariable; }
      set { m_cycleVariable = value; }
    }
    
    /// <summary>
    /// Start cycle variable/macro
    /// </summary>
    [XmlIgnore]
    public virtual string StartCycleVariable {
      get { return m_startCycleVariable; }
      set { m_startCycleVariable = value; }
    }
    
    /// <summary>
    /// Sequence detection method
    /// 
    /// Default: stamp (1024)
    /// </summary>
    [XmlIgnore]
    public virtual SequenceDetectionMethod SequenceDetectionMethod {
      get { return m_sequenceDetectionMethod; }
      set { m_sequenceDetectionMethod = value; }
    }

    /// <summary>
    /// End cycle detection method
    /// 
    /// Default: stamp (8)
    /// </summary>
    [XmlIgnore]
    public virtual CycleDetectionMethod CycleDetectionMethod {
      get { return m_cycleDetectionMethod; }
      set { m_cycleDetectionMethod = value; }
    }
    
    /// <summary>
    /// Start cycle detection method
    /// 
    /// Default: stamp (1)
    /// </summary>
    [XmlIgnore]
    public virtual StartCycleDetectionMethod StartCycleDetectionMethod {
      get { return m_startCycleDetectionMethod; }
      set { m_startCycleDetectionMethod = value; }
    }
    
    /// <summary>
    /// Default detection method
    /// 
    /// This is combination of Sequence/Cycle/StartCycle detection methods
    /// </summary>
    [XmlIgnore]
    public virtual DetectionMethod DefaultDetectionMethod {
      get { return (DetectionMethod) (((int) this.SequenceDetectionMethod) | ((int) this.CycleDetectionMethod) | ((int) this.StartCycleDetectionMethod)); }
    }
    
    /// <summary>
    /// Detection method variable/macro
    /// </summary>
    [XmlIgnore]
    public virtual string DetectionMethodVariable {
      get { return m_detectionMethodVariable; }
      set { m_detectionMethodVariable = value; }
    }
    
    /// <summary>
    /// Auto-sequence activity reference data
    /// </summary>
    [XmlIgnore]
    public virtual MachineModuleAutoSequenceActivity AutoSequenceActivity {
      get { return m_autoSequenceActivity; }
      set { m_autoSequenceActivity = value; }
    }
    
    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Name); }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineModule ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine"></param>
    public MachineModule (IMonitoredMachine monitoredMachine)
    {
      m_monitoredMachine = monitoredMachine;
    }
    #endregion // Constructors
        
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMonitoredMachine> (ref m_monitoredMachine);
      NHibernateHelper.Unproxy<ICncAcquisition> (ref m_cncAcquisition);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineModule other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      MachineModule other = obj as MachineModule;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
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
        return base.GetHashCode ();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineModule {this.Id} Name={this.Name}]";
      }
      else {
        return $"[MachineModule {this.Id}]";
      }
    }
  }

  /// <summary>
  /// Convert a MachineModuleAutoSequenceActivity enum to a string in database
  /// </summary>
  [Serializable]
  public class EnumMachineModuleAutoSequenceActivity: EnumStringType<MachineModuleAutoSequenceActivity>
  {
  }  
}
