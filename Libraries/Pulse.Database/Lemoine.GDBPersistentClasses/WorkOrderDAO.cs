// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IWorkOrderDAO">IWorkOrderDAO</see>
  /// </summary>
  public class WorkOrderDAO
    : VersionableNHibernateDAO<WorkOrder, IWorkOrder, int>
    , IWorkOrderDAO
  {
    
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderDAO).FullName);
    
    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old WorkOrder into a new one
    /// 
    /// This returns the merged WorkOrder
    /// </summary>
    /// <param name="oldWorkOrder"></param>
    /// <param name="newWorkOrder"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IWorkOrder Merge (IWorkOrder oldWorkOrder,
                             IWorkOrder newWorkOrder,
                             ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newWorkOrder).Id) { // newWorkOrder is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldWorkOrder).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newWorkOrder, oldWorkOrder,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldWorkOrder, newWorkOrder, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old WorkOrder into a new one
    /// 
    /// This returns the merged WorkOrder
    /// </summary>
    /// <param name="oldWorkOrder"></param>
    /// <param name="newWorkOrder">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IWorkOrder InternalMerge (IWorkOrder oldWorkOrder,
                              IWorkOrder newWorkOrder,
                              ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newWorkOrder).Id);
      
      LockForMerge (newWorkOrder);
      if (0 != ((Lemoine.Collections.IDataWithId)oldWorkOrder).Id) {
        LockForMerge (oldWorkOrder);
      }
      
      ((WorkOrder)newWorkOrder).Merge (oldWorkOrder, conflictResolution);
      
      if (0 != ((Lemoine.Collections.IDataWithId)oldWorkOrder).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        ISession session = NHibernateHelper.GetCurrentSession (); // Temporary
        // - Data
        IList<IManufacturingOrder> manufacturingOrders = ModelDAOHelper.DAOFactory.ManufacturingOrderDAO
          .FindAllByWorkOrder (oldWorkOrder);
        foreach (IManufacturingOrder manufacturingOrder in manufacturingOrders) {
          manufacturingOrder.WorkOrder = newWorkOrder;
        }
        {
          IList<IWorkOrderLine> otherWorkOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
            .FindAllByWorkOrder (oldWorkOrder);
          // Make it simple for the moment. Reject any merge if a WorkOrderLine was set
          // See later if something better can be imagined
          if (0 < otherWorkOrderLines.Count) {
            log.ErrorFormat ("Merge: " +
                             "could not merge the work order {0} into {1} " +
                             "because a work order line exists for {0}",
                             oldWorkOrder, newWorkOrder);
            throw new ConflictException (oldWorkOrder, newWorkOrder, "WorkOrderLine");
          }
        }
        // - Modification
        IList<IWorkOrderMachineAssociation> workOrderMachineAssociations =
          session.CreateCriteria<WorkOrderMachineAssociation> ()
          .Add (Expression.Eq ("WorkOrder", oldWorkOrder))
          .List<IWorkOrderMachineAssociation> ();
        foreach (IWorkOrderMachineAssociation workOrderMachineAssociation
                 in workOrderMachineAssociations) {
          workOrderMachineAssociation.WorkOrder = newWorkOrder;
        }
        IList<IWorkOrderProjectUpdate> workOrderProjectUpdates =
          session.CreateCriteria<WorkOrderProjectUpdate> ()
          .Add (Expression.Eq ("WorkOrder", oldWorkOrder))
          .List<IWorkOrderProjectUpdate> ();
        foreach (IWorkOrderProjectUpdate workOrderProjectUpdate
                 in workOrderProjectUpdates) {
          workOrderProjectUpdate.WorkOrder = newWorkOrder;
        }
        // - Analysis
        IList<IOperationSlot> operationSlots =
          ModelDAO.ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByWorkOrder (oldWorkOrder);
        foreach (IOperationSlot operationSlot in operationSlots) {
          ((OperationSlot)operationSlot).WorkOrder = newWorkOrder;
        }
        
        { // - Update IntermediateWorkPieceTarget
          IList<IIntermediateWorkPieceTarget> iwpTargets =
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
            .FindByWorkOrder (oldWorkOrder);
          foreach (IIntermediateWorkPieceTarget iwpTarget in iwpTargets) {
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .MakeTransient (iwpTarget);
            IIntermediateWorkPieceTarget existing =
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .FindByKey (iwpTarget.IntermediateWorkPiece, iwpTarget.Component, newWorkOrder, iwpTarget.Line, iwpTarget.Day, iwpTarget.Shift);
            if (null != existing) {
              existing.Number += iwpTarget.Number;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                .MakePersistent (existing);
            }
            else {
              IIntermediateWorkPieceTarget created =
                ModelDAOHelper.ModelFactory.CreateIntermediateWorkPieceTarget (iwpTarget.IntermediateWorkPiece,
                                                                               iwpTarget.Component,
                                                                               newWorkOrder,
                                                                               iwpTarget.Line,
                                                                               iwpTarget.Day,
                                                                               iwpTarget.Shift);
              created.Number = iwpTarget.Number;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                .MakePersistent (created);
            }
          }
        }
        
        var extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IWorkOrderExtension> ());
        foreach (var extension in extensions) {
          extension.Merge (oldWorkOrder, newWorkOrder);
        }

        // Modifications
        // There is no need to add some Modification
        // WorkOrderProjectUpdate rows,
        // because the data is automatically updated in the analysis tables above

        // Delete the old work order
        ModelDAOHelper.DAOFactory.WorkOrderDAO.MakeTransient (oldWorkOrder);
      }
      
      return newWorkOrder;
    }
    
    /// <summary>
    /// Lock a WorkOrder for merge
    /// </summary>
    void LockForMerge (IWorkOrder workOrder)
    {
      UpgradeLock (workOrder);
      if (workOrder.Projects != null) {
        if (!NHibernateUtil.IsInitialized(workOrder.Projects)) {
          NHibernateUtil.Initialize(workOrder.Projects);
        }
      }
      NHibernateUtil.Initialize (workOrder.Status);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// Try to get the WorkOrder entity matching an external code
    /// <param name="workOrderExternalCode"></param>
    /// </summary>
    public IWorkOrder FindByExternalCode(string workOrderExternalCode)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrder> ()
        .Add (Restrictions.Eq ("ExternalCode", workOrderExternalCode))
        .UniqueResult<WorkOrder> ();
    }
    
    /// <summary>
    /// Try to get the WorkOrder entity matching a code
    /// <param name="workOrderCode"></param>
    /// </summary>
    public IWorkOrder FindByCode(string workOrderCode)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrder> ()
        .Add (Restrictions.Eq ("Code", workOrderCode))
        .UniqueResult<WorkOrder> ();
    }
    
    /// <summary>
    /// Try to get the WorkOrder entity matching a name
    /// <param name="workOrderName"></param>
    /// </summary>
    public IWorkOrder FindByName(string workOrderName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrder> ()
        .Add (Restrictions.Eq ("Name", workOrderName))
        .UniqueResult<WorkOrder> ();
    }
    
    /// <summary>
    /// Tests if exists others WorkOrder have same name like WorkOrder with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameName(String name, int id)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<WorkOrder>()
        .Add(Restrictions.Eq("Name",name))
        .Add(Restrictions.IsNotNull("Name"))
        .Add(Restrictions.Not(Restrictions.Eq("Id",id)))
        .SetProjection(Projections.RowCount())
        .UniqueResult();
      return (count > 0)?true:false;
    }
    
    /// <summary>
    /// Tests if exists others WorkOrder have same code like WorkOrder with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameCode(String code, int id)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<WorkOrder>()
        .Add(Restrictions.Eq("Code",code))
        .Add(Restrictions.IsNotNull("Code"))
        .Add(Restrictions.Not(Restrictions.Eq("Id",id)))
        .SetProjection(Projections.RowCount())
        .UniqueResult();
      return (count > 0)?true:false;
    }
    
    
    /// <summary>
    /// Tests if exists WorkOrder have same name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameName(String name)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<WorkOrder>()
        .Add(Restrictions.Eq("Name",name))
        .Add(Restrictions.IsNotNull("Name"))
        .SetProjection(Projections.RowCount())
        .UniqueResult();
      return (count > 0)?true:false;
    }
    
    /// <summary>
    /// Tests if exists WorkOrder have same code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameCode(String code)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<WorkOrder>()
        .Add(Restrictions.Eq("Code",code))
        .Add(Restrictions.IsNotNull("Code"))
        .SetProjection(Projections.RowCount())
        .UniqueResult();
      return (count > 0)?true:false;
    }

    /// <summary>
    /// return all WorkOrder with loading associated Project
    /// </summary>
    /// <returns></returns>
    public IList<IWorkOrder> FindAllEager()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrder>()
        .Fetch (SelectMode.Fetch, "Projects")
        .List<IWorkOrder>();
    }
    
    /// <summary>
    /// Initialize the associated projects
    /// </summary>
    /// <param name="workOrder"></param>
    public void InitializeProjects (IWorkOrder workOrder)
    {
      NHibernateUtil.Initialize(workOrder.Projects);
    }
  }
}
