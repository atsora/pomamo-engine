// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using Lemoine.Core.Log;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Common DAO methods for all the MachineRangeSlots
  /// </summary>
  public abstract class GenericMachineRangeSlotDAO<TSlot, I>
    : RangeSlotDAO<TSlot, I>
    , IGenericByMachineUpdateDAO<I, int>
    , IMachineSlotDAO<I>
    , IOverlapsRangeByStepDefaultStrategy<I, IMachine>
    where TSlot : GenericMachineRangeSlot, I
    where I : ISlot, IPartitionedByMachine
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericMachineRangeSlotDAO<TSlot, I>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    /// <param name="lowerInfPossible"></param>
    /// <param name="upperInfPossible"></param>
    /// <param name="lowerIncPossible"></param>
    /// <param name="upperIncPossible"></param>
    protected GenericMachineRangeSlotDAO (bool dayColumn, bool lowerInfPossible, bool upperInfPossible, bool lowerIncPossible, bool upperIncPossible)
      : base (dayColumn, lowerInfPossible, upperInfPossible, lowerIncPossible, upperIncPossible)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public I FindById (int id, IMachine machine)
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
    public I FindById (int id, int machineId)
    {
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: id={0} machineId={1} type={2}", id, machineId, typeof (TSlot));
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
            log.DebugFormat ("FindById: the entity id={0} machineId={1} type={2} is taken from cache",
                            id, machineId, typeof (TSlot));
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
        .Add (Restrictions.Eq ("Machine.Id", machineId))
        .UniqueResult<I> ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("FindById: the entity id={0} type={1} was queried",
                        id, typeof (TSlot));
      }
      return entity;
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public I FindByIdAndLock (int id, IMachine machine)
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
    public I FindByIdAndLock (int id, int machineId)
    {
      I entity = FindById (id, machineId);
      UpgradeLock (entity);
      return entity;
    }

    /// <summary>
    /// Implementation of IMachineSlotDAO
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual IList<I> FindAll (IMachine machine)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: Gist indexes do not support sorting, so the upper index is used instead
        .AddOrder (Order.Asc (GetUpperProjection ())) // NULLS LAST is the default
        .List<I> ();
    }

    /// <summary>
    /// Implementation of IMachineSlotDAO
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindAllAsync (IMachine machine)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: Gist indexes do not support sorting, so the upper index is used instead
        .AddOrder (Order.Asc (GetUpperProjection ())) // NULLS LAST is the default
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public override I FindAt (Bound<DateTime> at)
    {
      log.Fatal ("FindAt: invalid method for a machine slot");
      throw new InvalidOperationException ("Invalid method for a machine slot");
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<I> FindAtAsync (Bound<DateTime> at)
    {
      log.Fatal ("FindAtAsync: invalid method for a machine slot");
      return await System.Threading.Tasks.Task.FromException<I> (new InvalidOperationException ("Invalid method for a machine slot"));
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public virtual I FindAt (IMachine machine, Bound<DateTime> at)
    {
      Debug.Assert (null != machine);

      if (at.HasValue) {
        DateTime utc;
        switch (at.Value.Kind) {
          case DateTimeKind.Utc:
            utc = at.Value;
            break;
          case DateTimeKind.Local:
            utc = at.Value.ToUniversalTime ();
            break;
          case DateTimeKind.Unspecified:
          default:
            log.Error ($"FindAt: date/time {at} is of kind Unspecified");
            Debug.Assert (DateTimeKind.Unspecified != at.Value.Kind);
            utc = at.Value;
            break;
        }
        var result = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<TSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
          .UniqueResult<I> ();
        if (result is not null && !result.DateTimeRange.ContainsElement (at)) {
          log.Fatal ($"FindAt: returned element with range{result.DateTimeRange} does not contain {at}");
        }
        return result;
      }
      else { // !at.HasValue
        switch (at.BoundType) {
          case BoundType.Lower:
            return NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<TSlot> ()
              .Add (Restrictions.Eq ("Machine.Id", machine.Id))
              .Add (new FunctionExpression ("DateTimeRange", "lower_inf"))
              .UniqueResult<I> ();
          case BoundType.Upper:
            return NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<TSlot> ()
              .Add (Restrictions.Eq ("Machine.Id", machine.Id))
              .Add (new FunctionExpression ("DateTimeRange", "upper_inf"))
              .UniqueResult<I> ();
          default:
            log.FatalFormat ("Unexpected BoundType");
            throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<I> FindAtAsync (IMachine machine, Bound<DateTime> at)
    {
      Debug.Assert (null != machine);

      if (at.HasValue) {
        DateTime utc;
        switch (at.Value.Kind) {
        case DateTimeKind.Utc:
          utc = at.Value;
          break;
        case DateTimeKind.Local:
          utc = at.Value.ToUniversalTime ();
          break;
        case DateTimeKind.Unspecified:
        default:
          log.ErrorFormat ("FindAt: " +
                           "date/time {0} is of kind Unspecified",
                           at);
          Debug.Assert (DateTimeKind.Unspecified != at.Value.Kind);
          utc = at.Value;
          break;
        }
        return await NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<TSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
          .UniqueResultAsync<I> ();
      }
      else { // !at.HasValue
        switch (at.BoundType) {
        case BoundType.Lower:
          return await NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (new FunctionExpression ("DateTimeRange", "lower_inf"))
            .UniqueResultAsync<I> ();
        case BoundType.Upper:
          return await NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (new FunctionExpression ("DateTimeRange", "upper_inf"))
            .UniqueResultAsync<I> ();
        default:
          log.FatalFormat ("Unexpected BoundType");
          throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRange: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<I> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRange: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<I> ();
      }

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified day range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindInDayRange (IMachine machine, DayRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FindInDayRange: empty range => return an empty list. StackTrace: {System.Environment.StackTrace}");
        }
        return new List<I> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified day range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindInDayRangeAsync (IMachine machine, DayRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FindOverlapsRange: empty range => return an empty list. StackTrace: {System.Environment.StackTrace}");
        }
        return new List<I> ();
      }

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InDayRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find all the slots at a specific day
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <returns></returns>
    public virtual IList<I> FindAtDay (IMachine machine, DateTime day)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (AtDay (day))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots at a specific day asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindAtDayAsync (IMachine machine, DateTime day)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (AtDay (day))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// Include the slots that are in the limit
    /// (they must be excluded programmatically then)
    ///  
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindOverlapsRangeWithLimit (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRange: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<I> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRangeWithBounds (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// Include the slots that are in the limit asynchronously
    /// (they must be excluded programmatically then)
    ///  
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindOverlapsRangeWithLimitAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRange: empty range => return an empty list. StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<I> ();
      }

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRangeWithBounds (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    public virtual IEnumerable<I> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        return new List<I> ();
      }

      if (descending) {
        var lowerProjection = GetLowerProjection ();
        var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
        if (range.Upper.HasValue) {
          if (range.UpperInclusive && IsLowerIncPossible ()) {
            criteria = criteria
              .Add (Restrictions.Le (lowerProjection, range.Upper.Value));
          }
          else { // !range.LowerInclusive || !IsUpperIncPossible
            criteria = criteria
              .Add (Restrictions.Lt (lowerProjection, range.Upper.Value));
          }
        }
        IEnumerable<I> result = criteria
          .Add (OverlapRange (range))
          .AddOrder (Order.Desc (lowerProjection))
          .SetMaxResults (n)
          .List<I> ();
        if (IsLowerInfPossible () && result.Count () < n) {
          I withLowerInf = NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (IsLowerInfinite ())
            .Add (OverlapRange (range))
            .UniqueResult<I> ();
          if (null != withLowerInf) {
            result = result
              .Concat (new List<I> { withLowerInf });
          }
        }
        return result;
      }
      else { // ascending
        var upperProjection = GetUpperProjection ();
        var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
        if (range.Lower.HasValue) {
          if (range.LowerInclusive && IsUpperIncPossible ()) {
            criteria = criteria
              .Add (Restrictions.Ge (upperProjection, range.Lower.Value));
          }
          else { // !range.LowerInclusive || !IsUpperIncPossible
            criteria = criteria
              .Add (Restrictions.Gt (upperProjection, range.Lower.Value));
          }
        }
        IEnumerable<I> result = criteria
          .Add (OverlapRange (range))
          .AddOrder (Order.Asc (upperProjection))
          .SetMaxResults (n)
          .List<I> ();
        if (IsUpperInfPossible () && result.Count () < n) {
          I withUpperInf = NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (IsUpperInfinite ())
            .Add (OverlapRange (range))
            .UniqueResult<I> ();
          if (null != withUpperInf) {
            result = result
              .Concat (new List<I> { withUpperInf });
          }
        }
        return result;
      }
    }

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IEnumerable<I>> FindFirstOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        return new List<I> ();
      }

      if (descending) {
        var lowerProjection = GetLowerProjection ();
        var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
        if (range.Upper.HasValue) {
          if (range.UpperInclusive && IsLowerIncPossible ()) {
            criteria = criteria
              .Add (Restrictions.Le (lowerProjection, range.Upper.Value));
          }
          else { // !range.LowerInclusive || !IsUpperIncPossible
            criteria = criteria
              .Add (Restrictions.Lt (lowerProjection, range.Upper.Value));
          }
        }
        IEnumerable<I> result = await criteria
          .Add (OverlapRange (range))
          .AddOrder (Order.Desc (lowerProjection))
          .SetMaxResults (n)
          .ListAsync<I> ();
        if (IsLowerInfPossible () && result.Count () < n) {
          I withLowerInf = NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (IsLowerInfinite ())
            .Add (OverlapRange (range))
            .UniqueResult<I> ();
          if (null != withLowerInf) {
            result = result
              .Concat (new List<I> { withLowerInf });
          }
        }
        return result;
      }
      else { // ascending
        var upperProjection = GetUpperProjection ();
        var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
        if (range.Lower.HasValue) {
          if (range.LowerInclusive && IsUpperIncPossible ()) {
            criteria = criteria
              .Add (Restrictions.Ge (upperProjection, range.Lower.Value));
          }
          else { // !range.LowerInclusive || !IsUpperIncPossible
            criteria = criteria
              .Add (Restrictions.Gt (upperProjection, range.Lower.Value));
          }
        }
        IEnumerable<I> result = await criteria
          .Add (OverlapRange (range))
          .AddOrder (Order.Asc (upperProjection))
          .SetMaxResults (n)
          .ListAsync<I> ();
        if (IsUpperInfPossible () && result.Count () < n) {
          I withUpperInf = await NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (IsUpperInfinite ())
            .Add (OverlapRange (range))
            .UniqueResultAsync<I> ();
          if (null != withUpperInf) {
            result = result
              .Concat (new List<I> { withUpperInf });
          }
        }
        return result;
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in an ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<I> FindOverlapsRangeAscending (IMachine machine,
                                                              UtcDateTimeRange range,
                                                              TimeSpan step)
    {
      var logger = LogManager.GetLogger (typeof (GenericMachineRangeSlotDAO<TSlot, I>).FullName + "." + machine.Id);
      var strategy = new DefaultOverlapsRangeByStepStrategy<I, IMachine> (this, step, false);
      return new FindOverlapsRangeEnumerable<I, IMachine> (strategy, machine, range, logger);
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public virtual IEnumerable<I> FindOverlapsRangeDescending (IMachine machine,
                                                               UtcDateTimeRange range,
                                                               TimeSpan step)
    {
      var logger = LogManager.GetLogger (typeof (GenericMachineRangeSlotDAO<TSlot, I>).FullName + "." + machine.Id);
      var strategy = new DefaultOverlapsRangeByStepStrategy<I, IMachine> (this, step, true);
      return new FindOverlapsRangeEnumerable<I, IMachine> (strategy, machine, range, logger);
    }
  }
}
