// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventToolLife
  /// Only functions for serialization here
  /// </summary>
  public partial class EventToolLife
  {
    /// <summary>
    /// Machine module for XML serialization
    /// </summary>
    [XmlElement("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule {
      get { return this.MachineModule as MachineModule; }
      set { this.MachineModule = value; }
    }
    
    /// <summary>
    /// Monitored machine
    /// For XML serialization
    /// </summary>
    [XmlElement("MonitoredMachine")]
    public virtual MonitoredMachine XmlSerializationMonitoredMachine {
      get { return this.MonitoredMachine as MonitoredMachine; }
      set { /* nothing */ }
    }
    
    /// <summary>
    /// Type of the event that has occured during the life of the tool
    /// For xml serialization
    /// </summary>
    [XmlElement("EventType")]
    public virtual EventToolLifeType XmlSerializationEventType {
      get { return m_eventType; }
      set { m_eventType = value; }
    }
    
    /// <summary>
    /// Old magazine number, may be null
    /// For xml serialization
    /// </summary>
    [XmlElement("OldMagazine")]
    public virtual string XmlSerializationOldMagazine {
      get { return GetNullable(m_oldMagazineNumber); }
      set { SetNullable(ref m_oldMagazineNumber, value); }
    }
    
    /// <summary>
    /// New magazine number, may be null
    /// For xml serialization
    /// </summary>
    [XmlElement("NewMagazine")]
    public virtual string XmlSerializationNewMagazine {
      get { return GetNullable(m_newMagazineNumber); }
      set { SetNullable(ref m_newMagazineNumber, value); }
    }
    
    /// <summary>
    /// Old pot number, may be null
    /// For xml serialization
    /// </summary>
    [XmlElement("OldPot")]
    public virtual string XmlSerializationOldPot {
      get { return GetNullable(m_oldPotNumber); }
      set { SetNullable(ref m_oldPotNumber, value); }
    }
    
    /// <summary>
    /// New pot number, may be null
    /// For xml serialization
    /// </summary>
    [XmlElement("NewPot")]
    public virtual string XmlSerializationNewPot {
      get { return GetNullable(m_newPotNumber); }
      set { SetNullable(ref m_newPotNumber, value); }
    }
    
    /// <summary>
    /// Old life value
    /// For xml serialization
    /// </summary>
    [XmlElement("OldValue")]
    public virtual string XmlSerializationOldValue {
      get { return GetNullable(m_oldValue); }
      set { SetNullable(ref m_oldValue, value); }
    }
    
    /// <summary>
    /// New life type
    /// For xml serialization
    /// </summary>
    [XmlElement("NewValue")]
    public virtual string XmlSerializationNewValue {
      get { return GetNullable(m_newValue); }
      set { SetNullable(ref m_newValue, value); }
    }
    
    /// <summary>
    /// Old warning value
    /// For xml serialization
    /// </summary>
    [XmlElement("OldWarning")]
    public virtual string XmlSerializationOldWarning {
      get { return GetNullable(m_oldWarning); }
      set { SetNullable(ref m_oldWarning, value); }
    }
    
    /// <summary>
    /// New warning value
    /// For xml serialization
    /// </summary>
    [XmlElement("NewWarning")]
    public virtual string XmlSerializationNewWarning {
      get { return GetNullable(m_newWarning); }
      set { SetNullable(ref m_newWarning, value); }
    }
    
    /// <summary>
    /// Old limit value
    /// For xml serialization
    /// </summary>
    [XmlElement("OldLimit")]
    public virtual string XmlSerializationOldLimit {
      get { return GetNullable(m_oldLimit); }
      set { SetNullable(ref m_oldLimit, value); }
    }
    
    /// <summary>
    /// New limit value
    /// For xml serialization
    /// </summary>
    [XmlElement("NewLimit")]
    public virtual string XmlSerializationNewLimit {
      get { return GetNullable(m_newLimit); }
      set { SetNullable(ref m_newLimit, value); }
    }
    
    /// <summary>
    /// Unit characterizing the life
    /// For XML serialization
    /// </summary>
    [XmlElement("Unit")]
    public virtual Unit XmlSerializationUnit {
      get { return this.Unit as Unit; }
      set { this.Unit = value; }
    }
    
    /// <summary>
    /// Machine observation state
    /// For XML serialization
    /// </summary>
    [XmlElement("MachineObservationState")]
    public virtual MachineObservationState XmlSerializationMachineObservationState {
      get { return this.MachineObservationState as MachineObservationState; }
      set { this.MachineObservationState = value; }
    }
    
    /// <summary>
    /// Previous date
    /// For XML serialization
    /// </summary>
    [XmlElement("PreviousLocalDateString")]
    public virtual string XmlSerializationPreviousLocalDateString {
      get { return this.DateTime.AddMilliseconds(-ElapsedTime).ToLocalTime().ToString(); }
      set { /* nothing */ }
    }
    
    /// <summary>
    /// Name of the old tool state
    /// For XML serialization
    /// </summary>
    [XmlElement("OldToolStateName")]
    public virtual string XmlSerializationOldToolStateName {
      get { return this.OldToolState.Name(true); }
      set { /* nothing */ }
    }
    
    /// <summary>
    /// Name of the new tool state
    /// For XML serialization
    /// </summary>
    [XmlElement("NewToolStateName")]
    public virtual string XmlSerializationNewToolStateName {
      get { return this.NewToolState.Name(true); }
      set { /* nothing */ }
    }
    
    void SetNullable(ref int? variable, string strValue)
    {
      if (string.IsNullOrEmpty(strValue) || strValue == "-") {
        variable = null;
      }
      else {
        variable = int.Parse(strValue);
      }
    }
    
    void SetNullable(ref double? variable, string strValue)
    {
      if (string.IsNullOrEmpty(strValue) || strValue == "-") {
        variable = null;
      }
      else {
        variable = double.Parse(strValue);
      }
    }
    
    string GetNullable(int? variable)
    {
      return variable.HasValue ? variable.Value.ToString() : "-";
    }
    
    string GetNullable(double? variable)
    {
      return variable.HasValue ? variable.Value.ToString() : "-";
    }
  }
}