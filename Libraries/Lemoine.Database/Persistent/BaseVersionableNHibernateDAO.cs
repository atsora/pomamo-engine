// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using Lemoine.ModelDAO;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Generic NHiberate DAO for the IVersionable classes.
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class BaseVersionableNHibernateDAO<T, I, ID>
    : BaseGenericNHibernateDAO<T, I, ID>, Lemoine.ModelDAO.IBaseGenericUpdateDAO<I, ID>
    where T: class, I, Lemoine.Collections.IDataWithId<ID>
    where I: Lemoine.Model.IDataWithVersion
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BaseVersionableNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override I MakePersistent (I entity)
    {
      // TODO: SaveOrUpdate does not look reliable
      //       in the case an Id was set after a first Save
      //       but the transaction fails after that
      NHibernateHelper.GetCurrentSession ()
        .SaveOrUpdate (entity);
      return entity;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<I> MakePersistentAsync (I entity)
    {
      // TODO: SaveOrUpdate does not look reliable
      //       in the case an Id was set after a first Save
      //       but the transaction fails after that
      await NHibernateHelper.GetCurrentSession ()
        .SaveOrUpdateAsync (entity);
      return entity;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// 
    /// Please note this is pretty inefficient when the machine is partitioned
    /// (the foreign key is not used for the moment)
    /// </summary>
    /// <param name="entity"></param>
    public virtual void UpgradeLock (I entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock (entity, NHibernate.LockMode.Upgrade);
    }
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual I Reload (I entity)
    {
      // Note: there are two methods to reload an entity
      // 1. Use Refresh, although they may be issues when elements of child collections have been deleted
      // 2. Use Evict () followed by Load ()
      // But do not use Merge that does not reload the data at all when it is already persistent
      NHibernateHelper.GetCurrentSession ().Refresh (entity);
      return entity;
    }

    /// <summary>
    /// Try to get an entity in cache
    /// 
    /// This is used by partitioned tables
    /// </summary>
    /// <param name="id"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected bool TryGetInCache (ID id, out I result)
    {
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
            log.Debug ($"TryGetInCache: the entity id={id} type={typeof (T)} is taken from cache");
          }
          result = convertedEntity;
          return true;
        }
        else {
          result = default (T);
          return false;
        }
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ("TryGetInCache: getting the item in the persistent cache failed", ex);
        }
        result = default (T);
        return false;
      }
    }
  }
}
