// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using NHibernate;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table WorkOrderMachineStamp
  /// </summary>
  [Serializable]
  public class WorkOrderMachineStamp
    : MachineStamp, IWorkOrderMachineStamp
  {
    #region Members
    IWorkOrder m_workOrder;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "WorkOrderMachineStamp"; }
    }

    /// <summary>
    /// Work order to associate to a machine
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
          throw new ArgumentNullException ("WorkOrderMachineStamp.WorkOrder");
        }
        m_workOrder = value;
      }
    }

    /// <summary>
    /// Reference to the related work order for Xml Serialization
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected WorkOrderMachineStamp ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected WorkOrderMachineStamp (IMachine machine, IWorkOrder workOrder, DateTime dateTime)
      : base (machine, dateTime)
    {
      WorkOrder = workOrder;
    }
    #endregion // Constructors

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      // handle obsolescence
      if (DateTime.UtcNow.Subtract(AnalysisConfigHelper.ObsoleteTime) > this.DateTime) {
        MarkAsObsolete ();
        return;
      }
      
      // try to find matching operation slots and cycles
      IOperationSlot operationSlotAtScan =
        ModelDAOHelper.DAOFactory
        .OperationSlotDAO.FindAt(this.Machine, this.DateTime);
      
      if (operationSlotAtScan == null)
      {
        // no operation slot found: analysis will be performed again at a later time
        // when an operation slot becomes available
        MarkAsPending (null);
        return;
      }
      
      // perform work order / operation slot stuff
      IWorkOrder slotWorkOrder = operationSlotAtScan.WorkOrder;

      if (slotWorkOrder == null)
      {
        // no work order on operation slot
        // perform work order / machine association with begin date = operation slot begin date
        IWorkOrderMachineAssociation workOrderAssociation =
          ModelDAOHelper.ModelFactory
          .CreateWorkOrderMachineAssociation(this.Machine,
                                             this.WorkOrder,
                                             new UtcDateTimeRange (operationSlotAtScan.BeginDateTime));
        ModelDAOHelper.DAOFactory
          .WorkOrderMachineAssociationDAO.MakePersistent(workOrderAssociation);
      }
      else {        
        if (slotWorkOrder != this.WorkOrder) {
          // a new operation slot will be created
          // use begin of last unassociated full operation cycle
          // as a work order / machine association begin date
          // if it exists; otherwise use scan date
          
          IOperationCycle lastFullUnassociatedCycle =
            ModelDAOHelper.DAOFactory
            .OperationCycleDAO.FindLastFullNotAssociated(operationSlotAtScan, this.DateTime);
          
          DateTime workOrderMachineAssociationBegin =
            ((lastFullUnassociatedCycle != null) && (lastFullUnassociatedCycle.Begin.HasValue)) ?
            lastFullUnassociatedCycle.Begin.Value : this.DateTime;
          
          IWorkOrderMachineAssociation workOrderAssociation =
            ModelDAOHelper.ModelFactory
            .CreateWorkOrderMachineAssociation(this.Machine,
                                               this.WorkOrder,
                                               new UtcDateTimeRange (workOrderMachineAssociationBegin));
          
          ModelDAOHelper.DAOFactory
            .WorkOrderMachineAssociationDAO.MakePersistent(workOrderAssociation);
          
        }  // if same work order, nothing to do
      }
      
      // analysis done
      MarkAsCompleted ("");
      return;
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient ReasonMachineAssociation to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
    }
  }
}
