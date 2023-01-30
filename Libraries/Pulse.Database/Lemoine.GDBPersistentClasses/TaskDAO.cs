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
  /// Implementation of <see cref="Lemoine.ModelDAO.ITaskDAO">ITaskDAO</see>
  /// </summary>
  public class TaskDAO
    : VersionableNHibernateDAO<Task, ITask, int>
    , ITaskDAO
  {
    ILog log = LogManager.GetLogger(typeof (TaskDAO).FullName);

    /// <summary>
    /// Find all the tasks matching a specific operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<ITask> FindAllByOperation (IOperation operation)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Task> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<ITask> ();
    }
    
    /// <summary>
    /// Find all the tasks matching a specific component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<ITask> FindAllByComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Task> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<ITask> ();
    }
    
    /// <summary>
    /// Find all the tasks matching a specific work order
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IList<ITask> FindAllByWorkOrder (IWorkOrder workOrder)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Task> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<ITask> ();
    }
    
    /// <summary>
    /// Get all the next possible tasks that match the specified machine and operation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<ITask> GetNext (IMachine machine,
                                 IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Task> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .Add (new Disjunction ()
              .Add (Restrictions.IsNull ("Machine"))
              .Add (Restrictions.Eq ("Machine", machine)))
        .Add (GetPendingTaskCriterion ())
        .AddOrder (Order.Asc ("Machine"))
        .AddOrder (Order.Asc ("Order"))
        .AddOrder (Order.Asc ("DueDateTime"))
        .List<ITask> ();
    }
    
    /// <summary>
    /// Get a criterion to restrict to pending tasks
    /// </summary>
    /// <returns></returns>
    ICriterion GetPendingTaskCriterion ()
    {
      return new Disjunction ()
        .Add (Restrictions.IsNull ("TaskStatus"))
        .Add (Restrictions.Eq ("TaskStatus", TaskStatus.New))
        .Add (Restrictions.Eq ("TaskStatus", TaskStatus.Running));
    }
    
    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old task into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldTask"></param>
    /// <param name="newTask"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public ITask Merge (ITask oldTask,
                        ITask newTask,
                        ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newTask).Id) { // newTask is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldTask).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newTask, oldTask,
                              localConflictResolution);
      }
      else { // 0 != newOperation.Id
        return InternalMerge (oldTask, newTask, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old operation into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldTask"></param>
    /// <param name="newTask">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    ITask InternalMerge (ITask oldTask,
                         ITask newTask,
                         ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newTask).Id);
      
      LockForMerge (newTask);
      if (0 != ((Lemoine.Collections.IDataWithId)oldTask).Id) {
        LockForMerge (oldTask);
      }
      
      ((Task)newTask).Merge (oldTask, conflictResolution);
      
      if (0 != ((Lemoine.Collections.IDataWithId)oldTask).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        // - Modifications
        // - Analysis
        IList<IOperationSlot> operationSlots =
          ModelDAO.ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByTask (oldTask);
        foreach (IOperationSlot operationSlot in operationSlots) {
          ((OperationSlot)operationSlot).Task = newTask;
        }
        
        // Modifications
        // There is no need to add some Modification
        // because the data is automatically updated in the analysis tables above
        
        // Flush the database before deleting the operation
        // because of some foreign key constraints
        ModelDAOHelper.DAOFactory.FlushData ();
        
        // Delete the old operation
        MakeTransient (oldTask);
      }
      
      return newTask;
    }
    
    /// <summary>
    /// Lock an operation for merge
    /// </summary>
    /// <param name="task"></param>
    internal protected void LockForMerge (ITask task)
    {
      UpgradeLock (task);
      NHibernateUtil.Initialize (task.Operation);
      NHibernateUtil.Initialize (task.Component);
      NHibernateUtil.Initialize (task.WorkOrder);
      NHibernateUtil.Initialize (task.Machine);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ITask Reload (ITask entity)
    {
      ITask result = base.Reload (entity);
      NHibernateUtil.Initialize (result.Operation);
      NHibernateUtil.Initialize (result.Component);
      NHibernateUtil.Initialize (result.WorkOrder);
      NHibernateUtil.Initialize (result.Machine);
      return result;
    }
  }
}
