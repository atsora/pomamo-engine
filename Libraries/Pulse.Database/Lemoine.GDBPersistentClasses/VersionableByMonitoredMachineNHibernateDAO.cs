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
  /// for tables that are partitioned by monitored machine
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class VersionableByMonitoredMachineNHibernateDAO<T, I, ID>
    : BaseVersionableNHibernateDAO<T, I, ID>, IGenericByMonitoredMachineUpdateDAO<I, ID>
    where T: class, I, Lemoine.Collections.IDataWithId<ID>
    where I: Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByMonitoredMachine
  {
    static readonly ILog log = LogManager.GetLogger(typeof (VersionableByMonitoredMachineNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public I FindById (ID id, IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);
      return FindById (id, machine.Id);
    }
    
    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public I FindById (ID id, int machineId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FindById: id={id} machineId={machineId} type={typeof (T)}");
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
            log.Debug ($"FindById: the entity id={id} machineId={machineId} type={typeof (T)} is taken from cache");
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
        log.Debug ($"FindById: run a request to get the entity id={id} type={typeof (T)}");
      }
      var entity = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (Restrictions.Eq ("Machine.Id", machineId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.Debug ($"FindById: the entity id={id} type={typeof (T)} was queried");
      }
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, IMonitoredMachine machine)
    {
      I entity = FindById (id, machine);
      UpgradeLock (entity);
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, int machineId)
    {
      I entity = FindById (id, machineId);
      UpgradeLock (entity);
      return entity;
    }
  }
}
