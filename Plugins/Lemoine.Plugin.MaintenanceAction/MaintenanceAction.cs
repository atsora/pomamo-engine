// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.Plugin.MaintenanceAction
{
  /// <summary>
  /// Persistent class of table maintenanceaction
  /// </summary>
  [Serializable]
  public class MaintenanceAction: IDataWithId, IDataWithVersion, Lemoine.Model.IPartitionedByMachine
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    string m_title;
    string m_description = "";
    MaintenanceActionType m_maintenanceActionType = MaintenanceActionType.Preventive;
    MaintenanceActionStatus m_status = MaintenanceActionStatus.Open;
    DateTime m_creationDateTime = DateTime.UtcNow;
    DateTime m_modifiedDateTime = DateTime.UtcNow;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Config Id
    /// </summary>
    public virtual int Id {
      get { return this.m_id; }
    }

    /// <summary>
    /// Config Version
    /// </summary>
    public virtual int Version {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine (machine or outsource)
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return m_machine; }
      protected set { m_machine = value; }
    }

    [XmlElement ("Machine")]
    public virtual GDBPersistentClasses.Machine XmlSerializationMachine {
      get { return this.Machine as GDBPersistentClasses.Machine; }
      set { this.Machine = value; }
    }

    [XmlAttribute("Title")]
    public virtual string Title {
      get { return m_title; }
      set { m_title = value; }
    }

    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }

    [XmlIgnore]
    public virtual MaintenanceActionType MaintenanceActionType {
      get { return m_maintenanceActionType; }
      set { m_maintenanceActionType = value; }
    }

    [XmlIgnore]
    public virtual MaintenanceActionStatus Status {
      get { return m_status; }
      set { m_status = value; }
    }

    [XmlIgnore]
    public virtual DateTime CreationDateTime {
      get { return m_creationDateTime; }
    }

    [XmlIgnore]
    public virtual DateTime ModifiedDateTime {
      get { return m_modifiedDateTime; }
      set { m_modifiedDateTime = value; }
    }

    [XmlIgnore]
    public virtual DateTime? CompletionDateTime
    {
      get; set;
    }
    
    /// <summary>
         /// Date/time when the machine stopped for the curative maintenance
         /// </summary>
    [XmlIgnore]
    public virtual DateTime? StopDateTime {
      get; set;
    }

    /// <summary>
    /// Fixed date/time when the maintenance action is scheduled
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? PlannedDateTime {
      get; set;
    }

    /// <summary>
    /// Remaining machining duration before the maintenance action at the creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? RemainingMachiningDuration {
      get; set;
    }

    /// <summary>
    /// For preventive maintenance action, frequency at which the maintenance action should take place in machining duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? StandardMachiningFrequency {
      get; set;
    }

    /// <summary>
    /// For preventive maintenance action, frequency at which the maintenance action should take place
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? StandardTotalFrequency {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="title"></param>
    /// <param name="maintenanceActionType"></param>
    internal protected MaintenanceAction (IMachine machine, string title, MaintenanceActionType maintenanceActionType)
    {
      m_machine = machine;
      m_title = title;
      m_maintenanceActionType = maintenanceActionType;
    }

    protected MaintenanceAction ()
    { }
    #endregion // Constructors
  }
}
