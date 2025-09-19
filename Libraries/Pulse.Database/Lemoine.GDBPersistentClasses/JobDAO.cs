// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Threading.Tasks;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of JobDAO.
  /// </summary>
  public class JobDAO : IJobDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (JobDAO).FullName);

    public virtual bool IsAttachedToSession (Lemoine.Model.IJob persistent)
    {
      var session = NHibernateHelper.GetCurrentSession ();
      return session.Contains (persistent.Project) && session.Contains (persistent.WorkOrder);
    }

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old Job into a new one
    /// 
    /// This returns the merged Job
    /// </summary>
    /// <param name="oldJob"></param>
    /// <param name="newJob"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IJob Merge (IJob oldJob,
                       IJob newJob,
                       ConflictResolution conflictResolution)
    {
      if (0 == newJob.ProjectId) { // newJob is not persistent, inverse the arguments
        Debug.Assert (0 != oldJob.ProjectId);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newJob, oldJob,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldJob, newJob, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old Job into a new one
    /// 
    /// This returns the merged Job
    /// </summary>
    /// <param name="oldJob"></param>
    /// <param name="newJob">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IJob InternalMerge (IJob oldJob,
                        IJob newJob,
                        ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != newJob.ProjectId);
      
      LockForMerge (newJob);
      if (0 != oldJob.ProjectId) {
        LockForMerge (oldJob);
      }
      
      (((Job)newJob).WorkOrder as WorkOrder)
        .Merge (((Job)oldJob).WorkOrder as WorkOrder,
                conflictResolution);
      (((Job)newJob).Project as Project)
        .Merge (((Job)oldJob).Project as Project,
                conflictResolution);
      
      return newJob;
    }
    
    /// <summary>
    /// Lock a Job for merge
    /// </summary>
    void LockForMerge (IJob job)
    {
      ((ProjectDAO)ModelDAOHelper.DAOFactory.ProjectDAO).LockForMerge (job.Project);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public IJob FindById (int projectId)
    {
      return FindByProjectId (projectId);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IJob> FindByIdAsync (int projectId)
    {
      return await FindByProjectIdAsync (projectId);
    }

    /// <summary>
    /// FindByWorkOrderId implementation
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <returns></returns>
    public IJob FindByWorkOrderId (int workOrderId)
    {
      IWorkOrder workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
        .FindById (workOrderId);
      if (null == workOrder) {
        return null;
      }
      else {
        return workOrder.Job;
      }
    }
    
    /// <summary>
    /// FindByProjectId implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public IJob FindByProjectId (int projectId)
    {
      IProject project = ModelDAOHelper.DAOFactory.ProjectDAO
        .FindById (projectId);
      if (null == project) {
        return null;
      }
      else {
        return project.Job;
      }
    }

    /// <summary>
    /// FindByProjectId implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IJob> FindByProjectIdAsync (int projectId)
    {
      IProject project = await ModelDAOHelper.DAOFactory.ProjectDAO
        .FindByIdAsync (projectId);
      if (null == project) {
        return null;
      }
      else {
        return project.Job;
      }
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public IList<IJob> FindAll ()
    {
      IList<IJob> jobs = new List<IJob> ();
      IList<IProject> projects = ModelDAOHelper.DAOFactory.ProjectDAO.FindAll ();
      foreach (IProject project in projects) {
        jobs.Add (project.Job);
      }
      return jobs;
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IList<IJob>> FindAllAsync ()
    {
      IList<IJob> jobs = new List<IJob> ();
      IList<IProject> projects = await ModelDAOHelper.DAOFactory.ProjectDAO.FindAllAsync ();
      foreach (IProject project in projects) {
        jobs.Add (project.Job);
      }
      return jobs;
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="job">not null</param>
    public void MakeTransient (IJob job)
    {
      Debug.Assert (null != job);
      
      IProject project = ((Job)job).Project;
      IWorkOrder workOrder = ((Job)job).WorkOrder;
      project.RemoveWorkOrder (workOrder);
      ModelDAOHelper.DAOFactory.WorkOrderDAO.MakeTransient (workOrder);
      ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransient (project);
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="job">not null</param>
    public async System.Threading.Tasks.Task MakeTransientAsync (IJob job)
    {
      Debug.Assert (null != job);

      IProject project = ((Job)job).Project;
      IWorkOrder workOrder = ((Job)job).WorkOrder;
      project.RemoveWorkOrder (workOrder);
      await ModelDAOHelper.DAOFactory.WorkOrderDAO.MakeTransientAsync (workOrder);
      await ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransientAsync (project);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="job">not null</param>
    public void Lock (IJob job)
    {
      Debug.Assert (null != job);
      
      IProject project = ((Job)job).Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
      NHibernateUtil.Initialize(project.WorkOrders);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="job">not null</param>
    public async System.Threading.Tasks.Task LockAsync (IJob job)
    {
      Debug.Assert (null != job);

      IProject project = ((Job)job).Project;
      await ModelDAOHelper.DAOFactory.ProjectDAO.LockAsync (project);
      NHibernateUtil.Initialize (project.WorkOrders);
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public IJob FindByIdAndLock (int projectId)
    {
      IProject project = ModelDAOHelper.DAOFactory.ProjectDAO
        .FindByIdAndLock (projectId);
      if (null == project) {
        return null;
      }
      else {
        Debug.Assert (null != project.WorkOrders);
        NHibernateUtil.Initialize (project.WorkOrders);
        return project.Job;
      }
    }
    
    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="job">not null</param>
    /// <returns></returns>
    public IJob MakePersistent (IJob job)
    {
      Debug.Assert (null != job);
      
      IProject project = ((Job)job).Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
      IWorkOrder workOrder = ((Job)job).WorkOrder;
      ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workOrder);
      return job;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="job">not null</param>
    /// <returns></returns>
    public async Task<IJob> MakePersistentAsync (IJob job)
    {
      Debug.Assert (null != job);

      IProject project = ((Job)job).Project;
      await ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistentAsync (project);
      IWorkOrder workOrder = ((Job)job).WorkOrder;
      await ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistentAsync (workOrder);
      return job;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="job">not null</param>
    public void UpgradeLock (IJob job)
    {
      Debug.Assert (null != job);
      
      IProject project = ((Job)job).Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.UpgradeLock (project);
      NHibernateUtil.Initialize(project.WorkOrders);
    }
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails)
    /// </summary>
    /// <param name="job">not null</param>
    /// <returns></returns>
    public IJob Reload (IJob job)
    {
      Debug.Assert (null != job);
      
      IProject project = ((Job)job).Project;
      project = ModelDAOHelper.DAOFactory.ProjectDAO.Reload (project);
      NHibernateUtil.Initialize(project.WorkOrders);
      return project.Job;
    }
  }
}
