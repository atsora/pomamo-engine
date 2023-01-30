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
  /// Common DAO methods for all the LineSlots
  /// </summary>
  public abstract class GenericLineSlotDAO<TSlot, I>
    : BeginEndSlotDAO<TSlot, I>, IGenericByLineUpdateDAO<I, int>
    where TSlot: BeginEndSlot, I
    where I: ISlot, IPartitionedByLine
  {
    readonly ILog log = LogManager.GetLogger(typeof (GenericLineSlotDAO<TSlot, I>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    protected GenericLineSlotDAO (bool dayColumn)
      : base (dayColumn)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="line">not null</param>
    /// <returns></returns>
    public I FindById (int id, ILine line)
    {
      Debug.Assert (null != line);
      return FindById (id, line.Id);
    }
    
    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lineId"></param>
    /// <returns></returns>
    public I FindById (int id, int lineId)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: id={0} lineId={1} type={2}", id, lineId, typeof (TSlot));
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
                            "the entity id={0} lineId={1} type={2} is taken from cache",
                            id, lineId, typeof (TSlot));
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
        .Add (Restrictions.Eq ("Line.Id", lineId))
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
    /// <param name="line"></param>
    /// <returns></returns>
    public I FindByIdAndLock (int id, ILine line)
    {
      I entity = FindById (id, line);
      UpgradeLock (entity);
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lineId"></param>
    /// <returns></returns>
    public I FindByIdAndLock (int id, int lineId)
    {
      I entity = FindById (id, lineId);
      UpgradeLock (entity);
      return entity;
    }
    
    /// <summary>
    /// Find all the I slots for the specified line
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public virtual IList<I> FindAll (ILine line)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
    
    /// <summary>
    /// Get all the line slots for the specified line and the specified time range
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> GetListInRange (ILine line, UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
    
    /// <summary>
    /// Get all the line slots for the specified time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> GetListInRange (UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
    
    /// <summary>
    /// Find the slot at the specified UTC date/time
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public virtual I FindAt (ILine line, DateTime dateTime)
    {
      Debug.Assert (null != line);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (AtUtcDateTime (dateTime))
        .UniqueResult<I> ();
    }
  }
}
