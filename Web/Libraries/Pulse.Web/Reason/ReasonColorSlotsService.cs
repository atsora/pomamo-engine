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
using Lemoine.Extensions.Web.Responses;
using Lemoine.Business;
using Lemoine.Web;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonColorSlotsService
  /// </summary>
  public class ReasonColorSlotsService
    : GenericCachedService<ReasonColorSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly string USE_SUMMARY_KEY = "Web.ReasonColorSlots.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly int SPLIT_BY_DAY_PARAMETER_DEFAULT = 7;

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonColorSlotsService).FullName);

    struct BlockKey
    {
      public bool Processing;
      public string Color;
      public bool OverwriteRequired;
      public bool Auto;

      public BlockKey (IReasonColorSlot slot)
      {
        this.Processing = slot.Processing;
        this.Color = slot.Color.ToUpperInvariant ();
        this.OverwriteRequired = slot.OverwriteRequired;
        this.Auto = slot.Auto;
      }

      public BlockKey (IReasonSummary summary)
      {
        this.Processing = false;
        this.Color = summary.Reason.Color.ToUpperInvariant ();
        this.OverwriteRequired = false; // do not consider it
        this.Auto = false; // do not consider it
      }

      public ReasonColorSlotBlockDetailDTO ConvertToDto ()
      {
        var result = new ReasonColorSlotBlockDetailDTO ();
        result.Processing = this.Processing;
        result.Color = this.Color.ToUpperInvariant ();
        result.OverwriteRequired = this.OverwriteRequired;
        result.Auto = this.Auto;
        return result;
      }
    }

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonColorSlotsService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, ReasonColorSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);

      TimeSpan timeCacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          // Even if the reason changes, the color should not really change => Long
          timeCacheTimeSpan = CacheTimeOut.OldLong.GetTimeSpan ();
        }
        else { // Past
          // Even if the reason changes, the color should not really change most of the time
          // but for the close past, let's keep the Short option (just in case)
          timeCacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
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
    public override object GetWithoutCache (ReasonColorSlotsRequestDTO request)
    {
      Debug.Assert (null != request);

      if (0 < request.MachineId) {
        return GetByMachine (request, request.MachineId);
      }
      else {
        return GetByGroup (request);
      }
    }

    object GetByGroup (ReasonColorSlotsRequestDTO request)
    {
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: group with id {0} is not valid",
            request.GroupId);
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (!group.SingleMachine) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: group with id {0} is not supported (multi-machines)",
            request.GroupId);
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }

      var machine = group.GetMachines ().First ();
      return GetByMachine (request, machine.Id);
    }

    object GetByMachine (ReasonColorSlotsRequestDTO request, int machineId)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        UtcDateTimeRange range = ParseRange (request.Range);
        if (!range.Duration.HasValue) {
          log.ErrorFormat ("GetWithoutCache: range duration cannot be computed");
          return new ErrorDTO ("Range duration cannot be computed",
              ErrorStatus.WrongRequestParameter);
        }

        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        IWorkOrder workOrder = null;
        if (0 < request.WorkOrderId) {
          workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
            .FindById (request.WorkOrderId);
          if (null == workOrder) {
            log.ErrorFormat ("GetWithoutCache: unknown work order with ID {0}", request.WorkOrderId);
            return new ErrorDTO ("No work order with the specified ID",
              ErrorStatus.WrongRequestParameter);
          }
        }

        IComponent component = null;
        if (0 < request.ComponentId) {
          component = ModelDAOHelper.DAOFactory.ComponentDAO
            .FindById (request.ComponentId);
          if (null == component) {
            log.ErrorFormat ("GetWithoutCache: unknown component with ID {0}", request.ComponentId);
            return new ErrorDTO ("No component with the specified ID",
              ErrorStatus.WrongRequestParameter);
          }
        }

        var maxDaysNoSplitByDay = request.MaxDaysNoSplitByDay ?? SPLIT_BY_DAY_PARAMETER_DEFAULT;
        var maxDurationNoSplitByDay = TimeSpan.FromDays (maxDaysNoSplitByDay);
        var noSplitByDay =
          (null != workOrder)
          || (null != component)
          || (request.SplitByDay.HasValue && !request.SplitByDay.Value)
          || (!request.SplitByDay.HasValue && (range.Duration.Value <= maxDurationNoSplitByDay));
        using (IDAOTransaction transaction = session.BeginReadOnlyDeferrableTransaction ("Web.ReasonColorSlots")) {
          if (noSplitByDay) {
            return GetAutoSplit (machine, range, request.SkipDetails, workOrder, component, request.SplitDivisor); // Less than a week or work order or component
          }
          else { // !noSplitByDay
            bool useSummary = Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_SUMMARY_KEY,
                                                                       USE_SUMMARY_DEFAULT);
            if (useSummary) {
              return GetSplitByDayWithSummary (machine, range, request.SkipDetailsByDay);
            }
            else {
              return GetSplitByDayWithSlot (machine, range, request.SkipDetailsByDay);
            }
          }
        }
      }
    }

    ReasonColorSlotsResponseDTO GetAutoSplit (IMachine machine, UtcDateTimeRange range, bool skipDetails, IWorkOrder workOrder, IComponent component, int splitDivisorInRequest)
    {
      Debug.Assert (range.Duration.HasValue);

      int verticalSplitMaxTotalDurationFactor;
      if (0 < splitDivisorInRequest) {
        verticalSplitMaxTotalDurationFactor = splitDivisorInRequest;
      }
      else {
        verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
          .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                            VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      }
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks (range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);

      var result = new ReasonColorSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<ReasonColorSlotBlockDTO> ();

      IEnumerable<IReasonColorSlot> slots;
      if ((null == workOrder) && (null == component)) {
        slots = (new Lemoine.Business.Reason.ReasonColorSlotDAO ())
          .FindOverlapsRange (machine, range, false);
      }
      else {
        IEnumerable<IOperationSlot> operationSlots;
        if (null != workOrder) {
          operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeWithWorkOrder (machine, workOrder, range);
        }
        else {
          operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (machine, range);
        }
        if (null != component) {
          operationSlots = operationSlots.Where (s => component.Equals (s.Component));
        }
        slots = new List<IReasonColorSlot> ();
        // TODO: group the ranges from operationSlots to limit the number of requests ?
        foreach (var operationSlot in operationSlots) {
          var r = new UtcDateTimeRange (operationSlot.DateTimeRange.Intersects (range));
          var newSlots = (new Lemoine.Business.Reason.ReasonColorSlotDAO ())
            .FindOverlapsRange (machine, r, false);
          slots = slots.Concat (newSlots);
        }
      }

      IList<IReasonColorSlot> pending = new List<IReasonColorSlot> ();
      foreach (var slot in slots) {
        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        var truncatedSlot = slot.Clone (slotRange);

        if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
          PushBlock (result, pending, skipDetails);
          pending.Clear ();
          pending.Add (truncatedSlot);
          PushBlock (result, pending, skipDetails);
          pending.Clear ();
        }
        else {
          pending.Add (truncatedSlot);
        }
      }
      PushBlock (result, pending, skipDetails);

      result.Processing = result.Blocks.Any (x => x.Processing);

      return result;
    }

    ReasonColorSlotsResponseDTO GetSplitByDayWithSlot (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new ReasonColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<ReasonColorSlotBlockDTO> ();

      var slots = (new Lemoine.Business.Reason.ReasonColorSlotDAO ())
        .FindOverlapsRange (machine, adjustedRange, false);

      IList<IReasonColorSlot> pending = new List<IReasonColorSlot> ();
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
        var truncatedSlot = slot.Clone (slotRange);

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

    void PushBlock (ReasonColorSlotsResponseDTO response, IEnumerable<IReasonColorSlot> slots, bool skipDetails)
    {
      PushBlock (response, slots, null, skipDetails);
    }

    void PushBlock (ReasonColorSlotsResponseDTO response, IEnumerable<IReasonColorSlot> slots, DateTime? day, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }

      var block = new ReasonColorSlotBlockDTO ();
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
        .GroupBy (slot => new BlockKey (slot),
                  (key, keySlots) => keySlots.Aggregate (key.ConvertToDto (),
                                                         (dto, s) => Add (dto, s)))
        .OrderByDescending (dto => dto.Color.ToUpperInvariant ())
        .ToList ();
      var main =
        block.Details.OrderByDescending (s => s.Duration)
        .FirstOrDefault ();
      if (null != main) {
        block.Color = main.Color;
      }
      block.Processing = block.Details.Any (d => d.Processing);
      block.OverwriteRequired = block.Details.Any (d => d.OverwriteRequired);
      block.Auto = block.Details.Any (d => d.Auto);
      if (skipDetails) {
        block.Details = null;
      }
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
      response.Blocks.Add (block);
      response.TotalDuration += (int)blockTotalDuration.TotalSeconds;
      response.MotionDuration += (int)blockMotionDuration.TotalSeconds;
      response.NotRunningDuration += (int)blockNotRunningDuration.TotalSeconds;
    }

    static ReasonColorSlotBlockDetailDTO Add (ReasonColorSlotBlockDetailDTO dto, IReasonColorSlot slot)
    {
      Debug.Assert (slot.DateTimeRange.Duration.HasValue);
      dto.Duration += (int)slot.DateTimeRange.Duration.Value.TotalSeconds;
      return dto;
    }

    static ReasonColorSlotBlockDetailDTO Add (ReasonColorSlotBlockDetailDTO dto, IReasonSummary summary)
    {
      dto.Duration += (int)summary.Time.TotalSeconds;
      return dto;
    }

    ReasonColorSlotsResponseDTO GetSplitByDayWithSummary (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new ReasonColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<ReasonColorSlotBlockDTO> ();

      { // Blocks
        var summaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
          .FindInDayRangeWithReason (machine, dayRange);
        foreach (var groupingByDay in summaries.GroupBy (summary => summary.Day)
                 .OrderBy (g => g.Key)) {
          var block = new ReasonColorSlotBlockDTO ();
          var blockRange = ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToUtcDateTimeRange (groupingByDay.Key);
          block.Range = blockRange
            .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          block.Day = ConvertDTO.DayToIsoString (groupingByDay.Key);
          block.Details = groupingByDay
            .AsEnumerable ()
            .GroupBy (s => new BlockKey (s),
                      (key, keySlots) => keySlots.Aggregate (key.ConvertToDto (),
                                                             (dto, a) => Add (dto, a)))
            .OrderByDescending (dto => dto.Color.ToUpperInvariant ())
            .ToList ();
          var main =
            block.Details.OrderByDescending (s => s.Duration)
            .FirstOrDefault ();
          if (null != main) {
            block.Color = main.Color;
          }
          block.Processing = block.Details.Any (d => d.Processing);
          block.OverwriteRequired = block.Details.Any (d => d.OverwriteRequired);
          block.Auto = block.Details.Any (d => d.Auto);
          if (skipDetails) {
            block.Details = null;
          }
          response.Blocks.Add (block);
        }
      }

      { // TotalTime / MotionTime
        RunTotalTime? runTotalTime = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
          .GetRunTotalTime (machine, dayRange);
        if (!runTotalTime.HasValue) {
          log.ErrorFormat ("GetSplitByDayWithSummary: " +
                           "no run/total time for machine {0} day range {1}",
                           machine, dayRange);
          // Fallback: alternative solution ?
        }
        else {
          response.MotionDuration = (int)runTotalTime.Value.Run.TotalSeconds;
          response.NotRunningDuration = (int)runTotalTime.Value.NotRunning.TotalSeconds;
          response.TotalDuration = (int)runTotalTime.Value.Total.TotalSeconds;
        }
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
