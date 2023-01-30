// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Business.CncValue;
using Lemoine.Business;
using Lemoine.Web;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Description of CncValueColorService
  /// </summary>
  public abstract class GenericCncValueColorService<TRequestDTO>
    : GenericCachedService<TRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 960;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly ILog log = LogManager.GetLogger (typeof (CncValueColorService).FullName);

    ICncValueColorDAO m_cncValueColorDAO;

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cncValueColorDAO"></param>
    public GenericCncValueColorService (ICncValueColorDAO cncValueColorDAO)
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
      m_cncValueColorDAO = cncValueColorDAO;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Extract the range from the requestDTO
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected abstract UtcDateTimeRange GetRange (TRequestDTO requestDTO);

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, TRequestDTO requestDTO)
    {
      UtcDateTimeRange range = GetRange (requestDTO);

      // Time span considering if the range in the in the past or in the present / future
      TimeSpan timeCacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          timeCacheTimeSpan = CacheTimeOut.OldLong.GetTimeSpan ();
        }
        else { // Past
          timeCacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
        }
      }
      else { // Current or future
        timeCacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
      }

      // Time span considering the number of pixels
      Debug.Assert (range.Duration.HasValue);
      if (!range.Duration.HasValue) {
        log.FatalFormat ("GetCacheTimeOut: " +
          "no range duration, return {0} for {1}",
          timeCacheTimeSpan, url);
        return timeCacheTimeSpan;
      }
      double rangeSeconds = range.Duration.Value.TotalSeconds;
      double secondsPerPixel = rangeSeconds / 1200.0; // Considering a 1200px width screen
      TimeSpan durationCacheTimeSpan = TimeSpan.FromSeconds (2.0 * secondsPerPixel); // No need to expire the cache if you do not get at least two more pixels

      TimeSpan cacheTimeSpan = (timeCacheTimeSpan < durationCacheTimeSpan) ? durationCacheTimeSpan : timeCacheTimeSpan; // Longer duration
      log.DebugFormat ("GetCacheTimeOut: " +
                       "cacheTimeSpan is {0} considering time:{1}, considering duration:{2} for url={3}",
                       cacheTimeSpan, timeCacheTimeSpan, durationCacheTimeSpan, url);
      return cacheTimeSpan;
    }

    /// <summary>
    /// Effective work after parsing the request DTO
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="range"></param>
    /// <param name="skipDetails"></param>
    /// <returns></returns>
    protected object GetResponse (IMachineModule machineModule, IField field, UtcDateTimeRange range, bool skipDetails)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyDeferrableTransaction ("Web.CncValueColor")) {
          if (range.Duration.Value <= TimeSpan.FromDays (7)) {
            return GetAutoSplit (machineModule, field, range, skipDetails); // Less than a week
          }
          else { // More than a week
            return GetSplitByDayWithSlot (machineModule, field, range, skipDetails);
          }
        }
      }
    }

    CncValueColorResponseDTO GetAutoSplit (IMachineModule machineModule, IField field, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks (range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);

      var result = new CncValueColorResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<CncValueColorBlockDTO> ();

      var slots = m_cncValueColorDAO
        .FindOverlapsRange (machineModule, field, range, false)
        .Where (slot => !slot.DateTimeRange.IsEmpty ());

      IList<ICncValueColor> pending = new List<ICncValueColor> ();
      ICncValueColor previousSlot = null;
      foreach (var slot in slots) {
        Debug.Assert (null != slot);
        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        var newDuration = slot.Duration;
        if (newDuration.HasValue && slot.DateTimeRange.Duration.HasValue) {
          var diff = slot.DateTimeRange.Duration.Value.Subtract (slotRange.Duration.Value);
          if (TimeSpan.FromTicks (0) < diff) {
            if (diff < newDuration) {
              newDuration = newDuration.Value.Subtract (diff);
            }
            else {
              newDuration = TimeSpan.FromTicks (0);
              log.WarnFormat ("GeAutoSplit: " +
                              "new duration is 0"); // should normally not happen really often
              continue;
            }
          }
        }
        var truncatedSlot = slot.Clone (slotRange, newDuration);

        if (null != previousSlot) {
          Debug.Assert (slot.DateTimeRange.Lower.HasValue);
          Debug.Assert (previousSlot.DateTimeRange.Upper.HasValue);
          Debug.Assert (previousSlot.DateTimeRange.Upper.Value <= slot.DateTimeRange.Lower.Value);
        }
        if ((null != previousSlot)
            && (minimumSplitDuration <= slot.DateTimeRange.Lower.Value.Subtract (previousSlot.DateTimeRange.Upper.Value))) {
          // Large gap
          PushBlockAndAddToPending (result, pending, skipDetails, truncatedSlot);
        }
        else if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
          PushBlockAndAddSlot (result, pending, skipDetails, truncatedSlot);
        }
        else {
          pending.Add (truncatedSlot);
        }

        previousSlot = slot;
      } // foreach

      PushBlock (result, pending, skipDetails);

      return result;
    }

    void PushBlockAndAddToPending (CncValueColorResponseDTO result, IList<ICncValueColor> pending,
                                   bool skipDetails, ICncValueColor slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    void PushBlockAndAddSlot (CncValueColorResponseDTO result, IList<ICncValueColor> pending,
                              bool skipDetails, ICncValueColor slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
    }

    CncValueColorResponseDTO GetSplitByDayWithSlot (IMachineModule machineModule, IField field, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new CncValueColorResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<CncValueColorBlockDTO> ();

      var slots = m_cncValueColorDAO
        .FindOverlapsRange (machineModule, field, adjustedRange, false);

      IList<ICncValueColor> pending = new List<ICncValueColor> ();
      DateTime currentDay = dayRange.Lower.Value;
      int slotIndex = 0;
      while ((slotIndex < slots.Count) && (currentDay <= dayRange.Upper.Value)) {
        var slot = slots[slotIndex];
        if (currentDay < slot.DayRange.Lower.Value) {
          PushBlock (response, pending, skipDetails);
          pending.Clear ();
          currentDay = currentDay.AddDays (1);
          continue;
        }
        else if (slot.DayRange.Upper.Value < currentDay) {
          ++slotIndex;
          continue;
        }

        var currentDayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedByDay (currentDay)
          .DateTimeRange;

        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (currentDayRange));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        var newDuration = slot.Duration;
        if (newDuration.HasValue && slot.DateTimeRange.Duration.HasValue) {
          var diff = slot.DateTimeRange.Duration.Value.Subtract (slotRange.Duration.Value);
          if (TimeSpan.FromTicks (0) < diff) {
            if (diff < newDuration) {
              newDuration = newDuration.Value.Subtract (diff);
            }
            else {
              newDuration = TimeSpan.FromTicks (0);
              log.WarnFormat ("GeAutoSplit: " +
                              "new duration is 0"); // should normally not happen really often
              continue;
            }
          }
        }
        var truncatedSlot = slot.Clone (slotRange, newDuration);

        pending.Add (truncatedSlot);

        if (currentDay < slot.DayRange.Upper.Value) {
          PushBlock (response, pending, currentDay, skipDetails);
          pending.Clear ();
          currentDay = currentDay.AddDays (1);
          continue; // But keep the same slot
        }
        else { // Go to the next slot
          ++slotIndex;
          continue;
        }
      }
      PushBlock (response, pending, currentDay, skipDetails);

      return response;
    }

    void PushBlock (CncValueColorResponseDTO response, IEnumerable<ICncValueColor> slots, bool skipDetails)
    {
      PushBlock (response, slots, null, skipDetails);
    }

    void PushBlock (CncValueColorResponseDTO response, IEnumerable<ICncValueColor> slots, DateTime? day, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }

      var block = new CncValueColorBlockDTO ();
      var range = new UtcDateTimeRange (slots.First ().DateTimeRange.Lower,
                                        slots.Last ().DateTimeRange.Upper,
                                        slots.First ().DateTimeRange.LowerInclusive,
                                        slots.Last ().DateTimeRange.UpperInclusive);
      block.Range = range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (day.HasValue) {
        block.Day = ConvertDTO.DayToIsoString (day.Value);
      }
      block.Details = slots
        .GroupBy (slot => slot.Color.ToUpperInvariant (),
                  (key, keySlots) => keySlots.Aggregate (Create (key),
                                                         (dto, s) => Add (dto, s)))
        .OrderByDescending (dto => dto.Color.ToUpperInvariant ())
        .ToList ();
      var main =
        block.Details.OrderByDescending (s => s.Duration)
        .FirstOrDefault ();
      if (null != main) {
        block.Color = main.Color;
      }
      if (skipDetails) {
        block.Details = null;
      }
      response.Blocks.Add (block);
    }

    static CncValueColorBlockDetailDTO Create (string color)
    {
      var dto = new CncValueColorBlockDetailDTO ();
      dto.Color = color.ToUpperInvariant ();
      return dto;
    }

    static CncValueColorBlockDetailDTO Add (CncValueColorBlockDetailDTO dto, ICncValueColor slot)
    {
      Debug.Assert (slot.Duration.HasValue);
      dto.Duration += (int)slot.Duration.Value.TotalSeconds;
      return dto;
    }

    /// <summary>
    /// Parse a range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected UtcDateTimeRange ParseRange (string range)
    {
      if (string.IsNullOrEmpty (range)) {
        return GetDefaultRange ();
      }
      else if (range.Equals ("CurrentShift")) {
        return GetCurrentShift ();
      }
      else {
        return new UtcDateTimeRange (range);
      }
    }

    /// <summary>
    /// Return the default range: the current day
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetDefaultRange ()
    {
      var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
      var daySlot = Lemoine.Business.ServiceProvider
        .Get (dayAtRequest);
      if (daySlot is null) {
        log.Fatal ($"GetDefaultRange: no day slot now");
        return new UtcDateTimeRange ();
      }
      return daySlot.DateTimeRange;
    }

    /// <summary>
    /// Get the current shift range
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetCurrentShift ()
    {
      var currentShiftRequest = new Lemoine.Business.Shift.CurrentShift ();
      var currentShift = Lemoine.Business.ServiceProvider
        .Get (currentShiftRequest);
      if (currentShift is null) {
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider
          .Get (dayAtRequest);
        if (daySlot is null) {
          log.Fatal ($"GetCurrentShift: no day slot now");
          return new UtcDateTimeRange ();
        }
        return daySlot.DateTimeRange;
      }
      else {
        return currentShift.DateTimeRange;
      }
    }
    #endregion // Methods
  }
}
