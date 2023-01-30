// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ComponentDAO.
  /// </summary>
  public class ComponentDAO
    : VersionableNHibernateDAO<Component, IComponent, int>
    , IComponentDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComponentDAO).FullName);

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old component into a new one
    /// 
    /// This returns the merged Component
    /// </summary>
    /// <param name="oldComponent"></param>
    /// <param name="newComponent"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IComponent Merge (IComponent oldComponent,
                             IComponent newComponent,
                             ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newComponent).Id) { // newComponent is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldComponent).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newComponent, oldComponent,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldComponent, newComponent, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old Component into a new one
    /// 
    /// This returns the merged Component
    /// </summary>
    /// <param name="oldComponent"></param>
    /// <param name="newComponent">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IComponent InternalMerge (IComponent oldComponent,
                              IComponent newComponent,
                              ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newComponent).Id);

      LockForMerge (newComponent);
      if (0 != ((Lemoine.Collections.IDataWithId)oldComponent).Id) {
        LockForMerge (oldComponent);
      }

      // To be merged, oldComponent and newComponent must be part of the same project
      if (!object.Equals (newComponent.Project, oldComponent.Project)) {
        log.ErrorFormat ("InternalMerge: " +
                         "trying to merge two new Components that are part of two different projects " +
                         "old={0} new={1}",
                         oldComponent, newComponent);
        throw new InvalidOperationException ("The two components must be part of the same project to be merged");
      }

      Debug.Assert (newComponent is Component);
      (newComponent as Component).Merge (oldComponent as Component, conflictResolution);

      if (0 != ((Lemoine.Collections.IDataWithId)oldComponent).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        // - Data
        IList<ICadModel> cadModels = ModelDAOHelper.DAOFactory.CadModelDAO
          .FindAllWithComponent (oldComponent);
        foreach (ICadModel cadModel in cadModels) {
          cadModel.Component = newComponent;
        }
        IList<IStamp> stamps = ModelDAOHelper.DAOFactory.StampDAO
          .FindAllWithComponent (oldComponent);
        foreach (IStamp stamp in stamps) {
          stamp.Component = newComponent;
        }
        IList<ITask> tasks = ModelDAOHelper.DAOFactory.TaskDAO
          .FindAllByComponent (oldComponent);
        foreach (ITask task in tasks) {
          task.Component = newComponent;
        }
        { // Line
          IList<ILine> otherLines = ModelDAOHelper.DAOFactory.LineDAO
            .FindAllByComponent (oldComponent);
          foreach (ILine otherLine in otherLines) {
            if (!otherLine.Components.Contains (newComponent)) {
              otherLine.AddComponent (newComponent);
              IList<IWorkOrderLine> workOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
                .FindAllByLine (otherLine);
              foreach (IWorkOrderLine workOrderLine in workOrderLines) {
                IWorkOrder workOrder = workOrderLine.WorkOrder;
                if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (workOrder, newComponent)) {
                  // Associate the work order with the component
                  log.DebugFormat ("merge: " +
                                   "associate work order {0} with {1}",
                                   workOrder, this);
                  newComponent.Project.AddWorkOrder (workOrder);
                  IWorkOrderProjectUpdate update = ModelDAOHelper.ModelFactory
                    .CreateWorkOrderProjectUpdate (workOrder, newComponent.Project, WorkOrderProjectUpdateModificationType.NEW);
                  ModelDAOHelper.DAOFactory.WorkOrderProjectUpdateDAO.MakePersistent (update);
                }
              }
            }
            otherLine.RemoveComponent (oldComponent);
          }
        }
        // - Modifications
        IList<IComponentMachineAssociation> componentMachineAssociations =
          ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO
          .FindAllWithComponent (oldComponent);
        foreach (IComponentMachineAssociation componentMachineAssociation
                 in componentMachineAssociations) {
          componentMachineAssociation.Component = newComponent;
        }
        IList<IComponentIntermediateWorkPieceUpdate> componentIntermediateWorkPieceUpdates =
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO
          .FindAllWithComponent (oldComponent);
        foreach (IComponentIntermediateWorkPieceUpdate componentIntermediateWorkPieceUpdate
                 in componentIntermediateWorkPieceUpdates) {
          componentIntermediateWorkPieceUpdate.Component = newComponent;
        }
        IList<IProjectComponentUpdate> projectComponentUpdates =
          ModelDAOHelper.DAOFactory.ProjectComponentUpdateDAO
          .FindAllWithComponent (oldComponent);
        foreach (IProjectComponentUpdate projectComponentUpdate
                 in projectComponentUpdates) {
          projectComponentUpdate.Component = newComponent;
        }
        // - Analysis
        IList<IOperationSlot> operationSlots =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByComponent (oldComponent);
        foreach (IOperationSlot operationSlot in operationSlots) {
          ((OperationSlot)operationSlot).Component = newComponent;
        }

        { // - Update IntermediateWorkPieceTarget
          IList<IIntermediateWorkPieceTarget> iwpTargets =
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
            .FindByComponent (oldComponent);
          foreach (IIntermediateWorkPieceTarget iwpTarget in iwpTargets) {
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .MakeTransient (iwpTarget);
            IIntermediateWorkPieceTarget existing =
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .FindByKey (iwpTarget.IntermediateWorkPiece, newComponent, iwpTarget.WorkOrder, iwpTarget.Line, iwpTarget.Day, iwpTarget.Shift);
            if (null != existing) {
              existing.Number += iwpTarget.Number;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                .MakePersistent (existing);
            }
            else {
              IIntermediateWorkPieceTarget created =
                ModelDAOHelper.ModelFactory.CreateIntermediateWorkPieceTarget (iwpTarget.IntermediateWorkPiece,
                                                                               newComponent,
                                                                               iwpTarget.WorkOrder,
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
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IComponentExtension> ());
        foreach (var extension in extensions) {
          extension.Merge (oldComponent, newComponent);
        }

        // Modifications
        // There is no need to add some Modification
        // ProjectComponentUpdate / ComponentIntermediateWorkUpdate /
        // StampUpdate rows,
        // because the data is automatically updated in the analysis tables above

        // Delete the old component
        MakeTransient (oldComponent);
      }

      return newComponent;
    }

    /// <summary>
    /// Lock a component for merge
    /// </summary>
    protected internal void LockForMerge (IComponent component)
    {
      UpgradeLock (component);
      NHibernateUtil.Initialize (component.ComponentIntermediateWorkPieces);
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
        NHibernateUtil.Initialize (componentIntermediateWorkPiece.IntermediateWorkPiece);
      }
      NHibernateUtil.Initialize (component.Type);
    }
    #endregion // IMergeDAO implementation

    #region Methods
    /// <summary>
    /// Find Component by Name and Project
    /// </summary>
    /// <param name="name"></param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    public IComponent FindByNameAndProject (string name, IProject project)
    {
      Debug.Assert (null != name);
      Debug.Assert (null != project);

      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Component> ()
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.Eq ("Project.Id", project.Id))
        .UniqueResult<IComponent> ();
    }

    /// <summary>
    /// Find Component by Code and Project
    /// </summary>
    /// <param name="name"></param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    public IComponent FindByCodeAndProject (string code, IProject project)
    {
      Debug.Assert (null != code);
      Debug.Assert (null != project);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Component> ()
        .Add (Restrictions.Eq ("Code", code))
        .Add (Restrictions.Eq ("Project.Id", project.Id))
        .UniqueResult<IComponent> ();
    }

    /// <summary>
    /// Tests if exists others Component have same name like Component with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameName (String name, int id, int projectId)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .CreateAlias ("Components", "component")
        .Add (Restrictions.Eq ("this.Id", projectId))
        .Add (Restrictions.Not (Restrictions.Eq ("component.Id", id)))
        .Add (Restrictions.Eq ("component.Name", name))
        .Add (Restrictions.IsNotNull ("component.Name"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }

    /// <summary>
    /// Tests if exists others Component have same code like Component with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Boolean IfExistsOtherWithSameCode (String code, int id, int projectId)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .CreateAlias ("Components", "component")
        .Add (Restrictions.Eq ("this.Id", projectId))
        .Add (Restrictions.Not (Restrictions.Eq ("component.Id", id)))
        .Add (Restrictions.Eq ("component.Code", code))
        .Add (Restrictions.IsNotNull ("component.Code"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }

    /// <summary>
    /// Tests if exists Component have same name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameName (String name, int projectId)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .CreateAlias ("Components", "component")
        .Add (Restrictions.Eq ("this.Id", projectId))
        .Add (Restrictions.Eq ("component.Name", name))
        .Add (Restrictions.IsNotNull ("component.Name"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }

    /// <summary>
    /// Tests if exists Component have same code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Boolean IfExistsWithSameCode (String code, int projectId)
    {
      int count = (int)NHibernateHelper.GetCurrentSession ().CreateCriteria<Project> ()
        .CreateAlias ("Components", "component")
        .Add (Restrictions.Eq ("this.Id", projectId))
        .Add (Restrictions.Eq ("component.Code", code))
        .Add (Restrictions.IsNotNull ("component.Code"))
        .SetProjection (Projections.RowCount ())
        .UniqueResult ();
      return (count > 0);
    }

    /// <summary>
    /// Remove one of the intermediate work piece of the component
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateWorkPiece"></param>
    public void RemoveIntermediateWorkPiece (IComponent component, IIntermediateWorkPiece intermediateWorkPiece)
    {
      NHibernateUtil.Initialize (component.ComponentIntermediateWorkPieces);
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
        NHibernateUtil.Initialize (componentIntermediateWorkPiece.IntermediateWorkPiece);
      }
      IList<IComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
        component.RemoveIntermediateWorkPiece (intermediateWorkPiece);
      ModelDAOHelper.DAOFactory.FlushData ();
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in componentIntermediateWorkPieces) {
        IComponentIntermediateWorkPiece ciwp =
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.Reload (componentIntermediateWorkPiece);
        ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (ciwp);
      }
    }

    /// <summary>
    /// Change the associated project
    /// </summary>
    /// <param name="component"></param>
    /// <param name="newProject"></param>
    public void ChangeProject (IComponent component, IProject newProject)
    {
      ModelDAOHelper.DAOFactory.ProjectDAO.Lock (component.Project);
      IProject oldProject = component.Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.InitializeComponents (oldProject);
      component.Project = newProject;
      ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (oldProject);
      ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (newProject);
      ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);

      IProjectComponentUpdate update = ModelDAOHelper.ModelFactory
        .CreateProjectComponentUpdate (component, oldProject, newProject);
      update.DateTime = DateTime.UtcNow;
      ModelDAOHelper.DAOFactory.ProjectComponentUpdateDAO.MakePersistent (update);
    }

    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IComponent Reload (IComponent entity)
    {
      IComponent result = base.Reload (entity);
      NHibernateUtil.Initialize (result.Type);
      return result;
    }

    /// <summary>
    /// Initialize the associated ComponentIntermediateWorkPieces
    /// </summary>
    /// <param name="component"></param>
    public void InitializeComponentIntermediateWorkPieces (IComponent component)
    {
      NHibernateUtil.Initialize (component.ComponentIntermediateWorkPieces);
    }

    /// <summary>
    /// Find components whose names match a pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public IList<IComponent> FindByNameStartPattern (string pattern)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Component> ()
        .Add (Restrictions.Like ("Name", pattern, MatchMode.Start, null))
        .List<IComponent> ();
    }
    #endregion // Methods
  }
}
