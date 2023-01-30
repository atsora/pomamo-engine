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
using Lemoine.Business.CncAlarm;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Business;
using Lemoine.Web;

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Description of CncAlarmColorService
  /// </summary>
  public class CncAlarmColorService
    : GenericCachedService<CncAlarmColorRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 960;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmColorService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncAlarmColorService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, CncAlarmColorRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);
      
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
    public override object GetWithoutCache(CncAlarmColorRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown monitored machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        Debug.Assert (null != machine);
        
        UtcDateTimeRange range = ParseRange (request.Range);
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncAlarmColor"))
        {
          if (range.Duration.Value <= TimeSpan.FromDays (7)) {
            return GetAutoSplit (machine, range, request.SkipDetails); // Less than a week
          }
          else { // More than a week
            return GetSplitByDayWithSlot (machine, range, request.SkipDetails);
          }
        }
      }
    }
    
    CncAlarmColorResponseDTO GetAutoSplit (IMonitoredMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);
      
      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks(range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);
      
      var result = new CncAlarmColorResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<CncAlarmColorBlockDTO> ();

      var slots = (new CncAlarmColorDAO ())
        .FindOverlapsRange (machine, range)
        .Where (slot => !slot.DateTimeRange.IsEmpty ());
      
      IList<ICncAlarmColor> pending = new List<ICncAlarmColor> ();
      ICncAlarmColor previousSlot = null;
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
        if ( (null != previousSlot)
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

    void PushBlockAndAddToPending (CncAlarmColorResponseDTO result, IList<ICncAlarmColor> pending,
                                   bool skipDetails, ICncAlarmColor slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    void PushBlockAndAddSlot (CncAlarmColorResponseDTO result, IList<ICncAlarmColor> pending,
                              bool skipDetails, ICncAlarmColor slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
    }
    
    CncAlarmColorResponseDTO GetSplitByDayWithSlot (IMonitoredMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      
      var response = new CncAlarmColorResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<CncAlarmColorBlockDTO> ();

      var slots = (new CncAlarmColorDAO ())
        .FindOverlapsRange (machine, adjustedRange);
      
      IList<ICncAlarmColor> pending = new List<ICncAlarmColor> ();
      DateTime currentDay = dayRange.Lower.Value;
      int slotIndex = 0;
      while ( (slotIndex < slots.Count) && (currentDay <= dayRange.Upper.Value)) {
        var slot = slots [slotIndex];
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

    void PushBlock (CncAlarmColorResponseDTO response, IEnumerable<ICncAlarmColor> slots, bool skipDetails)
    {
      PushBlock (response, slots, null, skipDetails);
    }
    
    void PushBlock (CncAlarmColorResponseDTO response, IEnumerable<ICncAlarmColor> slots, DateTime? day, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }
      
      var block = new CncAlarmColorBlockDTO ();
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
    
    static CncAlarmColorBlockDetailDTO Create (string color)
    {
      var dto = new CncAlarmColorBlockDetailDTO ();
      dto.Color = color.ToUpperInvariant ();
      return dto;
    }

    static CncAlarmColorBlockDetailDTO Add (CncAlarmColorBlockDetailDTO dto, ICncAlarmColor slot)
    {
      Debug.Assert (slot.Duration.HasValue);
      dto.Duration += (int) slot.Duration.Value.TotalSeconds;
      return dto;
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
