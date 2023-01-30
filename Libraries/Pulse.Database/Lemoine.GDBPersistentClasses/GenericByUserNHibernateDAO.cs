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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHibernate DAO
  /// for tables that are partitioned by user
  /// </summary>
  public abstract class GenericByUserNHibernateDAO<T, I, ID>
    : BaseVersionableNHibernateDAO<T, I, ID>, IGenericByUserUpdateDAO<I, ID>
    where T : class, I
    where I : Lemoine.Model.IPartitionedByUser, Lemoine.Collections.IDataWithId<ID>, IDataWithVersion
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericByUserNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user">not null</param>
    /// <returns></returns>
    public I FindById (ID id, Lemoine.Model.IUser user)
    {
      Debug.Assert (null != user);
      return FindById (id, user.Id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual I FindById (ID id, int userId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FindById: id={id} userId={userId} type={typeof (T)}");
      }

      try {
        object entityInCache = NHibernateHelper.GetCurrentSession ()
            .GetPersistentCacheOnly<T> (id);
        if ((null != entityInCache)
          && NHibernateUtil.IsInitialized (entityInCache)
          && (entityInCache is I)) {
          var convertedEntity = (I)(T)entityInCache;
          if (log.IsDebugEnabled) {
            log.Debug ($"FindById: the entity id={id} userId={userId} type={typeof (T)} is taken from cache");
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
        .Add (Restrictions.Eq ("UserId", userId))
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
    /// <param name="user"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, IUser user)
    {
      I entity = FindById (id, user);
      UpgradeLock (entity);
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public I FindByIdAndLock (ID id, int userId)
    {
      I entity = FindById (id, userId);
      UpgradeLock (entity);
      return entity;
    }

  }
}
