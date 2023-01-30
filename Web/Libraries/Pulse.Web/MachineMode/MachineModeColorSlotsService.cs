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
  /// Description of MachineModeColorSlotsService
  /// </summary>
  public class MachineModeColorSlotsService
    : GenericCachedService<MachineModeColorSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly string USE_SUMMARY_KEY = "Web.MachineModeColorSlots.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeColorSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineModeColorSlotsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
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
    protected override TimeSpan GetCacheTimeOut (string url, MachineModeColorSlotsRequestDTO requestDTO)
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
    public override object GetWithoutCache(MachineModeColorSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
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
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.MachineModeColorSlots"))
        {
          if (range.Duration.Value <= TimeSpan.FromDays (7)) {
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
    
    MachineModeColorSlotsResponseDTO GetAutoSplit (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);
      
      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks(range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);
      
      var result = new MachineModeColorSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<MachineModeColorSlotBlockDTO> ();

      var slots = (new Lemoine.Business.MachineMode.MachineModeColorSlotDAO ())
        .FindOverlapsRange (machine, range, false);
      
      IList<IMachineModeColorSlot> pending = new List<IMachineModeColorSlot> ();
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
    
    MachineModeColorSlotsResponseDTO GetSplitByDayWithSlot (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);
      
      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      
      var response = new MachineModeColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<MachineModeColorSlotBlockDTO> ();

      var slots = (new Lemoine.Business.MachineMode.MachineModeColorSlotDAO ())
        .FindOverlapsRange (machine, adjustedRange, false);
      
      IList<IMachineModeColorSlot> pending = new List<IMachineModeColorSlot> ();
      DateTime currentDay = dayRange.Lower.Value;
      int slotIndex = 0;
      while ( (slotIndex < slots.Count) && (currentDay <= dayRange.Upper.Value)) {
        var slot = slots [slotIndex];
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

    void PushBlock (MachineModeColorSlotsResponseDTO response, IEnumerable<IMachineModeColorSlot> slots)
    {
      PushBlock (response, slots, null);
    }
    
    void PushBlock (MachineModeColorSlotsResponseDTO response, IEnumerable<IMachineModeColorSlot> slots, DateTime? day)
    {
      if (!slots.Any ()) {
        return;
      }
      
      var block = new MachineModeColorSlotBlockDTO ();
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
      TimeSpan blockTotalDuration = slots
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      TimeSpan blockMotionDuration = slots
        .Where (s => s.Running)
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      response.Blocks.Add (block);
      response.TotalDuration += (int) blockTotalDuration.TotalSeconds;
      response.MotionDuration += (int) blockMotionDuration.TotalSeconds;
    }
    
    static MachineModeColorSlotBlockDetailDTO Create (string color)
    {
      var dto = new MachineModeColorSlotBlockDetailDTO ();
      dto.Color = color.ToUpperInvariant ();
      return dto;
    }

    static MachineModeColorSlotBlockDetailDTO Add (MachineModeColorSlotBlockDetailDTO dto, IMachineModeColorSlot slot)
    {
      Debug.Assert (slot.DateTimeRange.Duration.HasValue);
      dto.Duration += (int) slot.DateTimeRange.Duration.Value.TotalSeconds;
      return dto;
    }

    static MachineModeColorSlotBlockDetailDTO Add (MachineModeColorSlotBlockDetailDTO dto, IMachineActivitySummary summary)
    {
      dto.Duration += (int) summary.Time.TotalSeconds;
      return dto;
    }

    MachineModeColorSlotsResponseDTO GetSplitByDayWithSummary (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);
      
      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      
      var response = new MachineModeColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<MachineModeColorSlotBlockDTO> ();

      { // Blocks
        var summaries = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
          .FindInDayRangeWithMachineMode (machine, dayRange);
        foreach (var groupingByDay in summaries.GroupBy (summary => summary.Day)
                 .OrderBy (g => g.Key)) {
          var block = new MachineModeColorSlotBlockDTO ();
          var blockRange = ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToUtcDateTimeRange (groupingByDay.Key);
          block.Range = blockRange
            .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          block.Day = ConvertDTO.DayToIsoString (groupingByDay.Key);
          block.Details = groupingByDay
            .AsEnumerable ()
            .GroupBy (s => s.MachineMode.Color.ToUpperInvariant (),
                      (key, keySlots) => keySlots.Aggregate (Create (key),
                                                             (dto, a) => Add (dto, a)))
            .OrderByDescending (dto => dto.Color.ToUpperInvariant ())
            .ToList ();
          var main =
            block.Details.OrderByDescending (s => s.Duration)
            .FirstOrDefault ();
          if (null != main) {
            block.Color = main.Color;
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
        response.TotalDuration += (int) blockTotalDuration.TotalSeconds;
        response.MotionDuration += (int) blockMotionDuration.TotalSeconds;
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
