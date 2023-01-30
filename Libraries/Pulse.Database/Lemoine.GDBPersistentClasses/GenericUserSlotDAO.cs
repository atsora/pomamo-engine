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
  /// Common DAO methods for all the UserSlots
  /// </summary>
  public class GenericUserSlotDAO<TSlot, I>
    : BeginEndSlotDAO<TSlot, I>, IGenericByUserUpdateDAO<I, int>, IGenericUserSlotDAO<I>
    where TSlot: BeginEndSlot, I
    where I: ISlot, IPartitionedByUser
  {
    readonly ILog log = LogManager.GetLogger(typeof (GenericUserSlotDAO<TSlot, I>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    protected GenericUserSlotDAO (bool dayColumn)
      : base (dayColumn)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user">not null</param>
    /// <returns></returns>
    public I FindById (int id, IUser user)
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
    public I FindById (int id, int userId)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: id={0} userId={1} type={2}", id, userId, typeof (TSlot));
      }

      // Note: entityInCache may be a MachineModificationProxy for example
      // that can't be converted directly to the right modification (e.g. ReasonMachineAssociation),
      // hence the exception management and the cast try
      try {
        var entityInCache = NHibernateHelper.GetCurrentSession ()
          .GetPersistentCacheOnly<TSlot> (id);
        if ((null != entityInCache)
          && NHibernateUtil.IsInitialized (entityInCache)
          && (entityInCache is I)) {
          var convertedEntity = (I)entityInCache;
          if (log.IsDebugEnabled) {
            log.DebugFormat ("FindById: " +
                            "the entity id={0} userId={1} type={2} is taken from cache",
                            id, userId, typeof (TSlot));
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
        log.DebugFormat ("FindById: run a request to get the entity id={0} type={1}", id, typeof (TSlot));
      }
      var entity = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (Restrictions.Eq ("User.Id", userId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: " +
                        "the entity id={0} type={1} was queried",
                        id, typeof (TSlot));
      }
      return entity;

    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public I FindByIdAndLock (int id, IUser user)
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
    public I FindByIdAndLock (int id, int userId)
    {
      I entity = FindById (id, userId);
      UpgradeLock (entity);
      return entity;
    }
    
    /// <summary>
    /// Implementation of IUserSlotDAO
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public virtual IList<I> FindAll (IUser user)
    {
      Debug.Assert (null != user);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public virtual I FindAt (IUser user, DateTime dateTime)
    {
      Debug.Assert (null != user);
      Debug.Assert (DateTimeKind.Utc == dateTime.Kind);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (AtUtcDateTime (dateTime))
        .UniqueResult<I> ();
    }
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindOverlapsRange (IUser user, UtcDateTimeRange range)
    {
      Debug.Assert (null != user);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }


    /// <summary>
    /// Get all the user slots for the specified user and the specified time range
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> GetListInRange (IUser user, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
  }
}
