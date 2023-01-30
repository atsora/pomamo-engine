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
using Lemoine.Extensions.Web.Responses;
using Lemoine.Business;
using Lemoine.Web;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Description of RunningSlotsService
  /// </summary>
  public class RunningSlotsService
    : GenericCachedService<RunningSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly string USE_SUMMARY_KEY = "Web.RunningSlots.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger (typeof (RunningSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public RunningSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, RunningSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);

      TimeSpan timeCacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          // Long because the machine mode is not changing really often in the past
          timeCacheTimeSpan = CacheTimeOut.OldLong.GetTimeSpan ();
        }
        else { // Past
          // Long because the machine mode is not changing really often in the past
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
      TimeSpan durationCacheTimeSpan = TimeSpan.FromSeconds (secondsPerPixel); // No need to expire the cache if you do not get at least one more pixel

      TimeSpan cacheTimeSpan = (timeCacheTimeSpan < durationCacheTimeSpan) ? durationCacheTimeSpan : timeCacheTimeSpan; // Longer duration
      log.DebugFormat ("GetCacheTimeOut: " +
                       "cacheTimeSpan is {0} considering time:{1}, considering duration:{2} for url={3}",
                       cacheTimeSpan, timeCacheTimeSpan, durationCacheTimeSpan, url);
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (RunningSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        UtcDateTimeRange range = ParseRange (request.Range);

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.RunningSlots")) {
          if (range.Duration.HasValue && range.Duration.Value <= TimeSpan.FromDays (7)) {
            return GetAutoSplit (machine, range); // Less than a week
          }
          else { // More than a week
            bool useSummary = Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_SUMMARY_KEY,
                                                                       USE_SUMMARY_DEFAULT);
            if (useSummary) {
              return GetSplitByDayWithSummary (machine, range);
            }
            else {
              return GetSplitByDayWithSlot (machine, range);
            }
          }
        }
      }
    }

    RunningSlotsResponseDTO GetAutoSplit (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);

      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks (range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);

      var result = new RunningSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<RunningSlotBlockDTO> ();

      var slots = (new Lemoine.Business.MachineMode.RunningSlotDAO ())
        .FindOverlapsRange (machine, range, false);

      IList<IRunningSlot> pending = new List<IRunningSlot> ();
      foreach (var slot in slots) {
        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        var truncatedSlot = slot.Clone (slotRange);

        if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
          PushBlock (result, pending);
          pending.Clear ();
          pending.Add (truncatedSlot);
          PushBlock (result, pending);
          pending.Clear ();
        }
        else {
          pending.Add (truncatedSlot);
        }
      }
      PushBlock (result, pending);

      return result;
    }

    RunningSlotsResponseDTO GetSplitByDayWithSlot (IMachine machine, UtcDateTimeRange range)
    {
      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new RunningSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<RunningSlotBlockDTO> ();

      var slots = (new Lemoine.Business.MachineMode.RunningSlotDAO ())
        .FindOverlapsRange (machine, adjustedRange, false);

      IList<IRunningSlot> pending = new List<IRunningSlot> ();
      DateTime currentDay = dayRange.Lower.Value;
      int slotIndex = 0;
      while ((slotIndex < slots.Count) && (currentDay <= dayRange.Upper.Value)) {
        var slot = slots[slotIndex];
        if (currentDay < slot.DayRange.Lower.Value) {
          PushBlock (response, pending);
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
        var truncatedSlot = slot.Clone (slotRange);

        pending.Add (truncatedSlot);

        if (currentDay < slot.DayRange.Upper.Value) {
          PushBlock (response, pending, currentDay);
          pending.Clear ();
          currentDay = currentDay.AddDays (1);
          continue; // But keep the same slot
        }
        else { // Go to the next slot
          ++slotIndex;
          continue;
        }
      }
      PushBlock (response, pending, currentDay);

      return response;
    }

    void PushBlock (RunningSlotsResponseDTO response, IEnumerable<IRunningSlot> slots)
    {
      PushBlock (response, slots, null);
    }

    void PushBlock (RunningSlotsResponseDTO response, IEnumerable<IRunningSlot> slots, DateTime? day)
    {
      if (!slots.Any ()) {
        return;
      }

      var block = new RunningSlotBlockDTO ();
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
        .GroupBy (slot => slot.NullableRunning,
                  (key, keySlots) => keySlots.Aggregate (Create (key),
                                                         (dto, s) => Add (dto, s)))
        .OrderBy (dto => (dto.Running ? 0 : (dto.NotRunning ? 1 : 2)))
        .ToList ();
      var main =
        block.Details.OrderByDescending (s => s.Duration)
        .FirstOrDefault ();
      if (null != main) {
        block.Running = main.Running;
        block.NotRunning = main.NotRunning;
      }
      response.Blocks.Add (block);
      TimeSpan blockTotalDuration = slots
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      TimeSpan blockMotionDuration = slots
        .Where (s => s.Running)
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      TimeSpan blockNotRunningDuration = slots
        .Where (s => s.NotRunning)
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      response.TotalDuration += (int)blockTotalDuration.TotalSeconds;
      response.MotionDuration += (int)blockMotionDuration.TotalSeconds;
      response.NotRunningDuration += (int)blockNotRunningDuration.TotalSeconds;
    }

    static RunningSlotBlockDetailDTO Create (bool? nullableRunning)
    {
      var dto = new RunningSlotBlockDetailDTO ();
      dto.Running = nullableRunning.HasValue && nullableRunning.Value;
      dto.NotRunning = nullableRunning.HasValue && !nullableRunning.Value;
      return dto;
    }

    static RunningSlotBlockDetailDTO Add (RunningSlotBlockDetailDTO dto, IRunningSlot slot)
    {
      Debug.Assert (slot.DateTimeRange.Duration.HasValue);
      dto.Duration += (int)slot.DateTimeRange.Duration.Value.TotalSeconds;
      return dto;
    }

    static RunningSlotBlockDetailDTO Add (RunningSlotBlockDetailDTO dto, IMachineActivitySummary summary)
    {
      dto.Duration += (int)summary.Time.TotalSeconds;
      return dto;
    }

    RunningSlotsResponseDTO GetSplitByDayWithSummary (IMachine machine, UtcDateTimeRange range)
    {
      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new RunningSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<RunningSlotBlockDTO> ();

      { // Blocks
        var summaries = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
          .FindInDayRangeWithMachineMode (machine, dayRange);
        foreach (var groupingByDay in summaries.GroupBy (summary => summary.Day)
                 .OrderBy (g => g.Key)) {
          var block = new RunningSlotBlockDTO ();
          var blockRange = ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToUtcDateTimeRange (groupingByDay.Key);
          block.Range = blockRange
            .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          block.Day = ConvertDTO.DayToIsoString (groupingByDay.Key);
          block.Details = groupingByDay
            .AsEnumerable ()
            .GroupBy (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value,
                      (key, keySlots) => keySlots.Aggregate (Create (key),
                                                             (dto, a) => Add (dto, a)))
            .OrderBy (dto => (dto.Running ? 0 : (dto.NotRunning ? 1 : 2)))
            .ToList ();
          var main =
            block.Details.OrderByDescending (s => s.Duration)
            .FirstOrDefault ();
          if (null != main) {
            block.Running = main.Running;
            block.NotRunning = main.NotRunning;
          }
          response.Blocks.Add (block);
        }
        TimeSpan blockTotalDuration = summaries
          .Aggregate (TimeSpan.FromSeconds (0),
                      (accumulate, s) => accumulate.Add (s.Time));
        TimeSpan blockMotionDuration = summaries
          .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
          .Aggregate (TimeSpan.FromSeconds (0),
                      (accumulate, s) => accumulate.Add (s.Time));
        TimeSpan blockNotRunningDuration = summaries
          .Where (s => s.MachineMode.Running.HasValue && !s.MachineMode.Running.Value)
          .Aggregate (TimeSpan.FromSeconds (0),
                      (accumulate, s) => accumulate.Add (s.Time));
        response.TotalDuration += (int)blockTotalDuration.TotalSeconds;
        response.MotionDuration += (int)blockMotionDuration.TotalSeconds;
        response.NotRunningDuration += (int)blockNotRunningDuration.TotalSeconds;
      }

      return response;
    }

    UtcDateTimeRange ParseRange (string range)
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
