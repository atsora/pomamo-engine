// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Lemoine.Business.Operation;
using Lemoine.Core.Cache;
using Lemoine.Collections;
using Lemoine.Conversion;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ProductionTracker
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class ProductionTrackerService
    : GenericAsyncCachedService<ProductionTrackerRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionTrackerService).FullName);

    const string PAST_LIMIT_KEY = "Plugin.ProductionTracker.PastLimit";
    static readonly TimeSpan PAST_LIMIT_DEFAULT = TimeSpan.FromDays (365);

    /// <summary>
    /// 
    /// </summary>
    public ProductionTrackerService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override TimeSpan GetCacheTimeOut (string url, ProductionTrackerRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.Range)) {
        log.Error ($"GetCacheTimeOut: range {request.Range} is empty in url={url}");
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      var range = new UtcDateTimeRange (request.Range);
      TimeSpan cacheTimeSpan;

      if (range.IsEmpty ()) {
        cacheTimeSpan = CacheTimeOut.Permanent.GetTimeSpan ();
      }
      else { // Not empty
        if (Bound.Compare<DateTime> (range.Upper, DateTime.UtcNow.AddHours (-8)) < 0) { // Past
          cacheTimeSpan = CacheTimeOut.Config.GetTimeSpan ();
        }
        else if (Bound.Compare<DateTime> (range.Upper, DateTime.UtcNow.AddHours (-1)) < 0) {
          cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
        }
        else {
          cacheTimeSpan = CacheTimeOut.CurrentLong.GetTimeSpan ();
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
      }
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request">not null</param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (ProductionTrackerRequestDTO request)
    {
      Debug.Assert (null != request);

      var groupId = request.GroupId;
      if (request.GroupId is null) {
        log.Error ($"Get: no group");
        return new ErrorDTO ("No group was specified",
                             ErrorStatus.WrongRequestParameter);
      }
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);

      var range = new UtcDateTimeRange (request.Range);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"Get: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (group.SingleMachine) {
        var machine = group.GetMachines ().Single ();
        return await GetByMachineAsync (machine, range, request.GlobalTarget, request.ProductionCapacity);
      }
      else {
        return await GetByGroupAsync (group, range, request.GlobalTarget, request.ProductionCapacity);
      }
    }

    async System.Threading.Tasks.Task<ProductionTrackerResponseDTO> GetByGroupAsync (IGroup group, UtcDateTimeRange range, bool includeGlobalTarget, bool includeProductionCapacity)
    {
      Debug.Assert (null != group);

      var localDateHourRanges = GetLocalDateHourRanges (range).ToDictionary (x => x.Item1, x => x.Item2);

      var result = new ProductionTrackerResponseDTO ();

      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));

      var aggregatingPartsMachines = group.GetAggregatingPartsMachines ();
      if (!aggregatingPartsMachines.Any ()) {
        log.Warn ($"GetByGroupAsync: no aggregating parts machine for group {group.Name}");
        return result;
      }

      var machineResponsesTasks = aggregatingPartsMachines
        .Select (m => GetByMachineAsync (m, range, includeGlobalTarget, includeProductionCapacity, localDateHourRanges));
      var machineResponses = await Task.WhenAll (machineResponsesTasks);

      if (machineResponses.Any (x => x.GlobalTarget.HasValue)) {
        result.GlobalTarget = machineResponses.Sum (x => x?.GlobalTarget ?? 0.0);
      }
      if (machineResponses.Any (x => x.ProductionCapacity.HasValue)) {
        result.ProductionCapacity = machineResponses.Sum (x => x?.ProductionCapacity ?? 0.0);
      }

      foreach (var localDateHourRange in localDateHourRanges.OrderBy (x => x.Key)) {
        var localHour = localDateHourRange.Key.ToLocalIsoString ();
        var machineDtos = machineResponses
          .SelectMany (x => x.HourlyData)
          .Where (x => string.Equals (localHour, x.LocalHour));

        var perHourData = new ProductionTrackerResponsePerHourDTO ();
        perHourData.LocalHour = localHour;

        if (machineDtos.Any ()) {
          var hourRange = machineDtos
            .Select (x => new UtcDateTimeRange (x.Range))
            .Aggregate (new UtcDateTimeRange (), (x, y) => new UtcDateTimeRange (x.Union (y)));
          perHourData.Range = hourRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          perHourData.Actual = machineDtos
            .Sum (x => x.Actual);
          if (machineDtos.Any (x => x.Target.HasValue)) {
            perHourData.Target = machineDtos
              .Sum (x => x?.Target ?? 0.0);
          }
          perHourData.Static = machineDtos.All (x => x.Static);
        }

        result.HourlyData.Add (perHourData);
      }

      return result;
    }

    async System.Threading.Tasks.Task<ProductionTrackerResponseDTO> GetByMachineAsync (IMachine machine, UtcDateTimeRange range, bool includeGlobalTarget, bool includeProductionCapacity)
    {
      Debug.Assert (null != machine);

      var localDateHourRanges = GetLocalDateHourRanges (range).ToDictionary (x => x.Item1, x => x.Item2);

      return await GetByMachineAsync (machine, range, includeGlobalTarget, includeProductionCapacity, localDateHourRanges);
    }

    async System.Threading.Tasks.Task<ProductionTrackerResponseDTO> GetByMachineAsync (IMachine machine, UtcDateTimeRange range, bool includeGlobalTarget, bool includeProductionCapacity, IDictionary<DateTime, UtcDateTimeRange> localDateHourRanges)
    {
      var result = new ProductionTrackerResponseDTO ();

      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));

      var partProductionRangesTasks = localDateHourRanges
        .Select (async x => (x.Key, await GetPartProductionRangeAsync (machine, x.Value, range)));
      var partProductionRanges = (await Task.WhenAll (partProductionRangesTasks))
        .ToDictionary (x => x.Item1, x => x.Item2);

      IOperation operation;
      if (includeProductionCapacity || includeGlobalTarget) {
        operation = partProductionRanges
          .SelectMany (x => x.Value.Operations)
          .UniqueOrDefault (new EqualityComparerDataWithId<IOperation> ());
      }
      else {
        operation = null;
      }

      double? productionCapacityPerHour = null;
      if (null != operation) {
        var productionDuration = Lemoine.Business.MachineObservationState.Production.GetProductionDuration (machine, range);

        if (includeGlobalTarget) {
          if (0 < productionDuration.TotalSeconds) {
            var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation.StandardProductionTargetPerHour (machine, operation);
            var standardProductionTargetPerHour = await Lemoine.Business.ServiceProvider
              .GetAsync (standardProductionTargetPerHourRequest);
            if (standardProductionTargetPerHour.HasValue) {
              result.GlobalTarget = productionDuration.TotalHours * standardProductionTargetPerHour;
            }
          }
          else {
            result.GlobalTarget = 0.0;
          }
        }

        if (includeProductionCapacity) {
          if (0 < productionDuration.TotalSeconds) {
            var productionCapacityPerHourRequest = new Lemoine.Business.Operation
              .ProductionCapacityPerHour (machine, operation);
            productionCapacityPerHour = await Lemoine.Business.ServiceProvider
              .GetAsync (productionCapacityPerHourRequest);
            if (productionCapacityPerHour.HasValue) {
              result.ProductionCapacity = productionDuration.TotalHours * productionCapacityPerHour;
            }
          }
          else {
            result.ProductionCapacity = 0.0;
          }
        }
      } // null != operation

      foreach (var localDateHourRange in localDateHourRanges.OrderBy (x => x.Key)) {
        var partProductionRange = partProductionRanges[localDateHourRange.Key];
        var hourlyDataDto = GetPerHourData (machine, localDateHourRange.Key, localDateHourRange.Value, partProductionRange);
        if ((null != operation) && includeProductionCapacity && productionCapacityPerHour.HasValue) {
          var productionDuration = Lemoine.Business.MachineObservationState.Production.GetProductionDuration (machine, localDateHourRange.Value, range);
          hourlyDataDto.ProductionCapacity = productionCapacityPerHour.Value * productionDuration.TotalHours;
        }
        result.HourlyData.Add (hourlyDataDto);
      }

      return result;
    }

    IEnumerable<(DateTime, UtcDateTimeRange)> GetLocalDateHourRanges (UtcDateTimeRange range)
    {
      var now = DateTime.UtcNow;
      var pastLimit = Lemoine.Info.ConfigSet.LoadAndGet (PAST_LIMIT_KEY, PAST_LIMIT_DEFAULT);
      var lowerBound = range.Lower.HasValue
        ? range.Lower.Value
        : now.Subtract (pastLimit);
      var upperBound = Bound.Compare<DateTime> (range.Upper, now) < 0
        ? range.Upper.Value
        : now; // No data in the past

      var minLocalDateHour = ConvertToLocalDateHour (lowerBound);
      var maxLocalDateHour = ConvertToLocalDateHour (upperBound);
      var localDateHour = minLocalDateHour;
      IDictionary<DateTime, PartProductionRange> partProductionRanges = new Dictionary<DateTime, PartProductionRange> ();
      while (localDateHour <= maxLocalDateHour) {
        var nextLocalDateHour = localDateHour.AddHours (1);
        var perHourRange = new UtcDateTimeRange (new UtcDateTimeRange (localDateHour.ToUniversalTime (), nextLocalDateHour.ToUniversalTime ()).Intersects (range));
        if (!perHourRange.IsEmpty ()) {
          yield return (localDateHour, perHourRange);
        }
        localDateHour = nextLocalDateHour;
      }
    }

    DateTime ConvertToLocalDateHour (DateTime utcDateTime)
    {
      var localDateTime = utcDateTime.ToLocalTime ();
      return new DateTime (localDateTime.Year, localDateTime.Month, localDateTime.Day,
        localDateTime.Hour, 00, 00, DateTimeKind.Local);
    }

    async Task<PartProductionRangeResponse> GetPartProductionRangeAsync (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange)
    {
      var partProductionRangeRequest = new Lemoine.Business.Operation
        .PartProductionRange (machine, range, preLoadRange);
      var partProductionRange = await Lemoine.Business.ServiceProvider
        .GetAsync (partProductionRangeRequest);
      return partProductionRange;
    }

    ProductionTrackerResponsePerHourDTO GetPerHourData (IMachine machine, DateTime localDateHour, UtcDateTimeRange range, PartProductionRangeResponse partProductionRange)
    {
      var perHourData = new ProductionTrackerResponsePerHourDTO ();
      perHourData.LocalHour = localDateHour.ToLocalIsoString ();
      perHourData.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      perHourData.Actual = partProductionRange.NbPieces;
      perHourData.Target = partProductionRange.Goal;
      perHourData.Static = !partProductionRange.InProgress;

      return perHourData;
    }
  }
}
