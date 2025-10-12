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
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Web;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// ProductionRate service
  /// 
  /// Return the machine % utilization for the specified day range or date/time range
  /// 
  /// To get the utilization target, use the ProductionRateTarget service
  /// </summary>
  public class ProductionRateService
    : GenericCachedService<ProductionRateRequestDTO>
  {
    static readonly string USE_SUMMARY_KEY = "Web.ProductionRate.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly string USE_SUMMARY_MIN_DURATION_KEY = "Web.ProductionRate.UseSummaryMinDuration";
    static readonly TimeSpan USE_SUMMARY_MIN_DURATION_DEFAULT = TimeSpan.FromDays (7);

    static readonly string PRODUCTION_RATE_SUMMARY_ACTIVE_KEY = "Summary.ProductionRate.Active";

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionRateService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ProductionRateService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, ProductionRateRequestDTO requestDTO)
    {
      UtcDateTimeRange range;
      if (!string.IsNullOrEmpty (requestDTO.Range)) {
        range = ParseRange (requestDTO.Range);
      }
      else {
        range = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (ParseDayRange (requestDTO.DayRange)));
      }

      TimeSpan cacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider.Get (dayAtRequest);
        if (daySlot is null) {
          cacheTimeSpan = CacheTimeOut.CurrentLong.GetTimeSpan ();
          log.Error ($"GetCacheTimeout: no day at {DateTime.UtcNow}, fallback to {cacheTimeSpan}");
        }
        else if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldLong.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentLong.GetTimeSpan ();
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
      }
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ProductionRateRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.GroupId)) {
        log.Error ($"GetWithoutCache: no groupId in request");
        return new ErrorDTO ("Missing group", ErrorStatus.WrongRequestParameter);
      }
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);
      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetWithoutCache: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (group.SingleMachine) {
        var machine = group.GetMachines ().Single ();
        return await GetByMachineAsync (machine, request);
      }
      else {
        // TODO: ...
        if (log.IsErrorEnabled) {
          log.Error ($"GetWithoutCache: group with id {request.GroupId} is not supported (multi-machines)");
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }
    }

    async Task<object> GetByMachineAsync (IMachine machine, ProductionRateRequestDTO request)
    {
      Debug.Assert (null != machine);
      if (machine is null) {
        log.Fatal ($"GetByMachine: null machine");
        throw new ArgumentNullException ("machine");
      }

      DayRange dayRange = ParseDayRange (request.DayRange);

      if (!string.IsNullOrEmpty (request.Range)) {
        UtcDateTimeRange range = await ParseRangeAsync (request.Range);
        // Check if range can't be converted into a dayRange
        dayRange = await ServiceProvider.GetAsync (new Lemoine.Business.Time.DayRangeFromRange (range));
        var rangeFromDays = await ServiceProvider.GetAsync (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
        if (!rangeFromDays.Equals (range)) { // Conversion is no ok: consider range
          if (log.IsDebugEnabled) {
            log.Debug ($"GetWithoutCache: consider range {range}");
          }
          TimeSpan useSummaryMinDuration = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (USE_SUMMARY_MIN_DURATION_KEY,
                                   USE_SUMMARY_MIN_DURATION_DEFAULT);
          if (range.Duration.HasValue && (useSummaryMinDuration <= range.Duration.Value)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetWithoutCache: use summary because duration {range.Duration} is longer than {useSummaryMinDuration}");
            }
            return await GetByDayAsync (machine, dayRange);
          }
          else {
            return await GetByRangeAsync (machine, range);
          }
        }
      }

      return await GetByDayAsync (machine, dayRange);
    }

    async Task<ProductionRateResponseDTO> GetByDayAsync (IMachine machine, DayRange dayRange)
    {
      bool useSummary = ConfigSet.LoadAndGet<bool> (USE_SUMMARY_KEY, USE_SUMMARY_DEFAULT)
        && ConfigSet.LoadAndGet (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, false);
      if (useSummary) {
        return GetByDayWithSummary (machine, dayRange);
      }
      else {
        return await GetByDayWithSlotAsync (machine, dayRange);
      }
    }

    async Task<ProductionRateResponseDTO> GetByRangeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);

      var response = new ProductionRateResponseDTO ();
      response.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var slots = (await ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRangeAsync (machine, range))
          .Where (s => s.ProductionRate.HasValue);
        Debug.Assert (slots.All (s => s.Duration.HasValue));
        response.ProductionRate = slots
          .WeightedAverage (s => s.ProductionRate.Value, s => s.Duration.Value)
          .Item1;
      }

      return response;
    }

    async Task<ProductionRateResponseDTO> GetByDayWithSlotAsync (IMachine machine, DayRange dayRange)
    {
      UtcDateTimeRange range = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));

      var response = await GetByRangeAsync (machine, range);
      response.DayRange = dayRange.ToString (bound => ConvertDTO.DayToIsoString (bound));
      return response;
    }

    ProductionRateResponseDTO GetByDayWithSummary (IMachine machine, DayRange dayRange)
    {
      var response = new ProductionRateResponseDTO ();
      response.DayRange = dayRange.ToString (bound => ConvertDTO.DayToIsoString (bound));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        response.ProductionRate = ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
            .GetRateInDayRange (machine, dayRange);
      }
      return response;
    }

    DayRange ParseDayRange (string range)
    {
      return Task.Run (() => ParseDayRangeAsync (range)).GetAwaiter ().GetResult ();
    }

    async Task<DayRange> ParseDayRangeAsync (string range)
    {
      if (string.IsNullOrEmpty (range)) {
        IDaySlot todaySlot = await GetTodaySlotAsync ();
        Debug.Assert (todaySlot.Day.HasValue);
        return new DayRange (todaySlot.Day.Value, todaySlot.Day.Value);
      }
      else {
        return new DayRange (range);
      }
    }

    UtcDateTimeRange ParseRange (string range)
    {
      return Task.Run (() => ParseRangeAsync (range)).GetAwaiter ().GetResult ();
    }

    async Task<UtcDateTimeRange> ParseRangeAsync (string range)
    {
      Debug.Assert (!string.IsNullOrEmpty (range));
      if (range.Equals ("CurrentShift")) {
        return await GetCurrentShiftAsync ();
      }
      else {
        return new UtcDateTimeRange (range);
      }
    }

    /// <summary>
    /// Return today slot
    /// </summary>
    /// <returns></returns>
    async Task<IDaySlot> GetTodaySlotAsync ()
    {
      var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
      var daySlot = await Lemoine.Business.ServiceProvider
        .GetAsync (dayAtRequest);
      return daySlot;
    }

    /// <summary>
    /// Get the current shift range
    /// </summary>
    /// <returns></returns>
    async Task<UtcDateTimeRange> GetCurrentShiftAsync ()
    {
      var currentShiftRequest = new Lemoine.Business.Shift.CurrentShift ();
      var currentShift = await Lemoine.Business.ServiceProvider
        .GetAsync (currentShiftRequest);
      if (currentShift is null) {
        var daySlot = await GetTodaySlotAsync ();
        if (daySlot is null) {
          log.Fatal ($"GetCurrentShiftAsync: no day slot now");
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

