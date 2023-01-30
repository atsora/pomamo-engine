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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ProjectDAO.
  /// </summary>
  public class ProjectDAO
    : VersionableNHibernateDAO<Project, IProject, int>
    , IProjectDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProjectDAO).FullName);

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old Project into a new one
    /// 
    /// This returns the merged Project
    /// </summary>
    /// <param name="oldProject"></param>
    /// <param name="newProject"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IProject Merge (IProject oldProject,
                           IProject newProject,
                           ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newProject).Id) { // newProject is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldProject).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newProject, oldProject,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldProject, newProject, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old Project into a new one
    /// 
    /// This returns the merged Project
    /// </summary>
    /// <param name="oldProject"></param>
    /// <param name="newProject">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IProject InternalMerge (IProject oldProject,
                            IProject newProject,
                            ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newProject).Id);

      LockForMerge (newProject);
      if (0 != ((Lemoine.Collections.IDataWithId)oldProject).Id) {
        LockForMerge (oldProject);
      }

      ((Project)newProject).Merge (oldProject, conflictResolution);

      if (0 != ((Lemoine.Collections.IDataWithId)oldProject).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        ISession session = NHibernateHelper.GetCurrentSession (); // Temporary
        // - Data
        // - Modification
        IList<IWorkOrderProjectUpdate> workOrderProjectUpdates =
          session.CreateCriteria<WorkOrderProjectUpdate> ()
          .Add (Restrictions.Eq ("Project", oldProject))
          .List<IWorkOrderProjectUpdate> ();
        foreach (IWorkOrderProjectUpdate workOrderProjectUpdate
                 in workOrderProjectUpdates) {
          workOrderProjectUpdate.Project = newProject;
        }
        IList<IProjectComponentUpdate> oldProjectComponentUpdates =
          session.CreateCriteria<ProjectComponentUpdate> ()
          .Add (Restrictions.Eq ("OldProject", oldProject))
          .List<IProjectComponentUpdate> ();
        foreach (IProjectComponentUpdate projectComponentUpdate
                 in oldProjectComponentUpdates) {
          projectComponentUpdate.OldProject = newProject;
        }
        IList<IProjectComponentUpdate> newProjectComponentUpdates =
          session.CreateCriteria<ProjectComponentUpdate> ()
          .Add (Restrictions.Eq ("NewProject", oldProject))
          .List<IProjectComponentUpdate> ();
        foreach (IProjectComponentUpdate projectComponentUpdate
                 in newProjectComponentUpdates) {
          projectComponentUpdate.NewProject = newProject;
        }
        // - Analysis

        // Modifications
        // There is no need to add some Modification
        // WorkOrderProjectUpdate / ProjectComponentUpdate /
        // StampUpdate rows,
        // because the data is automatically updated in the analysis tables above

        // Delete the old operation
        ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransient (oldProject);
      }

      return newProject;
    }

    /// <summary>
    /// Lock a project for merge
    /// </summary>
    protected internal void LockForMerge (IProject project)
    {
      UpgradeLock (project);
      NHibernateUtil.Initialize (project.Components);
      NHibernateUtil.Initialize (project.WorkOrders);
    }
    #endregion // IMergeDAO implementation

    /// <summary>
    /// <see cref="IProjectDAO"/>
    /// </summary>
    public IProject FindByName (string projectName)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Name", projectName))
        .SetMaxResults (1)
        .UniqueResult<IProject> ();
    }

    /// <summary>
    /// <see cref="IProjectDAO"/>
    /// </summary>
    public IProject FindByCode (string projectCode)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Code", projectCode))
        .UniqueResult<IProject> ();
    }

    /// <summary>
    /// Get all Project ordered by name
    /// </summary>
    /// <returns></returns>
    public IList<IProject> FindAllOrderByName ()
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .AddOrder (Order.Asc ("Name"))
        .AddOrder (Order.Asc ("Code"))
        .List<IProject> ();
    }

    /// <summary>
    /// Tests if exists others Project have same name like Project with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameName (String name, int id)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.IsNotNull ("Name"))
        .Add (Restrictions.Not (Restrictions.Eq ("Id", id)))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }


    /// <summary>
    /// Tests if exists others Project have same code like Project with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameCode (String code, int id)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Code", code))
        .Add (Restrictions.IsNotNull ("Code"))
        .Add (Restrictions.Not (Restrictions.Eq ("Id", id)))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }


    /// <summary>
    /// Tests if exists Project have same name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameName (String name)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.IsNotNull ("Name"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return count > 0;
    }

    /// <summary>
    /// Tests if exists Project have same code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameCode (String code)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.Eq ("Code", code))
        .Add (Restrictions.IsNotNull ("Code"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return count > 0;
    }

    /// <summary>
    /// Get orphans Project, means project without link to WorkOrder
    /// </summary>
    /// <returns></returns>
    public IList<IProject> GetOrphans ()
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .Add (Restrictions.IsEmpty ("WorkOrders"))
        .List<IProject> ();
    }

    /// <summary>
    /// Initialize the associated components
    /// </summary>
    /// <param name="project"></param>
    public void InitializeComponents (IProject project)
    {
      NHibernateUtil.Initialize (project.Components);
    }
  }
}
