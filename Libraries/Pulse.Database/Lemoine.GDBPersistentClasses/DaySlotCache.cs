// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Thread safe cache of DaySlot values
  /// </summary>
  public sealed class DaySlotCache
  {
    static readonly string CACHE_SIZE_KEY = "DaySlot.Cache.Size";
    static readonly int CACHE_SIZE_DEFAULT = 365 * 5; // 5 years

    #region Members
    readonly LRUDictionary<DateTime, IDaySlot> m_lru; // LRU map day -> IDaySlot
    volatile bool m_active = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DaySlotCache).FullName);

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private DaySlotCache ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ("DaySlotCache: initialization");
      }

      var cacheSize = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (CACHE_SIZE_KEY, CACHE_SIZE_DEFAULT);
      m_lru = new LRUDictionary<DateTime, IDaySlot> (cacheSize);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Is the cache active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsActive ()
    {
      return Instance.m_active;
    }

    /// <summary>
    /// Activate the cache
    /// </summary>
    /// <returns></returns>
    public static void Activate ()
    {
      Instance.m_active = true;
    }

    /// <summary>
    /// Deactivate the cache
    /// </summary>
    /// <returns>active status of the cache before</returns>
    public static bool Deactivate ()
    {
      log.Info ("Deactivate");

      if (!IsActive ()) {
        log.Info ($"Deactivate: already inactive");
        return false;
      }
      else {
        Instance.m_active = false;
        Instance.m_lru.Clear ();
        log.Info ($"Deactivate: DaySlotCache is now deactivated");
        return true;
      }
    }

    /// <summary>
    /// Set the maximum size of the cache
    /// </summary>
    /// <param name="size"></param>
    public static void SetSize (int size)
    {
      Instance.m_lru.Size = size;
    }

    /// <summary>
    /// Add a daySlot in cache
    /// </summary>
    /// <param name="daySlot"></param>
    public static void Add (IDaySlot daySlot)
    {
      if (!IsActive ()) {
        return;
      }

      if (daySlot is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Add: daySlot is null => do nothing");
        }
        return;
      }

      if (!daySlot.Day.HasValue) {
        log.Warn ($"Add: do not add day slot with range {daySlot?.DateTimeRange} because no day is associated to it");
        return;
      }

      DateTime day = daySlot.Day.Value;
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      if (DateTimeKind.Local == day.Kind) {
        day = new DateTime (day.Ticks, DateTimeKind.Unspecified);
      }

      Instance.m_lru.Add (day, daySlot);
    }

    /// <summary>
    /// Try to get a value in cache
    /// </summary>
    /// <param name="dayParameter"></param>
    /// <param name="daySlot"></param>
    public static bool TryGetValueFromDay (DateTime dayParameter, out IDaySlot daySlot)
    {
      if (IsActive ()) {
        DateTime day = dayParameter;
        Debug.Assert (DateTimeKind.Utc != day.Kind);
        if (DateTimeKind.Local == day.Kind) {
          day = new DateTime (day.Ticks, DateTimeKind.Unspecified);
        }
        bool result = Instance.m_lru.TryGetValue (day, out daySlot);
        if (!result) {
          log.Info ($"TryGetValueFromDay: day {day} was not in cache");
        }
        return result;
      }
      else {
        log.Info ($"TryGetValueFromDay: the cache is not active => return false");
        daySlot = default (DaySlot);
        return false;
      }
    }

    /// <summary>
    /// Try to get a value in cache
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="daySlot"></param>
    public static bool TryGetValueFromDateTime (DateTime dateTime, out IDaySlot daySlot)
    {
      if (!IsActive ()) {
        daySlot = default (DaySlot);
        return false;
      }

      Debug.Assert (DateTimeKind.Unspecified != dateTime.Kind);
      DateTime localDateTime = dateTime.ToLocalTime ();
      DateTime utcDateTime = dateTime.ToUniversalTime ();
      DateTime day;
      bool result;

      // 1. Try first localDateTime.Date
      day = localDateTime.Date;
      result = TryGetValueFromDay (day, out daySlot);
      if (result && daySlot.DateTimeRange.ContainsElement (utcDateTime)) {
        Debug.Assert (day.Equals (daySlot.Day.Value));
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetValueFromDateTime: return day {daySlot.Day} range {daySlot.DateTimeRange} for {dateTime}");
        }
        return true;
      }

      // 2. Try next the previous day or the next day
      if (localDateTime.TimeOfDay < TimeSpan.FromHours (12)) { // Try the previous day
        day = day.AddDays (-1).Date;
      }
      else { // Try the next day
        day = day.AddDays (+1).Date;
      }
      result = TryGetValueFromDay (day, out daySlot);
      if (result && daySlot.DateTimeRange.ContainsElement (utcDateTime)) {
        Debug.Assert (day.Equals (daySlot.Day.Value));
        if (log.IsDebugEnabled) {
          log.Debug ($"TryGetValueFromDateTime: return day {daySlot.Day} range {daySlot.DateTimeRange} for {dateTime} after considering day {day}");
        }
        return true;
      }

      if (log.IsInfoEnabled) {
        log.Info ($"TryGetValueFromDateTime: dateTime {dateTime} (local: {localDateTime}) was not in cache => return false");
      }
      daySlot = default (DaySlot);
      return false;
    }

    /// <summary>
    /// Clear the cache
    /// </summary>
    public static void Clear ()
    {
      Instance.m_lru.Clear ();
    }
    #endregion // Methods

    #region Instance
    static DaySlotCache Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly DaySlotCache instance = new DaySlotCache ();
    }
    #endregion // Instance
  }

  /// <summary>
  /// Class to suspect the activation of the DaySlotCache with the using keyword
  /// </summary>
  public sealed class DaySlotCacheSuspend : IDisposable
  {
    readonly bool m_active;

    /// <summary>
    /// Constructor
    /// </summary>
    public DaySlotCacheSuspend ()
    {
      m_active = DaySlotCache.Deactivate ();
    }

    /// <summary>
    /// <see cref="IDisposable" />
    /// </summary>
    public void Dispose ()
    {
      if (m_active) {
        DaySlotCache.Activate ();
      }
    }
  }
}
