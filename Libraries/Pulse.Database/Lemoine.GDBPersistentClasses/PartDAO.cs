// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.GDBMigration;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of PartDAO.
  /// </summary>
  public class PartDAO
    : IPartDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PartDAO).FullName);

    public virtual bool IsAttachedToSession (Lemoine.Model.IPart persistent)
    {
      var session = NHibernateHelper.GetCurrentSession ();
      return session.Contains (persistent.Component) && session.Contains (persistent.Project);
    }

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old Part into a new one
    /// 
    /// This returns the merged Part
    /// </summary>
    /// <param name="oldPart"></param>
    /// <param name="newPart"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IPart Merge (IPart oldPart,
                        IPart newPart,
                        ConflictResolution conflictResolution)
    {
      if (0 == newPart.ComponentId) { // newPart is not persistent, inverse the arguments
        Debug.Assert (0 != oldPart.ComponentId);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newPart, oldPart,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldPart, newPart, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old Part into a new one
    /// 
    /// This returns the merged Part
    /// </summary>
    /// <param name="oldPart"></param>
    /// <param name="newPart">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    static IPart InternalMerge (IPart oldPart,
                                IPart newPart,
                                ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != newPart.ComponentId);
      
      IComponent oldComponent = oldPart.Component;
      IComponent newComponent = newPart.Component;
      IProject oldProject = oldPart.Project;
      IProject newProject = newPart.Project;
      
      // - Merge the projects
      IProject mergedProject = ModelDAOHelper.DAOFactory.ProjectDAO
        .Merge (oldProject, newProject, conflictResolution);
      
      ModelDAOHelper.DAOFactory.Flush ();
      NHibernateHelper.GetCurrentSession ().Clear ();
      
      // Note: this is not fully clear yet, why some of the lines below are necessary
      //       I thought this should have been done automatically, but there must be a reference problem somewhere
      foreach (Lemoine.Model.IComponent component in mergedProject.Components) {
        if (!component.Equals (newComponent)) {
          oldComponent = component;
          break;
        }
      }
      
      // - Merge the components
      Debug.Assert (object.Equals (oldComponent.Project, mergedProject));
      Debug.Assert (newComponent.Project == mergedProject);
      IComponent mergedComponent = ModelDAOHelper.DAOFactory.ComponentDAO
        .Merge (oldComponent, newComponent, conflictResolution);
      
      Debug.Assert (mergedComponent.Project == mergedProject);
      return new Part ((Component)mergedComponent);
    }
    
    /// <summary>
    /// Lock a part for merge
    /// </summary>
    static void LockForMerge (IPart part)
    {
      ((ComponentDAO)ModelDAOHelper.DAOFactory.ComponentDAO).LockForMerge (part.Component);
      ((ProjectDAO)ModelDAOHelper.DAOFactory.ProjectDAO).LockForMerge (part.Project);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    public IPart FindById (int componentId)
    {
      return FindByComponentId (componentId);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IPart> FindByIdAsync (int componentId)
    {
      return await FindByComponentIdAsync (componentId);
    }

    /// <summary>
    /// FindByComponentId implementation
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    public IPart FindByComponentId (int componentId)
    {
      IComponent component = ModelDAOHelper.DAOFactory.ComponentDAO
        .FindById (componentId);
      if (null == component) {
        return null;
      }
      else {
        return component.Part;
      }
    }

    /// <summary>
    /// FindByComponentId implementation
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IPart> FindByComponentIdAsync (int componentId)
    {
      IComponent component = await ModelDAOHelper.DAOFactory.ComponentDAO
        .FindByIdAsync (componentId);
      if (null == component) {
        return null;
      }
      else {
        return component.Part;
      }
    }

    /// <summary>
    /// FindByProjectId implementation
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public IPart FindByProjectId (int projectId)
    {
      IProject project = ModelDAOHelper.DAOFactory.ProjectDAO
        .FindById (projectId);
      if (null == project) {
        return null;
      }
      else {
        return project.Part;
      }
    }
    
    /// <summary>
    /// <see cref="IPartDAO"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IPart FindByName (string name)
    {
      IProject project = ModelDAOHelper.DAOFactory.ProjectDAO
        .FindByName (name);
      if (null == project) {
        return null;
      }
      else {
        return project.Part;
      }
    }

    /// <summary>
    /// <see cref="IPartDAO"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IPart FindByCode (string code)
    {
      IProject project = ModelDAOHelper.DAOFactory.ProjectDAO
        .FindByCode (code);
      if (null == project) {
        return null;
      }
      else {
        return project.Part;
      }
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public IList<IPart> FindAll ()
    {
      IList<IPart> parts = new List<IPart> ();
      IList<IComponent> components = ModelDAOHelper.DAOFactory.ComponentDAO.FindAll ();
      foreach (IComponent component in components) {
        parts.Add (component.Part);
      }
      return parts;
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IList<IPart>> FindAllAsync ()
    {
      IList<IPart> parts = new List<IPart> ();
      IList<IComponent> components = await ModelDAOHelper.DAOFactory.ComponentDAO.FindAllAsync ();
      foreach (IComponent component in components) {
        parts.Add (component.Part);
      }
      return parts;
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="part">not null</param>
    public void MakeTransient (IPart part)
    {
      Debug.Assert (null != part);
      
      IComponent component = ((Part)part).Component;
      IProject project = component.Project;
      project.RemoveComponent (component);
      ModelDAOHelper.DAOFactory.ComponentDAO.MakeTransient (component);
      ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransient (project);
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="part">not null</param>
    public async System.Threading.Tasks.Task MakeTransientAsync (IPart part)
    {
      Debug.Assert (null != part);

      IComponent component = ((Part)part).Component;
      IProject project = component.Project;
      project.RemoveComponent (component);
      await ModelDAOHelper.DAOFactory.ComponentDAO.MakeTransientAsync (component);
      await ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransientAsync (project);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="part">not null</param>
    public void Lock (IPart part)
    {
      Debug.Assert (null != part);
      
      IComponent component = part.Component;
      ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
      IProject project = part.Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="part">not null</param>
    public async System.Threading.Tasks.Task LockAsync (IPart part)
    {
      Debug.Assert (null != part);

      IComponent component = part.Component;
      await ModelDAOHelper.DAOFactory.ComponentDAO.LockAsync (component);
      IProject project = part.Project;
      await ModelDAOHelper.DAOFactory.ProjectDAO.LockAsync (project);
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="componentId"></param>
    /// <returns></returns>
    public IPart FindByIdAndLock (int componentId)
    {
      IComponent component = ModelDAOHelper.DAOFactory.ComponentDAO
        .FindByIdAndLock (componentId);
      if (null == component) {
        return null;
      }
      else {
        ModelDAOHelper.DAOFactory.ProjectDAO.UpgradeLock (component.Project);
        return component.Part;
      }
    }
    
    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="part">not null</param>
    /// <returns></returns>
    public IPart MakePersistent (IPart part)
    {
      Debug.Assert (null != part);
      
      ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (((Part)part).Component.Project);
      ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (((Part)part).Component);
      return part;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="part">not null</param>
    /// <returns></returns>
    public async Task<IPart> MakePersistentAsync (IPart part)
    {
      Debug.Assert (null != part);

      await ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistentAsync (((Part)part).Component.Project);
      await ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistentAsync (((Part)part).Component);
      return part;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="part">not null</param>
    public void UpgradeLock (IPart part)
    {
      Debug.Assert (null != part);
      
      IComponent component = ((Part)part).Component;
      ModelDAOHelper.DAOFactory.ComponentDAO.UpgradeLock (component);
      IProject project = part.Project;
      ModelDAOHelper.DAOFactory.ProjectDAO.UpgradeLock (project);
    }
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails)
    /// </summary>
    /// <param name="part">not null</param>
    /// <returns></returns>
    public IPart Reload (IPart part)
    {
      Debug.Assert (null != part);
      
      IComponent component = ((Part)part).Component;
      component = ModelDAOHelper.DAOFactory.ComponentDAO.Reload (component);
      return component.Part;
    }
  }
}
