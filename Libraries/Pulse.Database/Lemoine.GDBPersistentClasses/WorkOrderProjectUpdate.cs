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
  /// Persistent class of table WorkOrderProjectUpdate
  /// 
  /// This table tracks all the modifications
  /// that are made in the WorkOrderProject table:
  /// creation / delete / modification.
  /// 
  /// It is necessary to allow the Analyzer service
  /// to update correctly all the Analysis tables.
  /// </summary>
  [Serializable]
  public class WorkOrderProjectUpdate: GlobalModification, IWorkOrderProjectUpdate
  {
    #region Members
    IWorkOrder m_workOrder;
    IProject m_project;
    int m_oldQuantity;
    int m_newQuantity;
    WorkOrderProjectUpdateModificationType m_typeOfModification;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderProjectUpdate).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden. It is only used by NHibernate
    /// </summary>
    protected WorkOrderProjectUpdate() {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    /// <param name="type"></param>
    internal protected WorkOrderProjectUpdate(IWorkOrder workOrder, IProject project, WorkOrderProjectUpdateModificationType type) {
      this.m_workOrder = workOrder;
      this.m_project = project;
      this.m_typeOfModification = type;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "WorkOrderProjectUpdate"; }
    }

    /// <summary>
    /// Work order
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("WorkOrder.set: " +
                           "null value");
          throw new ArgumentNullException ("WorkOrderProjectUpdate.WorkOrder");
        }
        m_workOrder = value;
      }
    }
    
    /// <summary>
    /// Work order for XML serialization
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    
    /// <summary>
    /// Project
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IProject Project {
      get { return m_project; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("Project.set: " +
                           "null value");
          throw new ArgumentNullException ("WorkOrderProjectUpdate.Project");
        }
        m_project = value;
      }
    }
    
    /// <summary>
    /// Project for XML serialization
    /// </summary>
    [XmlElement("Project")]
    public virtual Project XmlSerializationProject {
      get { return this.Project as Project; }
      set { this.Project = value; }
    }
    
    /// <summary>
    /// Old quantity of projects associated to a work order
    /// </summary>
    public virtual int OldQuantity {
      get { return m_oldQuantity; }
      set { m_oldQuantity = value; }
    }
    
    /// <summary>
    /// New quantity of projects associated to a work order
    /// </summary>
    public virtual int NewQuantity {
      get { return m_newQuantity; }
      set { m_newQuantity = value; }
    }
    
    /// <summary>
    /// Modification type
    /// </summary>
    public virtual WorkOrderProjectUpdateModificationType TypeOfModification {
      get { return m_typeOfModification; }
      set { m_typeOfModification = value; }
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
      
      if (null != this.Project) {
        switch (this.TypeOfModification) {
          case WorkOrderProjectUpdateModificationType.NEW:
            // Associate the work order to the operation slot
            // in case the work order is unique for a component
            if (GetDataStructureOption (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent)) {
              foreach (IComponent component in this.Project.Components) {
                Debug.Assert (null != component);
                IList<IOperationSlot> impactedOperationSlots =
                  ModelDAOHelper.DAOFactory.OperationSlotDAO.FindByComponent (component);
                foreach (IOperationSlot impactedOperationSlot in impactedOperationSlots) {
                  IWorkOrder workOrder =
                    Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (component);
                  if (!object.Equals (workOrder, impactedOperationSlot.WorkOrder)) {
                    impactedOperationSlot.WorkOrder = workOrder;
                  }
                }
              }
            }
            break;
          case WorkOrderProjectUpdateModificationType.DELETE:
            foreach (IComponent component in this.Project.Components) {
              IList<IOperationSlot> impactedOperationSlots =
                (new OperationSlotDAO ()).FindByWorkOrderComponent (this.WorkOrder, component);
              foreach (IOperationSlot impactedOperationSlot in impactedOperationSlots) {
                Debug.Assert (null != impactedOperationSlot.WorkOrder);
                Debug.Assert (null != impactedOperationSlot.Component);
                if (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly)
                    || !Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (impactedOperationSlot.WorkOrder, impactedOperationSlot.Operation)) {
                  // In the case the work order is determined from the component only,
                  // or the old work order is not compatible any more with the operation
                  // reset also the work order
                  impactedOperationSlot.WorkOrder = null;
                  AddAnalysisLog (LogLevel.INFO,
                                  string.Format ("Reset the work order " +
                                                 "between {0} and {1}",
                                                 impactedOperationSlot.BeginDateTime,
                                                 impactedOperationSlot.EndDateTime));
                }
                if (null != impactedOperationSlot.Line) {
                  if (!Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithComponent (impactedOperationSlot.Line, component)) {
                    impactedOperationSlot.Line = null;
                    AddAnalysisLog (LogLevel.INFO,
                                    string.Format ("Reset the line " +
                                                   "between {0} and {1}",
                                                   impactedOperationSlot.BeginDateTime,
                                                   impactedOperationSlot.EndDateTime));
                  }
                }
              }
            }
            break;
        }
      }
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomain/WorkOrderProjectUpdate?Broadcast=true");
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
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
      NHibernateHelper.Unproxy<IProject> (ref m_project);
    }
  }
}
