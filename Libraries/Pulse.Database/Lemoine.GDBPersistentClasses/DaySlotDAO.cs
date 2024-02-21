// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDaySlotDAO">IDaySlotDAO</see>
  /// </summary>
  public sealed class DaySlotDAO
    : RangeSlotDAO<DaySlot, IDaySlot>
    , IDaySlotDAO
  {
    ILog log = LogManager.GetLogger (typeof (DaySlotDAO).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal DaySlotDAO ()
      : base (false, true, true, true, false)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Clear the DaySlotCache
    /// </summary>
    public void ClearCache ()
    {
      Lemoine.GDBPersistentClasses.DaySlotCache.Clear ();
    }

    /// <summary>
    /// <see cref="IDaySlotDAO"/>
    /// </summary>
    /// <returns></returns>
    public IList<IDaySlot> FindWithNoWeekNumber ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<DaySlot> ()
        .Add (Restrictions.IsNotNull ("Day"))
        .Add (Restrictions.IsNull ("WeekNumber"))
        .List<IDaySlot> ();
    }

    /// <summary>
    /// Get the day slots which template has not been processed yet in the specified range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IDaySlot> GetNotProcessTemplate (UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<DaySlot> ()
        .Add (Restrictions.IsNull ("Day"))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // overlap
        .List<IDaySlot> ();
    }

    /// <summary>
    /// Get the day slots which template has not been processed yet in the specified range
    /// </summary>
    /// <param name="range"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public IList<IDaySlot> GetNotProcessTemplate (UtcDateTimeRange range, int limit)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<DaySlot> ()
        .Add (Restrictions.IsNull ("Day"))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // overlap
        .SetMaxResults (limit)
        .List<IDaySlot> ();
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public override IDaySlot FindAt (Bound<DateTime> at)
    {
      IDaySlot daySlot = base.FindAt (at);
      DaySlotCache.Add (daySlot);
      return daySlot;
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time asynchronously
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<IDaySlot> FindAtAsync (Bound<DateTime> at)
    {
      IDaySlot daySlot = await base.FindAtAsync (at);
      DaySlotCache.Add (daySlot);
      return daySlot;
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="dateTime">in UTC</param>
    /// <param name="useCache">Use the cache if it is available</param>
    /// <returns></returns>
    public IDaySlot FindAt (DateTime dateTime, bool useCache)
    {
      if (useCache) { // Try using the cache
        IDaySlot daySlot;
        if (DaySlotCache.TryGetValueFromDateTime (dateTime, out daySlot)) {
          return daySlot;
        }
      }

      return FindAt (dateTime);
    }

    /// <summary>
    /// Find by day
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public IDaySlot FindByDay (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind); // Unspecified or Local
      Debug.Assert (day.Equals (day.Date));

      IDaySlot daySlot = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<DaySlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .SetCacheable (true)
        .UniqueResult<IDaySlot> ();
      DaySlotCache.Add (daySlot);
      return daySlot;
    }

    /// <summary>
    /// Find by day
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IDaySlot> FindByDayAsync (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind); // Unspecified or Local
      Debug.Assert (day.Equals (day.Date));

      IDaySlot daySlot = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<DaySlot> ()
        .Add (Restrictions.Eq ("Day", day))
        .SetCacheable (true)
        .UniqueResultAsync<IDaySlot> ();
      DaySlotCache.Add (daySlot);
      return daySlot;
    }

    /// <summary>
    /// Find the processed day slot at the specified date/time
    /// 
    /// The cache may be used if it is active
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IDaySlot FindProcessedAt (DateTime dateTime)
    {
      { // Try using the cache
        IDaySlot daySlot;
        if (DaySlotCache.TryGetValueFromDateTime (dateTime, out daySlot)) {
          return daySlot;
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DaySlot daySlot = (DaySlot)FindAt (dateTime);
        Debug.Assert (null != daySlot);
        if (daySlot.Day.HasValue) { // Processed
          return daySlot;
        }
        else { // Not processed yet => do it now
          if (ModelDAOHelper.DAOFactory.IsTransactionReadOnly ()) {
            // alternative method to compute live a day slot at a given time
            // (i.e. without updating the database)
            log.WarnFormat ("FindProcessedAt: " +
                            "the transaction is read-only " +
                            "=> compute live the day slot at {0}",
                            dateTime);
            return GetLiveAt (daySlot.DayTemplate, dateTime);
          }
          else { // Read-write transaction
            log.WarnFormat ("FindProcessedAt: " +
                            "slot {0} was not processed yet, do it now",
                            daySlot);
            daySlot.ProcessTemplate (CancellationToken.None, new UtcDateTimeRange (dateTime,
                                                           dateTime,
                                                           "[]"),
                                     null, false, null, null);
            return FindAt (dateTime);
          }
        }
      }
    }

    /// <summary>
    /// Find the processed day slot at the specified date/time asynchronously
    /// 
    /// The cache may be used if it is active
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IDaySlot> FindProcessedAtAsync (DateTime dateTime)
    {
      { // Try using the cache
        IDaySlot daySlot;
        if (DaySlotCache.TryGetValueFromDateTime (dateTime, out daySlot)) {
          return daySlot;
        }
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DaySlot daySlot = (DaySlot)(await FindAtAsync (dateTime));
        Debug.Assert (null != daySlot);
        if (daySlot.Day.HasValue) { // Processed
          return daySlot;
        }
        else { // Not processed yet => do it now
          if (ModelDAOHelper.DAOFactory.IsTransactionReadOnly ()) {
            // alternative method to compute live a day slot at a given time
            // (i.e. without updating the database)
            log.WarnFormat ("FindProcessedAt: " +
                            "the transaction is read-only " +
                            "=> compute live the day slot at {0}",
                            dateTime);
            return GetLiveAt (daySlot.DayTemplate, dateTime);
          }
          else { // Read-write transaction
            log.WarnFormat ("FindProcessedAt: " +
                            "slot {0} was not processed yet, do it now",
                            daySlot);
            daySlot.ProcessTemplate (CancellationToken.None, new UtcDateTimeRange (dateTime,
                                                           dateTime,
                                                           "[]"),
                                     null, false, null, null);
            return await FindAtAsync (dateTime);
          }
        }
      }
    }

    /// <summary>
    /// Get live a day slot for the specific day template and date/time
    /// 
    /// This is suitable for read-only transactions
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IDaySlot GetLiveAt (IDayTemplate dayTemplate, DateTime dateTime)
    {
      DateTime day = dateTime.ToLocalTime ().AddDays (-1).Date;
      DateTime lastDay = dateTime.ToLocalTime ().AddDays (1).Date;
      while (day <= lastDay) { // Loop on days
        // Process the item one after each other
        // until 'end' only
        foreach (IDayTemplateItem item in dayTemplate.Items) {
          if (item.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) { // Day of week is ok
            TimeSpan correctedCutOff = item.CutOff;
            if (TimeSpan.FromHours (12) < correctedCutOff) {
              correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
            }

            Debug.Assert (TimeSpan.FromHours (-12) < correctedCutOff);
            Debug.Assert (correctedCutOff <= TimeSpan.FromHours (12));
            Debug.Assert (DateTimeKind.Utc != day.Kind);
            Debug.Assert (day.Equals (day.Date));

            DateTime dayLocalBegin = day.Add (correctedCutOff);
            DateTime dayLocalEnd;

            { // - Get the dayEnd which is the begin time of the next day, else consider the same cut-off
              //   although this may be approximative.
              DateTime nextDay = day.AddDays (1);
              IDaySlot nextDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (nextDay);
              if (null != nextDaySlot) {
                Debug.Assert (nextDaySlot.BeginDateTime.HasValue);
                dayLocalEnd = nextDaySlot.BeginDateTime.Value;
              }
              else {
                dayLocalEnd = nextDay.Add (correctedCutOff);
              }
            }
            UtcDateTimeRange dayUtcRange = new UtcDateTimeRange (dayLocalBegin, dayLocalEnd);
            if (dayUtcRange.ContainsElement (dateTime)) { // The right day is found
              log.DebugFormat ("GetLiveAt: " +
                               "day {0} {1} was found for {2}",
                               day, dayUtcRange, dateTime);
              IDaySlot daySlot = ModelDAOHelper.ModelFactory.CreateDaySlot (dayTemplate, dayUtcRange);
              daySlot.Day = day;
              return daySlot;
            }
          } // DayOfWeek
        } // Item loop
        day = day.AddDays (1).Date;
      }

      log.ErrorFormat ("GetLiveAt: " +
                       "there is no matching template item at {0} " +
                       "=> the day slot could not be computed live",
                       dateTime);
      throw new InvalidOperationException ("DaySlotDAO.GetLiveAt failure");
    }

    /// <summary>
    /// Get live a day slot for the specific day template and date/time asynchronously
    /// 
    /// This is suitable for read-only transactions
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    async System.Threading.Tasks.Task<IDaySlot> GetLiveAtAsync (IDayTemplate dayTemplate, DateTime dateTime)
    {
      DateTime day = dateTime.ToLocalTime ().AddDays (-1).Date;
      DateTime lastDay = dateTime.ToLocalTime ().AddDays (1).Date;
      while (day <= lastDay) { // Loop on days
        // Process the item one after each other
        // until 'end' only
        foreach (IDayTemplateItem item in dayTemplate.Items) {
          if (item.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) { // Day of week is ok
            TimeSpan correctedCutOff = item.CutOff;
            if (TimeSpan.FromHours (12) < correctedCutOff) {
              correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
            }

            Debug.Assert (TimeSpan.FromHours (-12) < correctedCutOff);
            Debug.Assert (correctedCutOff <= TimeSpan.FromHours (12));
            Debug.Assert (DateTimeKind.Utc != day.Kind);
            Debug.Assert (day.Equals (day.Date));

            DateTime dayLocalBegin = day.Add (correctedCutOff);
            DateTime dayLocalEnd;

            { // - Get the dayEnd which is the begin time of the next day, else consider the same cut-off
              //   although this may be approximative.
              DateTime nextDay = day.AddDays (1);
              IDaySlot nextDaySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDayAsync (nextDay);
              if (null != nextDaySlot) {
                Debug.Assert (nextDaySlot.BeginDateTime.HasValue);
                dayLocalEnd = nextDaySlot.BeginDateTime.Value;
              }
              else {
                dayLocalEnd = nextDay.Add (correctedCutOff);
              }
            }
            UtcDateTimeRange dayUtcRange = new UtcDateTimeRange (dayLocalBegin, dayLocalEnd);
            if (dayUtcRange.ContainsElement (dateTime)) { // The right day is found
              if (log.IsDebugEnabled) {
                log.Debug ($"GetLiveAt: day {day} {dayUtcRange} was found for {dateTime}");
              }
              IDaySlot daySlot = ModelDAOHelper.ModelFactory.CreateDaySlot (dayTemplate, dayUtcRange);
              daySlot.Day = day;
              return daySlot;
            }
          } // DayOfWeek
        } // Item loop
        day = day.AddDays (1).Date;
      }

      log.ErrorFormat ("GetLiveAt: " +
                       "there is no matching template item at {0} " +
                       "=> the day slot could not be computed live",
                       dateTime);
      throw new InvalidOperationException ("DaySlotDAO.GetLiveAt failure");
    }

    /// <summary>
    /// Find the processed day slot at the specified day
    /// 
    /// The cache may be used if it is active
    /// 
    /// null is returned if the requested day is too old
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public IDaySlot FindProcessedByDay (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      Debug.Assert (object.Equals (day.Date, day));

      if (day < AnalysisConfigHelper.MinTemplateProcessDateTime) {
        log.Warn ($"FindProcessedByDay: the day {day} is too old => fallback to null directly");
        return null;
      }

      { // Try using the cache
        IDaySlot daySlot;
        if (DaySlotCache.TryGetValueFromDay (day, out daySlot)) {
          return daySlot;
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DaySlot daySlot = (DaySlot)FindByDay (day);
        if (null == daySlot) { // Not processed yet => do it now
          if (ModelDAOHelper.DAOFactory.IsTransactionReadOnly ()) {
            log.Warn ($"FindProcessedByDay: the transaction is read-only => compute live the day slot for day {day}");
            return GetLiveDay (day);
          }
          else { // Read-write transaction
            log.Warn ($"FindProcessedByDay: slot at day {day} was not processed yet, do it now");
            UtcDateTimeRange range = new UtcDateTimeRange (day.AddDays (-1),
                                                           day.AddDays (2));
            IList<IDaySlot> slots = FindOverlapsRange (range);
            foreach (IDaySlot slot in slots) {
              if (!slot.Day.HasValue) {
                ((DaySlot)slot).ProcessTemplate (CancellationToken.None, range,
                                                 null, false, null, null);
              }
            }
            return FindByDay (day);
          }
        }
        else { // null != daySlot
          return daySlot;
        }
      }
    }

    /// <summary>
    /// Find the processed day slot at the specified day asynchronously
    /// 
    /// The cache may be used if it is active
    /// 
    /// null is returned if the requested day is too old
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IDaySlot> FindProcessedByDayAsync (DateTime day)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      Debug.Assert (object.Equals (day.Date, day));

      if (day < AnalysisConfigHelper.MinTemplateProcessDateTime) {
        log.Warn ($"FindProcessedByDayAsync: the day {day} is too old => fallback to null directly");
        return null;
      }

      { // Try using the cache
        IDaySlot daySlot;
        if (DaySlotCache.TryGetValueFromDay (day, out daySlot)) {
          return daySlot;
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DaySlot daySlot = (DaySlot)FindByDay (day);
        if (null == daySlot) { // Not processed yet => do it now
          if (ModelDAOHelper.DAOFactory.IsTransactionReadOnly ()) {
            log.Warn ($"FindProcessedByDayAsync: the transaction is read-only => compute live the day slot for day {day}");
            return await GetLiveDayAsync (day);
          }
          else { // Read-write transaction
            log.Warn ($"FindProcessedByDayAsync: slot at day {day} was not processed yet, do it now");
            UtcDateTimeRange range = new UtcDateTimeRange (day.AddDays (-1),
                                                           day.AddDays (2));
            IList<IDaySlot> slots = await FindOverlapsRangeAsync (range);
            foreach (IDaySlot slot in slots) {
              if (!slot.Day.HasValue) {
                ((DaySlot)slot).ProcessTemplate (CancellationToken.None, range,
                                                 null, false, null, null);
              }
            }
            return FindByDay (day);
          }
        }
        else { // null != daySlot
          return daySlot;
        }
      }
    }

    /// <summary>
    /// Compute live the day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    IDaySlot GetLiveDay (DateTime day)
    {
      bool addToCache = true;
      // - Get the possible day templates
      DateTime minDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddHours (-12).ToUniversalTime ();
      DateTime maxDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddDays (1).AddHours (12).ToUniversalTime ();
      IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
        .FindOverlapsRange (new UtcDateTimeRange (minDateTime, maxDateTime));
      foreach (IDaySlot slot in slots) {
        if (slot.Day.HasValue) {
          if (slot.Day.Value.Equals (day)) {
            Debug.Assert (false); // Normally not, else there is no need to compute it live
            // but the result is ok
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetLiveDay: " +
                               "got day slot {0} in database directly",
                               slot);
            }
            return slot;
          }
          else { // Not the right day, go to the next day template
            continue;
          }
        }
        else { // !daySlot.Day.HasValue
          IDayTemplate dayTemplate = slot.DayTemplate;
          Debug.Assert (null != dayTemplate);
          foreach (IDayTemplateItem item in dayTemplate.Items) {
            if (item.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) { // Day of week is ok
              TimeSpan correctedCutOff = item.CutOff;
              if (TimeSpan.FromHours (12) < correctedCutOff) {
                correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
              }

              Debug.Assert (TimeSpan.FromHours (-12) < correctedCutOff);
              Debug.Assert (correctedCutOff <= TimeSpan.FromHours (12));
              Debug.Assert (DateTimeKind.Utc != day.Kind);
              Debug.Assert (day.Equals (day.Date));

              DateTime dayLocalBegin = day.Add (correctedCutOff);
              DateTime dayEnd;

              { // - Get the dayEnd which is the begin time of the next day, else consider the same cut-off
                //   although this may be approximative.
                DateTime nextDay = day.AddDays (1);
                IDaySlot nextDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindByDay (nextDay);
                if (null != nextDaySlot) {
                  Debug.Assert (nextDaySlot.BeginDateTime.HasValue);
                  dayEnd = nextDaySlot.BeginDateTime.Value;
                }
                else {
                  try {
                    dayEnd = GetLiveDayBegin (nextDay);
                  }
                  catch (Exception) {
                    log.Error ("GetLiveDay: fallback using the cut-off time because GetLiveDayBegin failed");
                    addToCache = false;
                    dayEnd = nextDay.Add (correctedCutOff);
                  }
                }
              }
              UtcDateTimeRange dayUtcRange = new UtcDateTimeRange (dayLocalBegin, dayEnd);
              if (slot.DateTimeRange.ContainsRange (dayUtcRange)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetLiveDay: day slot is {dayUtcRange} for {day}");
                }
                IDaySlot daySlot = ModelDAOHelper.ModelFactory.CreateDaySlot (dayTemplate, dayUtcRange);
                daySlot.Day = day;
                if (addToCache) {
                  DaySlotCache.Add (daySlot);
                }
                return daySlot;
              }
            } // DayOfWeek
          } // Item loop
        }
      }

      log.Error ($"GetLiveDay: there is no matching template item for day {day} => the day slot could not be computed live");
      throw new InvalidOperationException ("DaySlotDAO.GetLiveDay failure");
    }

    /// <summary>
    /// Compute live the day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    async System.Threading.Tasks.Task<IDaySlot> GetLiveDayAsync (DateTime day)
    {
      bool addToCache = true;
      // - Get the possible day templates
      DateTime minDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddHours (-12).ToUniversalTime ();
      DateTime maxDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddDays (1).AddHours (12).ToUniversalTime ();
      IList<IDaySlot> slots = await ModelDAOHelper.DAOFactory.DaySlotDAO
        .FindOverlapsRangeAsync (new UtcDateTimeRange (minDateTime, maxDateTime));
      foreach (IDaySlot slot in slots) {
        if (slot.Day.HasValue) {
          if (slot.Day.Value.Equals (day)) {
            Debug.Assert (false); // Normally not, else there is no need to compute it live
            // but the result is ok
            if (log.IsDebugEnabled) {
              log.Debug ($"GetLiveDayAsync: got day slot {slot} in database directly");
            }
            return slot;
          }
          else { // Not the right day, go to the next day template
            continue;
          }
        }
        else { // !daySlot.Day.HasValue
          IDayTemplate dayTemplate = slot.DayTemplate;
          Debug.Assert (null != dayTemplate);
          foreach (IDayTemplateItem item in dayTemplate.Items) {
            if (item.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) { // Day of week is ok
              TimeSpan correctedCutOff = item.CutOff;
              if (TimeSpan.FromHours (12) < correctedCutOff) {
                correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
              }

              Debug.Assert (TimeSpan.FromHours (-12) < correctedCutOff);
              Debug.Assert (correctedCutOff <= TimeSpan.FromHours (12));
              Debug.Assert (DateTimeKind.Utc != day.Kind);
              Debug.Assert (day.Equals (day.Date));

              DateTime dayLocalBegin = day.Add (correctedCutOff);
              DateTime dayEnd;

              { // - Get the dayEnd which is the begin time of the next day, else consider the same cut-off
                //   although this may be approximative.
                DateTime nextDay = day.AddDays (1);
                IDaySlot nextDaySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO.FindByDayAsync (nextDay);
                if (null != nextDaySlot) {
                  Debug.Assert (nextDaySlot.BeginDateTime.HasValue);
                  dayEnd = nextDaySlot.BeginDateTime.Value;
                }
                else {
                  try {
                    dayEnd = GetLiveDayBegin (nextDay);
                  }
                  catch (Exception) {
                    log.Error ("GetLiveDayAsync: fallback using the cut-off time because GetLiveDayBegin failed");
                    addToCache = false;
                    dayEnd = nextDay.Add (correctedCutOff);
                  }
                }
              }
              UtcDateTimeRange dayUtcRange = new UtcDateTimeRange (dayLocalBegin, dayEnd);
              if (slot.DateTimeRange.ContainsRange (dayUtcRange)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetLiveDayAsync: day slot is {dayUtcRange} for {day}");
                }
                IDaySlot daySlot = ModelDAOHelper.ModelFactory.CreateDaySlot (dayTemplate, dayUtcRange);
                daySlot.Day = day;
                if (addToCache) {
                  DaySlotCache.Add (daySlot);
                }
                return daySlot;
              }
            } // DayOfWeek
          } // Item loop
        }
      }

      log.ErrorFormat ("GetLiveDay: " +
                       "there is no matching template item for day {0} " +
                       "=> the day slot could not be computed live",
                       day);
      throw new InvalidOperationException ("DaySlotDAO.GetLiveDay failure");
    }

    /// <summary>
    /// Compute live the day begin
    /// 
    /// This method is not recursive
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    DateTime GetLiveDayBegin (DateTime day)
    {
      // - Get the possible day templates
      DateTime minDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddHours (-12).ToUniversalTime ();
      DateTime maxDateTime =
        new DateTime (day.Year, day.Month, day.Day, 00, 00, 00, DateTimeKind.Local)
        .AddDays (1).AddHours (12).ToUniversalTime ();
      IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
        .FindOverlapsRange (new UtcDateTimeRange (minDateTime, maxDateTime));
      foreach (IDaySlot slot in slots) {
        if (slot.Day.HasValue) {
          if (slot.Day.Value.Equals (day)) {
            Debug.Assert (false); // Normally not, else there is no need to compute it live
            // but the result is ok
            log.DebugFormat ("GetLiveDayBegin: " +
                             "got day slot {0} in database directly",
                             slot);
            Debug.Assert (slot.BeginDateTime.HasValue);
            return slot.BeginDateTime.Value;
          }
          else { // Not the right day, go to the next day template
            continue;
          }
        }
        else { // !daySlot.Day.HasValue
          IDayTemplate dayTemplate = slot.DayTemplate;
          Debug.Assert (null != dayTemplate);
          foreach (IDayTemplateItem item in dayTemplate.Items) {
            if (item.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) { // Day of week is ok
              TimeSpan correctedCutOff = item.CutOff;
              if (TimeSpan.FromHours (12) < correctedCutOff) {
                correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
              }

              Debug.Assert (TimeSpan.FromHours (-12) < correctedCutOff);
              Debug.Assert (correctedCutOff <= TimeSpan.FromHours (12));
              Debug.Assert (DateTimeKind.Utc != day.Kind);
              Debug.Assert (day.Equals (day.Date));

              DateTime dayUtcBegin = day.Add (correctedCutOff).ToUniversalTime ();
              if (slot.DateTimeRange.ContainsElement (dayUtcBegin)) {
                log.DebugFormat ("GetLiveDayBegin: " +
                                 "day begin is {0} for {1}",
                                 dayUtcBegin, day);
                return dayUtcBegin;
              }
            } // DayOfWeek
          } // Item loop
        }
      }

      log.ErrorFormat ("GetLiveDayBegin: " +
                       "there is no matching template item for day {0} " +
                       "=> the day slot could not be computed live",
                       day);
      throw new InvalidOperationException ("DaySlotDAO.GetLiveDayBegin failure");
    }

    /// <summary>
    /// Find the list of day slots in the specified range
    /// 
    /// The range must be included in the range when the day are processed
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IDaySlot> FindProcessedInRange (UtcDateTimeRange range)
    {
      if (!IsProcessedRangeValid (range)) {
        log.ErrorFormat ("FindProcessedInRange: " +
                         "specified range {0} is not valid",
                         range);
        throw new ArgumentOutOfRangeException ("range");
      }

      IList<IDaySlot> result = new List<IDaySlot> ();

      // - First day slot
      Debug.Assert (range.Lower.HasValue);
      IDaySlot firstSlot = FindProcessedAt (range.Lower.Value);
      if (null == firstSlot) {
        Debug.Assert (null != firstSlot);
        log.ErrorFormat ("FindProcessedInRange: " +
                         "no processed day slot at {0}",
                         range.Lower.Value);
        throw new InvalidOperationException ("No first day slot");
      }
      result.Add (firstSlot);
      if (firstSlot.DateTimeRange.ContainsRange (range)) {
        // We get all the slots, return
        return result;
      }

      // - Last day slot
      Debug.Assert (range.Upper.HasValue);
      DateTime endDateTime = range.Upper.Value;
      if (!range.UpperInclusive) {
        endDateTime = endDateTime.AddTicks (-1);
      }
      IDaySlot lastSlot = FindProcessedAt (endDateTime);
      if (null == lastSlot) {
        Debug.Assert (null != lastSlot);
        log.ErrorFormat ("FindProcessedInRange: " +
                         "no processed day slot at {0}",
                         endDateTime);
        throw new InvalidOperationException ("No last day slot");
      }

      // - Loop on days
      Debug.Assert (firstSlot.Day.HasValue);
      Debug.Assert (lastSlot.Day.HasValue);
      Debug.Assert (!firstSlot.Day.Value.Equals (lastSlot.Day.Value));
      for (DateTime day = firstSlot.Day.Value.AddDays (1);
           day < lastSlot.Day.Value;
           day = day.AddDays (1).Date) {
        IDaySlot daySlot = FindProcessedByDay (day);
        if (null != daySlot) {
          result.Add (daySlot);
        }
        else { // null == daySlot
          log.WarnFormat ("FindProcessedInDayRange: " +
                          "no processed day found for {0}, " +
                          "probably because the day is too old " +
                          "=> skip it",
                          day);
        }
      }
      result.Add (lastSlot);

      return result;
    }

    /// <summary>
    /// Find the list of day slots in the specified day range
    /// 
    /// The range must be included in the range when the day are processed
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IDaySlot> FindProcessedInDayRange (DayRange range)
    {
      if (!IsProcessedDayRangeValid (range)) {
        log.ErrorFormat ("FindProcessedInDayRange: " +
                         "specified range {0} is not valid",
                         range);
        throw new ArgumentOutOfRangeException ("range");
      }

      // - Loop on days
      IList<IDaySlot> result = new List<IDaySlot> ();
      DateTime day = range.Lower.Value;
      while (range.ContainsElement (day)) {
        IDaySlot daySlot = FindProcessedByDay (day);
        if (null != daySlot) {
          result.Add (daySlot);
        }
        else { // null == daySlot
          log.WarnFormat ("FindProcessedInDayRange: " +
                          "no processed day found for {0}, " +
                          "probably because the day is too old " +
                          "=> skip it",
                          day);
        }
        day = day.AddDays (1).Date;
      }
      return result;
    }

    /// <summary>
    /// Check a range to request day slots is valid
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    bool IsProcessedRangeValid (UtcDateTimeRange range)
    {
      if (!range.Lower.HasValue) {
        log.ErrorFormat ("IsProcessedRangeValid: " +
                         "no lower bound in {0}",
                         range);
        return false;
      }

      if (range.Lower.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) {
        log.ErrorFormat ("IsProcessedRangeValid: " +
                         "lower bound of {0} is before min template process date/time {1}",
                         range, AnalysisConfigHelper.MinTemplateProcessDateTime);
        return false;
      }

      if (!range.Upper.HasValue) {
        log.ErrorFormat ("IsProcessedRangeValid: " +
                         "no upper bound in {0}",
                         range);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Check a day range to request day slots is valid
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    bool IsProcessedDayRangeValid (DayRange range)
    {
      if (!range.Lower.HasValue) {
        log.ErrorFormat ("IsProcessedDayRangeValid: " +
                         "no lower bound in {0}",
                         range);
        return false;
      }

      if (range.Lower.Value < AnalysisConfigHelper.MinTemplateProcessDateTime.Date) {
        log.ErrorFormat ("IsProcessedDayRangeValid: " +
                         "lower bound of {0} is before min template process date/time {1}",
                         range, AnalysisConfigHelper.MinTemplateProcessDateTime);
        return false;
      }

      if (!range.Upper.HasValue) {
        log.ErrorFormat ("IsProcessedDayRangeValid: " +
                         "no upper bound in {0}",
                         range);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Get the system day corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public DateTime GetDay (DateTime dateTime)
    {
      if (dateTime < AnalysisConfigHelper.MinTemplateProcessDateTime) { // Default
        DateTime localFallback = dateTime.ToLocalTime ().Date;
        DateTime fallback = new DateTime (localFallback.Year, localFallback.Month, localFallback.Day); // Unspecified
        log.WarnFormat ("GetDay: " +
                        "date/time is lower than {0} " +
                        "=> fallback to {1}",
                        AnalysisConfigHelper.MinTemplateProcessDateTime,
                        fallback);
        return fallback;
      }

      switch (dateTime.Kind) {
      case DateTimeKind.Utc:
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (dateTime);
          Debug.Assert (null != daySlot);
          if (daySlot.Day.HasValue) {
            log.DebugFormat ("GetDay: " +
                             "day for {0} is {1}",
                             dateTime, daySlot.Day.Value);
            return daySlot.Day.Value;
          }
          else { // !daySlot.Day.HasValue
            Debug.Assert (daySlot.Day.HasValue); // This should not happen, the slot should be processed
            DateTime localFallback = dateTime.ToLocalTime ().Date;
            DateTime fallback = new DateTime (localFallback.Year, localFallback.Month, localFallback.Day); // Unspecified
            log.FatalFormat ("GetDay: " +
                             "day is unknown is slot {0}, " +
                             "this should not happen because the slot should be processed " +
                             "=> fallback to {1}",
                             daySlot,
                             fallback);
            return fallback;
          }
        }
      case DateTimeKind.Unspecified:
        log.WarnFormat ("GetDay: " +
                        "date/time {0} in parameter with an unspecified kind, " +
                        "consider it is a UTC date/time",
                        dateTime);
        DateTime utcDateTime = new DateTime (dateTime.Year, dateTime.Month, dateTime.Day,
                                             dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond,
                                             DateTimeKind.Utc);
        return GetDay (utcDateTime);
      case DateTimeKind.Local: // or undefined
        return GetDay (dateTime.ToUniversalTime ());
      default:
        Debug.Assert (false);
        log.FatalFormat ("GetDay: " +
                         "unknown kind {0}",
                         dateTime.Kind);
        throw new Exception ("Unknown date/time kind");
      }
    }

    /// <summary>
    /// Get the system day corresponding to a specified date/time asynchronously
    /// 
    /// The kind of the specified date/time is taken into account.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DateTime> GetDayAsync (DateTime dateTime)
    {
      if (dateTime < AnalysisConfigHelper.MinTemplateProcessDateTime) { // Default
        DateTime localFallback = dateTime.ToLocalTime ().Date;
        DateTime fallback = new DateTime (localFallback.Year, localFallback.Month, localFallback.Day); // Unspecified
        log.WarnFormat ("GetDayAsync: " +
                        "date/time is lower than {0} " +
                        "=> fallback to {1}",
                        AnalysisConfigHelper.MinTemplateProcessDateTime,
                        fallback);
        return fallback;
      }

      switch (dateTime.Kind) {
      case DateTimeKind.Utc:
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IDaySlot daySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAtAsync (dateTime);
          Debug.Assert (null != daySlot);
          if (daySlot.Day.HasValue) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetDayAsync: day for {dateTime} is {daySlot.Day.Value}");
            }
            return daySlot.Day.Value;
          }
          else { // !daySlot.Day.HasValue
            Debug.Assert (daySlot.Day.HasValue); // This should not happen, the slot should be processed
            DateTime localFallback = dateTime.ToLocalTime ().Date;
            DateTime fallback = new DateTime (localFallback.Year, localFallback.Month, localFallback.Day); // Unspecified
            log.Fatal ($"GetDayAsync: day is unknown is slot {daySlot}, this should not happen because the slot should be processed => fallback to {fallback}");
            return fallback;
          }
        }
      case DateTimeKind.Unspecified:
        log.Warn ($"GetDayAsync: date/time {dateTime} in parameter with an unspecified kind, consider it is a UTC date/time");
        DateTime utcDateTime = new DateTime (dateTime.Year, dateTime.Month, dateTime.Day,
                                             dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond,
                                             DateTimeKind.Utc);
        return GetDay (utcDateTime);
      case DateTimeKind.Local: // or undefined
        return await GetDayAsync (dateTime.ToUniversalTime ());
      default:
        Debug.Assert (false);
        log.Fatal ($"GetDay: unknown kind {dateTime.Kind}");
        throw new Exception ("Unknown date/time kind");
      }
    }

    /// <summary>
    /// Get the system day end corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// If the specified date/time is the cut-off time, the previous day is considered.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public DateTime GetEndDay (DateTime dateTime)
    {
      return GetDay (dateTime.AddTicks (-1));
    }

    /// <summary>
    /// Get the system day end corresponding to a specified date/time
    /// 
    /// The kind of the specified date/time is taken into account.
    /// 
    /// If the specified date/time is the cut-off time, the previous day is considered.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DateTime> GetEndDayAsync (DateTime dateTime)
    {
      return await GetDayAsync (dateTime.AddTicks (-1));
    }

    /// <summary>
    /// Deduce a day range from a date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public DayRange ConvertToDayRange (UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        return new DayRange ("empty");
      }

      IDaySlot beginSlot = null;
      LowerBound<DateTime> beginDay;
      UpperBound<DateTime> endDay;

      if (range.Lower.HasValue) {
        if (range.Lower.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) { // Fallback to another value
          beginDay = range.Lower.Value.ToLocalTime ().Date;
          log.WarnFormat ("ConvertToDayRange: " +
                          "begin date/time is lower than {0} " +
                          "=> fallback to {1}",
                          AnalysisConfigHelper.MinTemplateProcessDateTime,
                          beginDay);
        }
        else {
          beginSlot = FindProcessedAt (range.Lower.Value);
          Debug.Assert (null != beginSlot);
          Debug.Assert (beginSlot.Day.HasValue);
          if (beginSlot.Day.HasValue) {
            beginDay = beginSlot.Day.Value;
          }
          else { // Fallback: the day was not processed
            beginDay = range.Lower.Value.ToLocalTime ().Date;
            log.ErrorFormat ("ConvertToDayRange: " +
                             "day is unknown is slot {0} " +
                             "=> fallback to {1}",
                             beginSlot,
                             beginDay);
            beginSlot = null;
          }
        }
      }
      else {
        beginDay = new LowerBound<DateTime> (null);
      }

      if (range.Upper.HasValue) {
        if ((null != beginSlot)
            && (beginSlot.DateTimeRange.ContainsElement (range.Upper.Value)
                || (!range.UpperInclusive && !beginSlot.DateTimeRange.UpperInclusive
                    && Bound.Equals<DateTime> (range.Upper, beginSlot.DateTimeRange.Upper)))) {
          endDay = beginSlot.Day.Value;
        }
        else if (range.Upper.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) {
          endDay = range.Upper.Value.ToLocalTime ().AddDays (1).Date;
          log.WarnFormat ("ConvertToDayRange: " +
                          "begin date/time is lower than {0} " +
                          "=> fallback to {1}",
                          AnalysisConfigHelper.MinTemplateProcessDateTime,
                          endDay);
        }
        else {
          endDay = GetEndDay (range.Upper.Value);
        }
      }
      else {
        endDay = new UpperBound<DateTime> (null);
      }

      return new DayRange (beginDay, endDay);
    }

    /// <summary>
    /// Deduce a day range from a date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DayRange> ConvertToDayRangeAsync (UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        return new DayRange ("empty");
      }

      IDaySlot beginSlot = null;
      LowerBound<DateTime> beginDay;
      UpperBound<DateTime> endDay;

      if (range.Lower.HasValue) {
        if (range.Lower.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) { // Fallback to another value
          beginDay = range.Lower.Value.ToLocalTime ().Date;
          log.Warn ($"ConvertToDayRangeAsync: begin date/time is lower than {AnalysisConfigHelper.MinTemplateProcessDateTime} => fallback to {beginDay}");
        }
        else {
          beginSlot = await FindProcessedAtAsync (range.Lower.Value);
          Debug.Assert (null != beginSlot);
          Debug.Assert (beginSlot.Day.HasValue);
          if (beginSlot.Day.HasValue) {
            beginDay = beginSlot.Day.Value;
          }
          else { // Fallback: the day was not processed
            beginDay = range.Lower.Value.ToLocalTime ().Date;
            log.Error ($"ConvertToDayRangeAsync: day is unknown is slot {beginSlot} => fallback to {beginDay}");
            beginSlot = null;
          }
        }
      }
      else {
        beginDay = new LowerBound<DateTime> (null);
      }

      if (range.Upper.HasValue) {
        if ((null != beginSlot)
            && (beginSlot.DateTimeRange.ContainsElement (range.Upper.Value)
                || (!range.UpperInclusive && !beginSlot.DateTimeRange.UpperInclusive
                    && Bound.Equals<DateTime> (range.Upper, beginSlot.DateTimeRange.Upper)))) {
          endDay = beginSlot.Day.Value;
        }
        else if (range.Upper.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) {
          endDay = range.Upper.Value.ToLocalTime ().AddDays (1).Date;
          log.Warn ($"ConvertToConvertToDayRangeAsyncDayRange: begin date/time is lower than {AnalysisConfigHelper.MinTemplateProcessDateTime} => fallback to {endDay}");
        }
        else {
          endDay = await GetEndDayAsync (range.Upper.Value);
        }
      }
      else {
        endDay = new UpperBound<DateTime> (null);
      }

      return new DayRange (beginDay, endDay);
    }

    /// <summary>
    /// Convert a day to a UTC date/time range
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public UtcDateTimeRange ConvertToUtcDateTimeRange (DateTime day)
    {
      return new UtcDateTimeRange (GetDayBegin (day),
                                   GetDayEnd (day));
    }

    /// <summary>
    /// Convert a day to a UTC date/time range
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<UtcDateTimeRange> ConvertToUtcDateTimeRangeAsync (DateTime day)
    {
      var lower = await GetDayBeginAsync (day);
      var upper = await GetDayEndAsync (day);
      return new UtcDateTimeRange (lower, upper);
    }

    /// <summary>
    /// Convert a day range to a UTC date/time range
    /// </summary>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    public UtcDateTimeRange ConvertToUtcDateTimeRange (DayRange dayRange)
    {
      if (dayRange.Lower.HasValue && dayRange.Upper.HasValue && dayRange.Lower.Value.Equals (dayRange.Upper.Value)) {
        // Unique day
        var (lower, upper) = GetDayBeginEnd (dayRange.Lower.Value);
        return new UtcDateTimeRange (lower, upper);
      }
      else {
        LowerBound<DateTime> lower = new LowerBound<DateTime> (null);
        if (dayRange.Lower.HasValue) {
          lower = GetDayBegin (dayRange.Lower.Value).ToUniversalTime ();
        }

        UpperBound<DateTime> upper = new UpperBound<DateTime> (null);
        if (dayRange.Upper.HasValue) {
          upper = GetDayEnd (dayRange.Upper.Value).ToUniversalTime ();
        }

        return new UtcDateTimeRange (lower, upper);
      }
    }

    /// <summary>
    /// Convert a day range to a UTC date/time range asynchronously
    /// </summary>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<UtcDateTimeRange> ConvertToUtcDateTimeRangeAsync (DayRange dayRange)
    {
      if (dayRange.Lower.HasValue && dayRange.Upper.HasValue
        && dayRange.Lower.Value.Equals (dayRange.Upper.Value)
        && dayRange.LowerInclusive
        && dayRange.UpperInclusive) {
        // Unique day
        var (lower, upper) = await GetDayBeginEndAsync (dayRange.Lower.Value);
        return new UtcDateTimeRange (lower, upper);
      }
      else {
        LowerBound<DateTime> lower = new LowerBound<DateTime> (null);
        if (dayRange.Lower.HasValue) {
          lower = (await GetDayBeginAsync (dayRange.Lower.Value)).ToUniversalTime ();
        }

        UpperBound<DateTime> upper = new UpperBound<DateTime> (null);
        if (dayRange.Upper.HasValue) {
          upper = (await GetDayEndAsync (dayRange.Upper.Value)).ToUniversalTime ();
        }

        return new UtcDateTimeRange (lower, upper);
      }
    }

    /// <summary>
    /// Get the start and end date/times of the corresponding day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public (DateTime, DateTime) GetDayBeginEnd (DateTime day)
    {
      IDaySlot daySlot = FindProcessedByDay (day);
      if (daySlot is null) {
        log.Warn ($"GetDayBeginEnd: no processed day found for {day}, probably because the day is too old => fallback to {day.Date.ToUniversalTime ()}");
        var date = day.Date;
        return (date.ToUniversalTime (), date.AddDays (1).ToUniversalTime ());
      }
      Debug.Assert (daySlot.BeginDateTime.HasValue);
      Debug.Assert (daySlot.EndDateTime.HasValue);
      return (daySlot.BeginDateTime.Value, daySlot.EndDateTime.Value);
    }

    /// <summary>
    /// Get the start and end date/times of the corresponding day asynchronously
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<(DateTime, DateTime)> GetDayBeginEndAsync (DateTime day)
    {
      IDaySlot daySlot = await FindProcessedByDayAsync (day);
      if (daySlot is null) {
        log.Warn ($"GetDayBeginEndAsync: no processed day found for {day}, probably because the day is too old => fallback to {day.Date.ToUniversalTime ()}");
        var date = day.Date;
        return (date.ToUniversalTime (), date.AddDays (1).ToUniversalTime ());
      }
      Debug.Assert (daySlot.BeginDateTime.HasValue);
      Debug.Assert (daySlot.EndDateTime.HasValue);
      return (daySlot.BeginDateTime.Value, daySlot.EndDateTime.Value);
    }

    /// <summary>
    /// Get the begin date/time of the corresponding day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public DateTime GetDayBegin (DateTime day)
    {
      IDaySlot daySlot = FindProcessedByDay (day);
      if (null == daySlot) {
        log.WarnFormat ("GetDayBegin: " +
                        "no processed day found for {0}, " +
                        "probably because the day is too old " +
                        "=> fallback to {1}",
                        day,
                        day.Date.ToUniversalTime ());
        return day.Date.ToUniversalTime ();
      }
      Debug.Assert (daySlot.BeginDateTime.HasValue);
      return daySlot.BeginDateTime.Value;
    }

    /// <summary>
    /// Get the begin date/time of the corresponding day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DateTime> GetDayBeginAsync (DateTime day)
    {
      IDaySlot daySlot = await FindProcessedByDayAsync (day);
      if (null == daySlot) {
        log.Warn ($"GetDayBeginAsync: no processed day found for {day}, probably because the day is too old => fallback to {day.Date.ToUniversalTime ()}");
        return day.Date.ToUniversalTime ();
      }
      Debug.Assert (daySlot.BeginDateTime.HasValue);
      return daySlot.BeginDateTime.Value;
    }

    /// <summary>
    /// Get the end date/time of the corresponding day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public DateTime GetDayEnd (DateTime day)
    {
      IDaySlot daySlot = FindProcessedByDay (day);
      if (null == daySlot) {
        log.WarnFormat ("GetDayBegin: " +
                        "no processed day found for {0}, " +
                        "probably because the day is too old " +
                        "=> fallback to {1}",
                        day,
                        day.AddDays (1).Date.ToUniversalTime ());
        return day.AddDays (1).Date.ToUniversalTime ();
      }
      Debug.Assert (daySlot.EndDateTime.HasValue);
      return daySlot.EndDateTime.Value;
    }

    /// <summary>
    /// Get the end date/time of the corresponding day
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<DateTime> GetDayEndAsync (DateTime day)
    {
      IDaySlot daySlot = await FindProcessedByDayAsync (day);
      if (null == daySlot) {
        log.Warn ($"GetDayEndAsync: no processed day found for {day}, probably because the day is too old => fallback to {day.AddDays (1).Date.ToUniversalTime ()}");
        return day.AddDays (1).Date.ToUniversalTime ();
      }
      Debug.Assert (daySlot.EndDateTime.HasValue);
      return daySlot.EndDateTime.Value;
    }

    /// <summary>
    /// Return the date/time range of today
    /// </summary>
    /// <returns></returns>
    public UtcDateTimeRange GetTodayRange ()
    {
      IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
      Debug.Assert (null != daySlot);
      Debug.Assert (daySlot.Day.HasValue);
      return daySlot.DateTimeRange;
    }

    /// <summary>
    /// Process the day slots in the specified range
    /// </summary>
    /// <param name="cancellationToken"></param>
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
      if (ModelDAOHelper.DAOFactory.IsTransactionReadOnly ()) {
        log.FatalFormat ("ProcessInRange: " +
                         "the transaction is read-only " +
                         "=> the day slots can't be processed");
        throw new InvalidOperationException ("ProcessInRange in a read-only transaction");
      }

      UtcDateTimeRange effectiveRange = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (AnalysisConfigHelper.MinTemplateProcessDateTime,
                                                                                                      DateTime.UtcNow.Add (AnalysisConfigHelper.MaxDaySlotProcess))));
      if (effectiveRange.IsEmpty ()) {
        log.WarnFormat ("ProcessInRange: " +
                        "effective range is empty");
        return;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IDaySlot> daySlots = GetNotProcessTemplate (effectiveRange);
        foreach (IDaySlot daySlot in daySlots) {
          checkedThread?.SetActive ();
          ((DaySlot)daySlot).ProcessTemplate (cancellationToken, effectiveRange, checkedThread);
        }
      }
    }
  }
}
