// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProjectComponentUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a Component and its Project.
  /// 
  /// It includes mainly the following two column to track the modifications:
  /// <item>Old Project ID: null in case a new project/component relation is set</item>
  /// <item>New Project ID: null in case the project/component relation is deleted</item>
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  [Serializable]
  public class ProjectComponentUpdate: GlobalModification, IProjectComponentUpdate
  {
    #region Members
    IComponent m_component;
    IProject m_oldProject;
    IProject m_newProject;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProjectComponentUpdate).FullName);
    
    #region Constructors
    /// <summary>
    ///  Default Constructor is forbidden. It is only used by NHibernate
    /// </summary>
    protected ProjectComponentUpdate()
    {
    }

    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="component"></param>
    /// <param name="oldProject"></param>
    /// <param name="newProject"></param>
    internal protected ProjectComponentUpdate(IComponent component, IProject oldProject, IProject newProject)
    {
      this.m_component = component;
      this.m_oldProject = oldProject;
      this.m_newProject = newProject;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ProjectComponentUpdate"; }
    }

    /// <summary>
    /// Associated component
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("Component.set: " +
                           "null value");
          throw new ArgumentNullException ("ProjectComponentUpdate.Component");
        }
        m_component = value;
      }
    }
    
    /// <summary>
    /// Associated component for XML serialization
    /// </summary>
    [XmlElement("Component")]
    public virtual Component XmlSerializationComponent {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }
    
    /// <summary>
    /// Old associated project
    /// 
    /// null in case a new project/component relation is set
    /// </summary>
    [XmlIgnore]
    public virtual IProject OldProject {
      get { return m_oldProject; }
      set { m_oldProject = value; }
    }
    
    /// <summary>
    /// Old associated project for XML serialization
    /// </summary>
    [XmlElement("OldProject")]
    public virtual Project XmlSerializationOldProject {
      get { return this.OldProject as Project; }
      set { this.OldProject = value; }
    }

    /// <summary>
    /// New associated project
    /// 
    /// null in case the project/component relation is deleted
    /// </summary>
    [XmlIgnore]
    public virtual IProject NewProject {
      get { return m_newProject; }
      set { m_newProject = value; }
    }

    /// <summary>
    /// New associated project for XML serialization
    /// </summary>
    [XmlElement("NewProject")]
    public virtual Project XmlSerializationNewProject {
      get { return this.NewProject as Project; }
      set { this.NewProject = value; }
    }
    #endregion // Getters / Setters

    #region Methods
    bool GetDataStructureOption (DataStructureConfigKey key)
    {
      return Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (key));
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.Component);
      
      // Get the impacted components in slots
      if (null != this.Component) {
        IList<IOperationSlot> impactedOperationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO.FindByComponent (this.Component);
        foreach (IOperationSlot impactedOperationSlot in impactedOperationSlots) {
          // Update the work order from the operation slot and work order slot in case
          // - the work order is only determined by the component
          // - the work order is not compatible any more with the component
          if (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly)
              || (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (impactedOperationSlot.WorkOrder,
                                                                      this.Component))) {
            IWorkOrder workOrder = null;
            if ( (null != this.Component) // workOrder is already null by default
                && GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly)) {
              workOrder =
                Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (this.Component);
            }
            if (!object.Equals (impactedOperationSlot.WorkOrder, workOrder)) {
              AddAnalysisLog (LogLevel.INFO,
                              string.Format ("Update or reset also the work order " +
                                             "from {0} to {1}",
                                             impactedOperationSlot.WorkOrder,
                                             workOrder));
              impactedOperationSlot.WorkOrder = workOrder;
            }
          }
          if (!Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithComponent (impactedOperationSlot.Line,
                                                             this.Component)) {
            Debug.Assert (null != impactedOperationSlot.Line);
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the line between {0} and {1}",
                                          impactedOperationSlot.BeginDateTime,
                                         impactedOperationSlot.EndDateTime));
            impactedOperationSlot.Line = null;
          }
        }
      }
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomain/ProjectComponentUpdate?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }    
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
      NHibernateHelper.Unproxy<IProject> (ref m_oldProject);
      NHibernateHelper.Unproxy<IProject> (ref m_newProject);
    }
  }
}
