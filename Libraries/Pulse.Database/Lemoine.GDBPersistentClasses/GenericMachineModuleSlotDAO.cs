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
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Common DAO methods for all the MachineModuleSlots
  /// </summary>
  public abstract class GenericMachineModuleSlotDAO<TSlot, I>
    : BeginEndSlotDAO<TSlot, I>, IGenericByMachineModuleUpdateDAO<I, int>, IMachineModuleSlotDAO<I>
    where TSlot: BeginEndSlot, I
    where I: ISlot, IPartitionedByMachineModule
  {
    readonly ILog log = LogManager.GetLogger(typeof (GenericMachineModuleSlotDAO<TSlot, I>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    protected GenericMachineModuleSlotDAO (bool dayColumn)
      : base (dayColumn)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public I FindById (int id, IMachineModule machineModule)
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
    public I FindById (int id, int machineModuleId)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: id={0} machineModuleId={1} type={2}", id, machineModuleId, typeof (TSlot));
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
                            "the entity id={0} machineModuleId={1} type={2} is taken from cache",
                            id, machineModuleId, typeof (TSlot));
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
        .Add (Restrictions.Eq ("MachineModule.Id", machineModuleId))
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
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public I FindByIdAndLock (int id, IMachineModule machineModule)
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
    public I FindByIdAndLock (int id, int machineModuleId)
    {
      I entity = FindById (id, machineModuleId);
      UpgradeLock (entity);
      return entity;
    }
    
    /// <summary>
    /// Implementation of IMachineModuleSlotDAO
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual IList<I> FindAll (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
    
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public virtual I FindAt (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (DateTimeKind.Utc == dateTime.Kind);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (AtUtcDateTime (dateTime))
        .UniqueResult<I> ();
    }
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }
        
    /// <summary>
    /// Get the current slot (meaning the last computed slot)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    protected I GetCurrent (IMachineModule machineModule)
    {
      // - Define the current time frame
      DateTime currentFrameEnd = DateTime.UtcNow;
      DateTime currentFrameBegin = currentFrameEnd.Subtract (AnalysisConfigHelper.CurrentTimeFrameDuration);
      DateTime currentFrameEndDay = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetEndDay (currentFrameEnd);
      
      // - Get the last two slots in the current time frame
      IList<I> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Le ("BeginDay", (LowerBound<DateTime>)currentFrameEndDay))
        .Add (Restrictions.Lt ("BeginDateTime", (LowerBound<DateTime>)currentFrameEnd))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.IsNull ("EndDateTime"))
              .Add (Restrictions.Gt ("EndDateTime", (UpperBound<DateTime>)currentFrameEnd)))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .SetMaxResults (2)
        .List<I> ();
      
      if (0 == slots.Count) {
        log.DebugFormat ("GetCurrent: " +
                         "no slot in the current time frame {0}-{1} " +
                         "=> return null",
                         currentFrameBegin, currentFrameEnd);
        return default(I); // null
      }
      else if (1 == slots.Count) {
        log.DebugFormat ("GetCurrentOperationSlot: " +
                         "only one operation slot in the current time frame {0}-{1} " +
                         "=> return it {2}",
                         currentFrameBegin, currentFrameEnd,
                         slots [0]);
        return slots [0];
      }
      else { // Two operation slots
        Debug.Assert (2 == slots.Count);
        if (slots [0].EndDateTime.HasValue
            && (slots [0].EndDateTime.Value < currentFrameEnd)) {
          log.DebugFormat ("GetCurrent: " +
                           "last slot is strictly before currentFrameEnd {0} " +
                           "=> return it {1}",
                           currentFrameEnd,
                           slots [0]);
          return slots [0];
        }
        else {
          log.DebugFormat ("GetCurrent: " +
                           "last slot {0} may be consider in the future " +
                           "=> return the second last operation slot {1}",
                           slots [0],
                           slots [1]);
          return slots [1];
        }
      }
    }
  }
}
