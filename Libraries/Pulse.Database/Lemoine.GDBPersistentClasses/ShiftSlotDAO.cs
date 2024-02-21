// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using Lemoine.Core.Log;
using NHibernate.Criterion;
using Lemoine.Business.Config;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftSlotDAO">IShiftSlotDAO</see>
  /// </summary>
  public sealed class ShiftSlotDAO
    : RangeSlotDAO<ShiftSlot, IShiftSlot>
    , IShiftSlotDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSlotDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    internal ShiftSlotDAO ()
      : base (false, true, true, true, false)
    {
    }

    /// <summary>
    /// Find the shift slots for a specific day and shift
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    public IList<IShiftSlot> FindWith (DateTime day, IShift shift)
    {
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .Add (Restrictions.Eq ("Shift.Id", shift.Id))
        .SetCacheable (true)
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IShiftSlot> ();
    }

    /// <summary>
    /// Find the shift slots for a specific day and shift
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <param name="shift">not null</param>
    /// <returns></returns>
    public async Task<IList<IShiftSlot>> FindWithAsync (DateTime day, IShift shift)
    {
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .Add (Restrictions.Eq ("Shift.Id", shift.Id))
        .SetCacheable (true)
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<IShiftSlot> ();
    }

    /// <summary>
    /// Find all the shift slots at the specified day
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public IList<IShiftSlot> FindAtDay (DateTime day)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .SetCacheable (true)
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IShiftSlot> ();
    }

    /// <summary>
    /// Return the first slot whose beginning is before (or equal to) a specific date
    /// The end can be after the specific date
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public IShiftSlot GetFirstSlotBeginningBefore (DateTime utcDateTime)
    {
      // Use the shiftslot_lower index (bug: no persister for LowerBound)
      /*return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .SetCacheable (true)
        .Add (Restrictions.Lt (GetLowerProjection (), (LowerBound<DateTime>)utcDateTime))
        .AddOrder (Order.Desc (GetLowerProjection ()))
        .SetMaxResults (1)
        .UniqueResult<IShiftSlot> ();*/

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .SetCacheable (true)
        .Add (BeginBeforeUtcDateTime (utcDateTime))
        .AddOrder (Order.Desc ("DateTimeRange"))
        .SetMaxResults (1)
        .UniqueResult<IShiftSlot> ();
    }

    /// <summary>
    /// Return the first slot whose end is after (or equal to) a specific date
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public IShiftSlot GetFirstSlotEndAfter(DateTime utcDateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .SetCacheable (true)
        .Add (EndAfterUtcDateTime (utcDateTime))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .SetMaxResults (1)
        .UniqueResult<IShiftSlot> ();
    }

    /// <summary>
    /// Find all the shift slots at the specified day
    /// with an early fetch of the breaks
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public IList<IShiftSlot> FindAtDayWithBreaks (DateTime day)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .Fetch (SelectMode.Fetch, "Breaks")
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IShiftSlot> ();
    }

    /// <summary>
    /// Return true if the shift is defined
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="shift">not null</param>
    /// <param name="day"></param>
    /// <returns></returns>
    public bool IsDefined (IShift shift, DateTime day)
    {
      Debug.Assert (null != shift);

      // Note: Shift.Id it used below because else the cache is not managed correctly

      return (NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<ShiftSlot> ()
              .Add (Restrictions.Eq ("Day", day))
              .Add (Restrictions.Eq ("Shift.Id", shift.Id))
              .SetCacheable (true)
              .List<IShiftSlot> ().Count != 0);
    }

    /// <summary>
    /// Get the shift slots which template has not been processed yet in the specified range in ascending order
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IShiftSlot> GetNotProcessTemplate (UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("TemplateProcessed", false))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IShiftSlot> ();
    }

    /// <summary>
    /// Get the shift slots which template has not been processed yet in the specified range in ascending order
    /// </summary>
    /// <param name="range"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public IEnumerable<IShiftSlot> GetNotProcessTemplate (UtcDateTimeRange range, int limit)
    {
      var upperProjection = GetUpperProjection ();
      IEnumerable<IShiftSlot> result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ShiftSlot> ()
        .Add (Restrictions.Eq ("TemplateProcessed", false))
        .Add (Restrictions.Gt (upperProjection, range.Lower.Value))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc (upperProjection))
        .SetMaxResults (limit)
        .List<IShiftSlot> ();
      if (IsUpperInfPossible () && result.Count () < limit) {
        IShiftSlot withUpperInf = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<ShiftSlot> ()
          .Add (Restrictions.Eq ("TemplateProcessed", false))
          .Add (IsUpperInfinite ())
          .Add (OverlapRange (range))
          .UniqueResult<IShiftSlot> ();
        if (null != withUpperInf) {
          result = result
            .Concat (new List<IShiftSlot> { withUpperInf });
        }
      }
      return result;
    }

    /// <summary>
    /// Process the day slots in the specified range
    /// </summary>
    /// <param name="range"></param>
    public void ProcessInRange (CancellationToken cancellationToken, UtcDateTimeRange range)
    {
      ProcessInRange (cancellationToken, range, null);
    }

    /// <summary>
    /// Process the day slots in the specified range
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range"></param>
    /// <param name="checkedThread"></param>
    public void ProcessInRange (CancellationToken cancellationToken, UtcDateTimeRange range, Lemoine.Threading.IChecked checkedThread)
    {
      UtcDateTimeRange effectiveRange = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (AnalysisConfigHelper.MinTemplateProcessDateTime,
                                                                                                      DateTime.UtcNow.Add (AnalysisConfigHelper.MaxShiftSlotProcess))));
      if (effectiveRange.IsEmpty ()) {
        log.WarnFormat ("ProcessInRange: " +
                        "effective range is empty");
        return;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IShiftSlot> shiftSlots = GetNotProcessTemplate (effectiveRange);
        foreach (IShiftSlot shiftSlot in shiftSlots) {
          checkedThread?.SetActive ();
          ((ShiftSlot)shiftSlot).ProcessTemplate (cancellationToken, effectiveRange, checkedThread);
        }
      }
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override IShiftSlot MakePersistent (IShiftSlot entity)
    {
      IShiftSlot result = base.MakePersistent (entity);
      foreach (var b in entity.Breaks) {
        NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdate (b);
      }
      return result;
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override async Task<IShiftSlot> MakePersistentAsync (IShiftSlot entity)
    {
      IShiftSlot result = await base.MakePersistentAsync (entity);
      foreach (var b in entity.Breaks) {
        await NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdateAsync (b);
      }
      return result;
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient (IShiftSlot entity)
    {
      foreach (var b in entity.Breaks) {
        NHibernateHelper.GetCurrentSession ()
          .Delete (b);
      }
      entity.Breaks.Clear ();
      base.MakeTransient (entity);
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IShiftSlot entity)
    {
      foreach (var b in entity.Breaks) {
        await NHibernateHelper.GetCurrentSession ()
          .DeleteAsync (b);
      }
      entity.Breaks.Clear ();
      await base.MakeTransientAsync (entity);
    }
  }
}
