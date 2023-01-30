// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table TaskMachineAssociation
  /// </summary>
  [Serializable]
  public class TaskMachineAssociation
    : MachineModification
    , ITaskMachineAssociation
    // Note: public else it is not serializable for the alert service
  {
    #region Members
    UtcDateTimeRange m_range;
    ITask m_task;
    AssociationOption? m_associationOption;
    bool m_partOfDetectionAnalysis;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "TaskMachineAssociation"; }
    }

    /// <summary>
    /// Range
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange Range
    {
      get { return this.m_range; }
    }

    /// <summary>
    /// Range for Xml serialization
    /// </summary>
    [XmlAttribute("Range")]
    public virtual string XmlSerializationRange
    {
      get
      {
        if (null == this.m_range) {
          log.WarnFormat ("XmlSerializationRange.get: " +
                          "range null");
          return "";
        }
        else {
          return this.m_range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss"));
        }
      }
      set
      {
        m_range = new UtcDateTimeRange (value);
      }
    }
    
    /// <summary>
    /// Association option
    /// </summary>
    public virtual AssociationOption? Option {
      get { return m_associationOption; }
      set { m_associationOption = value; }
    }

    /// <summary>
    /// Task to associate to a machine
    /// </summary>
    [XmlIgnore]
    public virtual ITask Task {
      get { return m_task; }
      set { m_task = value; }
    }

    /// <summary>
    /// Reference to the related Task for Xml Serialization
    /// </summary>
    [XmlElement("Task")]
    public virtual Task XmlSerializationTask {
      get { return this.Task as Task; }
      set { this.Task = value; }
    }

    /// <summary>
    /// Part of the detection analysis so that any analysis problem is logged
    /// in the detectionanalysislog table
    /// </summary>
    [XmlIgnore]
    public virtual bool PartOfDetectionAnalysis {
      get { return m_partOfDetectionAnalysis; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected TaskMachineAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected TaskMachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine)
    {
      m_range = range;
    }
    
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    internal protected TaskMachineAssociation (IMachine machine, UtcDateTimeRange  range, IModification mainModification,
                                               bool partOfDetectionAnalysis)
      : base (machine, mainModification)
    {
      m_range = range;
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      Analyze ();

      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomainByMachine/TaskAssociation/" + this.Machine.Id + "?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modifications
    /// </summary>
    public override void Apply ()
    {
      this.Analyze ();
    }
    
    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// taking into account the auto-sequence table
    /// 
    /// Note the DateTime property of the modification must be correctly
    /// set to use this
    /// </summary>
    public virtual void Analyze ()
    {
      var association = new WorkOrderMachineAssociation (this.Machine,
                                                         this.Range,
                                                         this.MainModification ?? this,
                                                         m_partOfDetectionAnalysis);
      association.DateTime = this.DateTime;
      association.Task = this.Task;
      association.Option = this.Option;
      association.Caller = this;
      association.Analyze ();
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<ITask> (ref m_task);
    }
  }
}
