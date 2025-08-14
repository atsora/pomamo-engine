// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IManufacturingOrderDAO">ITaskDAO</see>
  /// </summary>
  public class ManufacturingOrderDAO
    : VersionableNHibernateDAO<ManufacturingOrder, IManufacturingOrder, int>
    , IManufacturingOrderDAO
  {
    ILog log = LogManager.GetLogger(typeof (ManufacturingOrderDAO).FullName);

    /// <summary>
    /// Find all the manufacturing orders matching a specific operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<IManufacturingOrder> FindAllByOperation (IOperation operation)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ManufacturingOrder> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<IManufacturingOrder> ();
    }
    
    /// <summary>
    /// Find all the manufacturing orders matching a specific component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IManufacturingOrder> FindAllByComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ManufacturingOrder> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IManufacturingOrder> ();
    }
    
    /// <summary>
    /// Find all the manufacturing orders matching a specific work order
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IList<IManufacturingOrder> FindAllByWorkOrder (IWorkOrder workOrder)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ManufacturingOrder> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<IManufacturingOrder> ();
    }
    
    /// <summary>
    /// Get all the next possible manufacturing orders that match the specified machine and operation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<IManufacturingOrder> GetNext (IMachine machine,
                                 IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ManufacturingOrder> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .Add (new Disjunction ()
              .Add (Restrictions.IsNull ("Machine"))
              .Add (Restrictions.Eq ("Machine", machine)))
        .Add (GetPendingManufacturingOrderCriterion ())
        .AddOrder (Order.Asc ("Machine"))
        .AddOrder (Order.Asc ("Order"))
        .AddOrder (Order.Asc ("DueDateTime"))
        .List<IManufacturingOrder> ();
    }
    
    /// <summary>
    /// Get a criterion to restrict to pending manufacturing orders
    /// </summary>
    /// <returns></returns>
    ICriterion GetPendingManufacturingOrderCriterion ()
    {
      return new Disjunction ()
        .Add (Restrictions.IsNull ("ManufacturingOrderStatus"))
        .Add (Restrictions.Eq ("ManufacturingOrderStatus", ManufacturingOrderStatus.New))
        .Add (Restrictions.Eq ("ManufacturingOrderStatus", ManufacturingOrderStatus.Running));
    }
    
    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old manufacturing order into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldManufacturingOrder"></param>
    /// <param name="newManufacturingOrder"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IManufacturingOrder Merge (IManufacturingOrder oldManufacturingOrder,
                        IManufacturingOrder newManufacturingOrder,
                        ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newManufacturingOrder).Id) { // newTask is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldManufacturingOrder).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newManufacturingOrder, oldManufacturingOrder,
                              localConflictResolution);
      }
      else { // 0 != newOperation.Id
        return InternalMerge (oldManufacturingOrder, newManufacturingOrder, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old operation into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldManufacturingOrder"></param>
    /// <param name="newManufacturingOrder">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IManufacturingOrder InternalMerge (IManufacturingOrder oldManufacturingOrder,
                         IManufacturingOrder newManufacturingOrder,
                         ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newManufacturingOrder).Id);
      
      LockForMerge (newManufacturingOrder);
      if (0 != ((Lemoine.Collections.IDataWithId)oldManufacturingOrder).Id) {
        LockForMerge (oldManufacturingOrder);
      }
      
      ((ManufacturingOrder)newManufacturingOrder).Merge (oldManufacturingOrder, conflictResolution);
      
      if (0 != ((Lemoine.Collections.IDataWithId)oldManufacturingOrder).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        // - Modifications
        // - Analysis
        IList<IOperationSlot> operationSlots =
          ModelDAO.ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByManufacturingOrder (oldManufacturingOrder);
        foreach (IOperationSlot operationSlot in operationSlots) {
          ((OperationSlot)operationSlot).ManufacturingOrder = newManufacturingOrder;
        }
        
        // Modifications
        // There is no need to add some Modification
        // because the data is automatically updated in the analysis tables above
        
        // Flush the database before deleting the operation
        // because of some foreign key constraints
        ModelDAOHelper.DAOFactory.FlushData ();
        
        // Delete the old operation
        MakeTransient (oldManufacturingOrder);
      }
      
      return newManufacturingOrder;
    }
    
    /// <summary>
    /// Lock an operation for merge
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    internal protected void LockForMerge (IManufacturingOrder manufacturingOrder)
    {
      UpgradeLock (manufacturingOrder);
      NHibernateUtil.Initialize (manufacturingOrder.Operation);
      NHibernateUtil.Initialize (manufacturingOrder.Component);
      NHibernateUtil.Initialize (manufacturingOrder.WorkOrder);
      NHibernateUtil.Initialize (manufacturingOrder.Machine);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IManufacturingOrder Reload (IManufacturingOrder entity)
    {
      IManufacturingOrder result = base.Reload (entity);
      NHibernateUtil.Initialize (result.Operation);
      NHibernateUtil.Initialize (result.Component);
      NHibernateUtil.Initialize (result.WorkOrder);
      NHibernateUtil.Initialize (result.Machine);
      return result;
    }
  }
}
