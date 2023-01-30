// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MonitoredMachine
  /// 
  /// This table replaces with the Machine and Machine Module tables the old sfkmach table
  /// </summary>
  [Serializable]
  public class MonitoredMachine
    : Machine
    , IMonitoredMachine, IDataGridViewMonitoredMachine
  {
    #region Members
    ICollection <IMachineModule> m_machineModules = new InitialNullIdSet<IMachineModule, int> ();
    IMachineModule m_mainMachineModule;
    IField m_performanceField;
    OperationBar m_operationBar;
    TimeSpan? m_palletChangingDuration;
    bool m_operationFromCnc;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachine).FullName);

    #region Getters / Setters
    /// <summary>
    /// Set of machine modules this monitored machine is made of
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IMachineModule> MachineModules {
      get
      {
        return m_machineModules;
      }
    }
    
    /// <summary>
    /// Reference to the main machine module
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MainMachineModule {
      get { return m_mainMachineModule; }
      set { m_mainMachineModule = value; }
    }

    /// <summary>
    /// Reference to the CncAcquisition of the machine machine module
    /// </summary>
    [XmlIgnore]
    public virtual ICncAcquisition MainCncAcquisition {
      get
      {
        if (null == m_mainMachineModule) {
          return null;
        }
        else { // null != m_mainMachineModule
          return m_mainMachineModule.CncAcquisition;
        }
      }
      set
      {
        if (null == m_mainMachineModule) {
          m_mainMachineModule = new MachineModule (this);
        }
        m_mainMachineModule.CncAcquisition = value;
      }
    }
    
    /// <summary>
    /// Reference to the performance field that is associated to this monitored machine
    /// </summary>
    [XmlIgnore]
    public virtual IField PerformanceField {
      get { return m_performanceField; }
      set { m_performanceField = value; }
    }
    
    /// <summary>
    /// Get/Set what kind of data is displayed in the operation bar
    /// </summary>
    [XmlIgnore]
    public virtual OperationBar OperationBar {
      get { return m_operationBar; }
      set { m_operationBar = value; }
    }
    
    /// <summary>
    /// Pallet changing duration, null if not applicable
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? PalletChangingDuration {
      get { return m_palletChangingDuration; }
      set { m_palletChangingDuration = value; }
    }
    
    /// <summary>
    /// Pallet changing duration as string for XML serialization
    /// </summary>
    [XmlAttribute("PalletChangingDuration")]
    public virtual string PalletChangingDurationAsString {
      get
      {
        return (this.PalletChangingDuration.HasValue)
          ? this.PalletChangingDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.PalletChangingDuration =
          string.IsNullOrEmpty (value)
          ? default (TimeSpan?)
          : TimeSpan.Parse (value);
      }
    }
    
    /// <summary>
    /// Is operation information given by CNC (i.e. stamped files) or not for this machine ?
    /// </summary>
    [XmlIgnore]
    public virtual bool OperationFromCnc {
      get { return m_operationFromCnc; }
      set { m_operationFromCnc = value; }
    }

    /// <summary>
    /// <see cref="IMonitoredMachine"/>
    /// </summary>
    [XmlIgnore]
    public virtual IStampingConfigByName StampingConfigByName { get; set; } = null;
    #endregion // Getters / Setters
    
    #region Constructor
    /// <summary>
    /// Create a new monitored machine
    /// </summary>
    public MonitoredMachine() : base()
    {
      
    }
    
    /// <summary>
    /// Extend a machine to become a monitored machine
    /// </summary>
    /// <param name="machine"></param>
    public MonitoredMachine(IMachine machine)
    {
      this.Category = machine.Category;
      this.SubCategory = machine.SubCategory;
      this.Company = machine.Company;
      this.Department = machine.Department;
      this.Name = machine.Name;
      this.Code = machine.Code;
      this.ExternalCode = machine.ExternalCode;
      this.MonitoringType = machine.MonitoringType;
      this.DisplayPriority = machine.DisplayPriority;
    }
    #endregion // Constructor
    
    #region Add methods
    /// <summary>
    /// Add a machine module
    /// </summary>
    /// <param name="machineModule"></param>
    public virtual void AddMachineModule (IMachineModule machineModule)
    {
      machineModule.MonitoredMachine = this; // Everything is done in the setter
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Add a machine module in the member directly
    /// 
    /// To be used by the MachineModule class only
    /// </summary>
    /// <param name="machineModule"></param>
    protected internal virtual void AddMachineModuleForInternalUse (IMachineModule machineModule)
    {
      AddToProxyCollection<IMachineModule> (m_machineModules, machineModule);
    }
    
    /// <summary>
    /// Remove a machine module in the member directly
    /// 
    /// To be used by the MachineModule class only
    /// </summary>
    /// <param name="machineModule"></param>
    protected internal virtual void RemoveMachineModuleForInternalUse (IMachineModule machineModule)
    {
      RemoveFromProxyCollection<IMachineModule> (m_machineModules, machineModule);
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();    
    }
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMonitoredMachine other)
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
      IMachine other = obj as Machine;
      
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
      if (0 != this.Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * this.Id.GetHashCode();
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
        return $"[MonitoredMachine {this.Id} Name={this.Name}]";
      }
      else {
        return $"[MonitoredMachine {this.Id}]";
      }
    }
  }
}
