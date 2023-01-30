// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHibernate DAO
  /// for tables that are partitioned by monitored machine
  /// </summary>
  public abstract class GenericByMonitoredMachineNHibernateDAO<T, I, ID>
    : BaseGenericNHibernateDAO<T, I, ID>, IGenericByMonitoredMachineDAO<I, ID>
    where T: class, I
    where I: Lemoine.Model.IPartitionedByMonitoredMachine
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GenericByMonitoredMachineNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public I FindById (ID id, Lemoine.Model.IMonitoredMachine machine)
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
        log.DebugFormat ("FindById: id={0} machineId={1} type={2}", id, machineId, typeof (T));
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
                            "the entity id={0} machineId={1} type={2} is taken from cache",
                            id, machineId, typeof (T));
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
        .Add (Restrictions.Eq ("Machine.Id", machineId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: " +
                        "the entity id={0} type={1} was queried",
                        id, typeof (T));
      }
      return entity;
    }
  }
}
