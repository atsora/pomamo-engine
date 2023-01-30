// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Business;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Description of ProductionStateColorSlotsService
  /// </summary>
  public class ProductionStateColorSlotsService
    : GenericCachedService<ProductionStateColorSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly string USE_SUMMARY_KEY = "Web.ProductionStateColorSlots.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly string PRODUCTION_RATE_SUMMARY_ACTIVE_KEY = "Summary.ProductionRate.Active";
    static readonly string PRODUCTION_STATE_SUMMARY_ACTIVE_KEY = "Summary.ProductionState.Active";

    static readonly int SPLIT_BY_DAY_PARAMETER_DEFAULT = 7;

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateColorSlotsService).FullName);

    struct BlockKey
    {
      public string Color;
      public double? ProductionRate;

      public BlockKey (IProductionStateColorSlot slot)
      {
        this.Color = slot.Color.ToUpperInvariant ();
        this.ProductionRate = slot.ProductionRate;
      }

      public BlockKey (IProductionStateSummary summary)
      {
        this.Color = summary.ProductionState.Color.ToUpperInvariant ();
        this.ProductionRate = 0;
      }

      public ProductionStateColorSlotBlockDetailDTO ConvertToDto ()
      {
        var result = new ProductionStateColorSlotBlockDetailDTO ();
        result.Color = this.Color.ToUpperInvariant ();
        return result;
      }
    }

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ProductionStateColorSlotsService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, ProductionStateColorSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);

      TimeSpan timeCacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider.Get (dayAtRequest);
        if (daySlot is null) {
          timeCacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
          log.Error ($"GetCacheTimeout: no day at {DateTime.UtcNow}, fallback to {timeCacheTimeSpan}");
        }
        else if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
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
        log.Fatal ($"GetCacheTimeOut: no range duration, return {timeCacheTimeSpan} for {url}");
        return timeCacheTimeSpan;
      }
      double rangeSeconds = range.Duration.Value.TotalSeconds;
      double secondsPerPixel = rangeSeconds / 1200.0; // Considering a 1200px width screen
      TimeSpan durationCacheTimeSpan = TimeSpan.FromSeconds (secondsPerPixel); // No need to expire the cache if you do not get at least one more pixel

      TimeSpan cacheTimeSpan = (timeCacheTimeSpan < durationCacheTimeSpan) ? durationCacheTimeSpan : timeCacheTimeSpan; // Longer duration
      log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} considering time:{timeCacheTimeSpan}, considering duration:{durationCacheTimeSpan} for url={url}");
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ProductionStateColorSlotsRequestDTO request)
    {
      Debug.Assert (null != request);

      return await GetByGroupAsync (request);
    }

    async Task<object> GetByGroupAsync (ProductionStateColorSlotsRequestDTO request)
    {
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (!group.SingleMachine) {
        // TODO: ...
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not supported (multi-machines)");
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }

      var machine = group.GetMachines ().Single ();
      return await GetByMachineAsync (request, machine.Id);
    }

    async Task<object> GetByMachineAsync (ProductionStateColorSlotsRequestDTO request, int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        UtcDateTimeRange range = ParseRange (request.Range);
        if (!range.Duration.HasValue) {
          log.Error ("GetByMachine: range duration cannot be computed");
          return new ErrorDTO ("Range duration cannot be computed",
              ErrorStatus.WrongRequestParameter);
        }

        var machine = await ModelDAOHelper.DAOFactory.MachineDAO
          .FindByIdAsync (machineId);
        if (null == machine) {
          log.Error ($"GetByMachine: unknown machine with ID {machineId}");
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        IWorkOrder workOrder = null;
        if (0 < request.WorkOrderId) {
          workOrder = await ModelDAOHelper.DAOFactory.WorkOrderDAO
            .FindByIdAsync (request.WorkOrderId);
          if (null == workOrder) {
            log.Error ($"GetByMachine: unknown work order with ID {request.WorkOrderId}");
            return new ErrorDTO ("No work order with the specified ID",
              ErrorStatus.WrongRequestParameter);
          }
        }

        IComponent component = null;
        if (0 < request.ComponentId) {
          component = await ModelDAOHelper.DAOFactory.ComponentDAO
            .FindByIdAsync (request.ComponentId);
          if (null == component) {
            log.Error ($"GetByMachine: unknown component with ID {request.ComponentId}");
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
        using (var transaction = session.BeginReadOnlyDeferrableTransaction ("Web.ProductionState.ColorSlots")) {
          if (noSplitByDay) {
            return await GetAutoSplitAsync (machine, range, request.SkipDetails, workOrder, component, request.SplitDivisor); // Less than a week or work order or component
          }
          else { // !noSplitByDay
            bool useSummary = ConfigSet.LoadAndGet<bool> (USE_SUMMARY_KEY, USE_SUMMARY_DEFAULT)
              && ConfigSet.LoadAndGet (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, false)
              && ConfigSet.LoadAndGet (PRODUCTION_STATE_SUMMARY_ACTIVE_KEY, false);
            if (useSummary) {
              return await GetSplitByDayWithSummaryAsync (machine, range, request.SkipDetailsByDay);
            }
            else {
              return await GetSplitByDayWithSlotAsync (machine, range, request.SkipDetailsByDay);
            }
          }
        }
      }
    }

    async Task<ProductionStateColorSlotsResponseDTO> GetAutoSplitAsync (IMachine machine, UtcDateTimeRange range, bool skipDetails, IWorkOrder workOrder, IComponent component, int splitDivisorInRequest)
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

      var result = new ProductionStateColorSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<ProductionStateColorSlotBlockDTO> ();

      IEnumerable<IProductionStateColorSlot> slots;
      if ((null == workOrder) && (null == component)) {
        slots = await (new Lemoine.Business.ProductionState.ProductionStateColorSlotDAO ())
          .FindOverlapsRangeAsync (machine, range, false);
      }
      else {
        IEnumerable<IOperationSlot> operationSlots;
        if (null != workOrder) {
          operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeWithWorkOrder (machine, workOrder, range);
        }
        else {
          operationSlots = await ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeAsync (machine, range);
        }
        if (null != component) {
          operationSlots = operationSlots.Where (s => component.Equals (s.Component));
        }
        slots = new List<IProductionStateColorSlot> ();
        // TODO: group the ranges from operationSlots to limit the number of requests ?
        foreach (var operationSlot in operationSlots) {
          var r = new UtcDateTimeRange (operationSlot.DateTimeRange.Intersects (range));
          var newSlots = await (new Lemoine.Business.ProductionState.ProductionStateColorSlotDAO ())
            .FindOverlapsRangeAsync (machine, r, false);
          slots = slots.Concat (newSlots);
        }
      }

      IList<IProductionStateColorSlot> pending = new List<IProductionStateColorSlot> ();
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

      return result;
    }

    async Task<ProductionStateColorSlotsResponseDTO> GetSplitByDayWithSlotAsync (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = await ServiceProvider.GetAsync (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = await ServiceProvider.GetAsync (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new ProductionStateColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<ProductionStateColorSlotBlockDTO> ();

      var slots = await (new Lemoine.Business.ProductionState.ProductionStateColorSlotDAO ())
        .FindOverlapsRangeAsync (machine, adjustedRange, false);

      IList<IProductionStateColorSlot> pending = new List<IProductionStateColorSlot> ();
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

        var currentDayRange = (await ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedByDayAsync (currentDay))
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

    void PushBlock (ProductionStateColorSlotsResponseDTO response, IEnumerable<IProductionStateColorSlot> slots, bool skipDetails)
    {
      PushBlock (response, slots, null, skipDetails);
    }

    void PushBlock (ProductionStateColorSlotsResponseDTO response, IEnumerable<IProductionStateColorSlot> slots, DateTime? day, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }

      var block = new ProductionStateColorSlotBlockDTO ();
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
      if (skipDetails) {
        block.Details = null;
      }
      TimeSpan blockTotalDuration = slots
        .Aggregate (TimeSpan.FromSeconds (0),
                    (accumulate, s) => accumulate.Add (s.DateTimeRange.Duration.Value));
      var (blockAverageProductionRate, blockProductionRateDuration) = slots
        .Where (s => s.ProductionRate.HasValue && s.DateTimeRange.Duration.HasValue)
        .WeightedAverage (x => x.ProductionRate.Value, x => x.DateTimeRange.Duration.Value);
      response.Blocks.Add (block);
      response.TotalDuration += (int)blockTotalDuration.TotalSeconds;
      if (0 != blockProductionRateDuration.Ticks) {
        if (response.AverageProductionRate.HasValue) {
          var newDuration = response.ProductionRateDuration + (int)blockProductionRateDuration.TotalSeconds;
          response.AverageProductionRate = (response.ProductionRateDuration * response.AverageProductionRate.Value) / newDuration + (blockProductionRateDuration.TotalSeconds * blockAverageProductionRate) / newDuration;
          response.ProductionRateDuration = newDuration;
        }
        else {
          response.AverageProductionRate = blockAverageProductionRate;
        response.ProductionRateDuration += (int)blockProductionRateDuration.TotalSeconds;
      }
    }
    }

    static ProductionStateColorSlotBlockDetailDTO Add (ProductionStateColorSlotBlockDetailDTO dto, IProductionStateColorSlot slot)
    {
      Debug.Assert (slot.DateTimeRange.Duration.HasValue);
      dto.Duration += (int)slot.DateTimeRange.Duration.Value.TotalSeconds;
      return dto;
    }

    static ProductionStateColorSlotBlockDetailDTO Add (ProductionStateColorSlotBlockDetailDTO dto, IProductionStateSummary summary)
    {
      dto.Duration += (int)summary.Duration.TotalSeconds;
      return dto;
    }

    async Task<ProductionStateColorSlotsResponseDTO> GetSplitByDayWithSummaryAsync (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      DayRange dayRange = await ServiceProvider.GetAsync (new Lemoine.Business.Time.DayRangeFromRange (range));
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      UtcDateTimeRange adjustedRange = await ServiceProvider.GetAsync (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = new ProductionStateColorSlotsResponseDTO ();
      response.Range = adjustedRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      response.Blocks = new List<ProductionStateColorSlotBlockDTO> ();

      { // Blocks
        var summaries = await ModelDAOHelper.DAOFactory.ProductionStateSummaryDAO
          .FindInDayRangeWithProductionStateAsync (machine, dayRange);
        foreach (var groupingByDay in summaries.GroupBy (summary => summary.Day)
                 .OrderBy (g => g.Key)) {
          var block = new ProductionStateColorSlotBlockDTO ();
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
          if (skipDetails) {
            block.Details = null;
          }
          response.Blocks.Add (block);
        }
      }

      { // AverageProductionRate / ProductionRateDuration
        var productionRateSummaries = await ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
          .FindInDayRangeAsync (machine, dayRange);
        if (!productionRateSummaries.Any ()) {
          log.Error ($"GetSplitByDayWithSummary: no production rate for {machine} and day range {dayRange}");
          response.AverageProductionRate = null;
          response.ProductionRateDuration = (int)TimeSpan.FromSeconds (0).TotalSeconds;
        }
        else {
          var (rate, duration) = productionRateSummaries.WeightedAverage (x => x.ProductionRate, x => x.Duration);
          if (0 == duration.Ticks) {
            response.ProductionRateDuration = 0;
            response.AverageProductionRate = null;
          }
          else {
            response.ProductionRateDuration = (int)duration.TotalSeconds;
            response.AverageProductionRate = rate;
          }
        }

        { // Total duration
          response.TotalDuration = (int)(adjustedRange.Duration ?? TimeSpan.FromSeconds (0)).TotalSeconds;
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
