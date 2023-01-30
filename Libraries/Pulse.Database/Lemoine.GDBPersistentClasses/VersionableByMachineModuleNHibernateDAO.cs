// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate.Criterion;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHiberate DAO for the IVersionable classes
  /// for tables that are partitioned by machine module
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class VersionableByMachineModuleNHibernateDAO<T, I, ID>
    : BaseVersionableNHibernateDAO<T, I, ID>, IGenericByMachineModuleUpdateDAO<I, ID>
    where T: class, I, Lemoine.Collections.IDataWithId<ID>
    where I: Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByMachineModule
  {
    readonly ILog log = LogManager.GetLogger(typeof (VersionableByMachineModuleNHibernateDAO<T, I, ID>).FullName);

    #region Members
    readonly string m_machineModuleProperty;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Machine property (usually Machine)
    /// </summary>
    protected virtual string MachineModuleProperty {
      get { return m_machineModuleProperty; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModuleProperty">Usually Machine</param>
    protected VersionableByMachineModuleNHibernateDAO (string machineModuleProperty)
    {
      m_machineModuleProperty = machineModuleProperty;
    }
        
    /// <summary>
    /// Get a criterion on the machine module
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineModuleCriterion (int machineModuleId)
    {
      return Restrictions.Eq (this.MachineModuleProperty + ".Id", machineModuleId);
    }
    
    /// <summary>
    /// Get a criterion on the machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    protected internal virtual ICriterion GetMachineModuleCriterion (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      return Restrictions.Eq (this.MachineModuleProperty, machineModule);
    }    
    
    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule">Not null</param>
    /// <returns></returns>
    public I FindById (ID id, IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      return FindById (id, machineModule.Id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    public I FindById (ID id, int machineModuleId)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: id={0} machineModuleId={1} type={2}", id, machineModuleId, typeof (T));
      }

      // Note: entityInCache may be a MachineModificationProxy for example
      // that can't be converted directly to the right modification (e.g. ReasonMachineAssociation),
      // hence the exception management and the cast try
      try {
        var entityInCache = NHibernateHelper.GetCurrentSession ()
          .GetPersistentCacheOnly<T> (id);
        if ((null != entityInCache)
          && NHibernateUtil.IsInitialized (entityInCache)
          && (entityInCache is I)) {
          var convertedEntity = (I)entityInCache;
          if (log.IsDebugEnabled) {
            log.DebugFormat ("FindById: " +
                            "the entity id={0} machineModuleId={1} type={2} is taken from cache",
                            id, machineModuleId, typeof (T));
          }
          return convertedEntity;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ("FindById: getting the item in the persistent cache failed", ex);
        }
      }

      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: run a request to get the entity id={0} type={1}", id, typeof (T));
      }
      var entity = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (GetMachineModuleCriterion (machineModuleId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: " +
                        "the entity id={0} type={1} was queried",
                        id, typeof (T));
      }
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, IMachineModule machineModule)
    {
      I entity = FindById (id, machineModule);
      UpgradeLock (entity);
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, int machineModuleId)
    {
      I entity = FindById (id, machineModuleId);
      UpgradeLock (entity);
      return entity;
    }
  }
}
