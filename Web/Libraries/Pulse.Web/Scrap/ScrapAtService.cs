// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;
using System.Threading.Tasks;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Description of ReasonOnlySlotsService
  /// </summary>
  public class ScrapAtService
    : GenericCachedService<ScrapAtRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapAtService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ScrapAtService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.PastLong)
    {
    }

    DateTime ParseDateTime (string s)
    {
      var bound = ConvertDTO.IsoStringToDateTimeUtc (s);
      Debug.Assert (bound.HasValue);
      return bound.Value;
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, ScrapAtRequestDTO request)
    {
      var cacheTimeSpan = string.IsNullOrEmpty (request.At)
        ? CacheTimeOut.CurrentShort.GetTimeSpan ()
        : CacheTimeOut.PastLong.GetTimeSpan ();
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
    public override async Task<object> Get (ScrapAtRequestDTO request)
    {
      try {
        DateTime at = string.IsNullOrEmpty (request.At) ? DateTime.UtcNow : ParseDateTime (request.At);

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.Error ($"Get: unknown machine with ID {machineId}");
            return new ErrorDTO ("No machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          return await GetAsync (machine, at, string.IsNullOrEmpty (request.At));
        }
      }
      catch (Exception ex) {
        log.Error ("Get: exception", ex);
        throw;
      }
    }

    UtcDateTimeRange GetCycleRange (IOperationSlot operationSlot)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .FindAllWithOperationSlot (operationSlot);
        if (cycles.Any ()) {
          var firstCycle = cycles.First ();
          var cyclesLower = firstCycle.Begin.HasValue ? firstCycle.Begin.Value : firstCycle.DateTime;
          var lastCycle = cycles.Last ();
          var cyclesUpper = lastCycle.End.HasValue ? lastCycle.End.Value : lastCycle.DateTime;
          return new UtcDateTimeRange (cyclesLower, cyclesUpper);
        }
        else {
          log.Error ($"GetCycleRange: no range in {operationSlot.DateTimeRange}");
          return new UtcDateTimeRange ();
        }
      }
    }

    async Task<object> GetAsync (IMonitoredMachine machine, DateTime at, bool current)
    {
      var response = new ScrapAtResponseDTO ();
      if (current) {
        response.Current = true;
      }

      var operationSlotDateTime = at;
      var recent = current || DateTime.UtcNow.AddHours (-12) <= at;
      if (recent) {
        var operationDetectionStatusRequest = new Lemoine.Business.Operation.OperationDetectionStatus (machine);
        var operationDetectionStatus = await Lemoine.Business.ServiceProvider.GetAsync (operationDetectionStatusRequest);
        if (operationDetectionStatus.HasValue) {
          if (operationDetectionStatus.Value < at) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAsync: operationDetectionStatus {operationDetectionStatus.Value} before at={at}");
            }
            operationSlotDateTime = operationDetectionStatus.Value.AddSeconds (-1);
          }
        }
        else {
          log.Error ($"GetAsync: operation detection status can't be retrieved");
        }
      }
      response.At = ConvertDTO.DateTimeUtcToIsoString (operationSlotDateTime);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Web.Scrap.ScrapAt")) {
          var operationSlot = await ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindAtAsync (machine, operationSlotDateTime);
          if (operationSlot is null) {
            log.Error ($"GetAsync: no operation slot at {operationSlotDateTime}");
            return new ErrorDTO ("No operation slot at the specified time",
              ErrorStatus.NotApplicable);
          }
          if (operationSlot.Operation is null) {
            log.Error ($"GetAsync: no operation at {operationSlotDateTime}");
            return new ErrorDTO ("No operation at the specified time",
              ErrorStatus.NotApplicable);
          }
          bool currentOperationSlot = recent && (operationSlotDateTime <= operationSlot.DateTimeRange.Upper);
          var existingReports = await ModelDAOHelper.DAOFactory.ScrapReportDAO
            .FindOverlapsRange (machine, operationSlot.DateTimeRange);
          if (!existingReports.Any ()) {
            response.ExtendedRange = operationSlot.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
            response.Current = currentOperationSlot;
            response.SetOperationSlot (operationSlot, currentOperationSlot);
            if (0 == operationSlot.TotalCycles) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetAsync: no cycle in operation slot");
              }
              response.Range = "";
              response.FullCycleCount = 0;
              response.TotalCount = 0;
              return response;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetAsync: {operationSlot.AdjustedCycles} adjusted cycles in operation slot");
              }
              var cycleRange = GetCycleRange (operationSlot);
              response.Range = cycleRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              response.FullCycleCount = operationSlot.TotalCycles;
              response.TotalCount = operationSlot.Operation.GetTotalNumberOfIntermediateWorkPieces () * (operationSlot.TotalCycles - operationSlot.AdjustedCycles)
                + operationSlot.AdjustedQuantity;
              response.UnclassifiedCount = response.TotalCount;
              return response;
            }
          }
          else { // Existing reports
            // Check if the existing reports already cover all the cycles
            if (existingReports.Sum (x => x.NbCycles) >= operationSlot.TotalCycles) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetAsync: existing reports already cover all the cycles");
              }
              response.Range = "";
              response.ExtendedRange = "";
              response.FullCycleCount = 0;
              response.TotalCount = 0;
              response.SetOperationSlot (operationSlot, current);
              return response;
            }

            // Check if one of the existing report matches the time
            var matchingReport = existingReports.SingleOrDefault (x => x.DateTimeRange.ContainsElement (at));
            if (matchingReport is not null) {
              response.Range = matchingReport.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              var betweenRange = operationSlot.DateTimeRange;
              var before = existingReports
                .Where (x => x.DateTimeRange.IsStrictlyLeftOf (matchingReport.DateTimeRange))
                .OrderByDescending (x => x.DateTimeRange.Lower.Value)
                .FirstOrDefault ();
              if (null != before) {
                betweenRange = new UtcDateTimeRange (before.DateTimeRange.Upper.Value, betweenRange.Upper, "(]");
              }
              var after = existingReports
                  .Where (x => operationSlot.DateTimeRange.IsStrictlyRightOf (matchingReport.DateTimeRange))
                  .OrderBy (x => x.DateTimeRange.Lower.Value)
                  .FirstOrDefault ();
              if (null != after) {
                betweenRange = new UtcDateTimeRange (betweenRange.Lower, after.DateTimeRange.Lower.Value, "(]");
              }
              response.ExtendedRange = betweenRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              response.FullCycleCount = matchingReport.NbCycles;
              response.TotalCount = matchingReport.NbParts;
              response.ValidCount = matchingReport.NbValid;
              response.SetupCount = matchingReport.NbSetup;
              response.ScrapCount = matchingReport.NbScrap;
              response.FixableCount = matchingReport.NbFixable;
              response.UnclassifiedCount = matchingReport.NbParts
                - matchingReport.NbValid - matchingReport.NbSetup - matchingReport.NbSetup - matchingReport.NbScrap - matchingReport.NbFixable;
              response.ScrapReport = new ScrapReportDTO ();
              response.ScrapReport.Id = matchingReport.Id;
              response.ScrapReport.Reasons = matchingReport.Reasons
                .Select (x => new ScrapReasonDTO (x)).ToList ();
              response.SetOperationSlot (operationSlot, currentOperationSlot);
              return response;
            }
            else {
              var betweenRange = operationSlot.DateTimeRange;
              var before = existingReports
                .Where (x => x.DateTimeRange.Upper <= operationSlotDateTime)
                .OrderByDescending (x => x.DateTimeRange.Lower.Value)
                .FirstOrDefault ();
              if (null != before) {
                betweenRange = new UtcDateTimeRange (before.DateTimeRange.Upper.Value, betweenRange.Upper, "(]");
              }
              IScrapReport after = null;
              if (!current) {
                after = existingReports
                  .Where (x => operationSlotDateTime <= x.DateTimeRange.Lower)
                  .OrderBy (x => x.DateTimeRange.Lower.Value)
                  .FirstOrDefault ();
                if (null != after) {
                  betweenRange = new UtcDateTimeRange (betweenRange.Lower, after.DateTimeRange.Lower.Value, "(]");
                }
              }
              response.ExtendedRange = betweenRange.ToString (ConvertDTO.DateTimeUtcToIsoString);

              var cycles = await ModelDAOHelper.DAOFactory.OperationCycleDAO
                .FindOverlapsRangeAsync (machine, betweenRange);
              if (cycles.Any ()) {
                response.Current = (after is null) && currentOperationSlot;
                var firstCycle = cycles.First ();
                var cyclesLower = firstCycle.Begin.HasValue ? firstCycle.Begin.Value : firstCycle.DateTime;
                var lastCycle = cycles.Last ();
                var cyclesUpper = lastCycle.End.HasValue ? lastCycle.End.Value : lastCycle.DateTime;
                var cycleRange = new UtcDateTimeRange (cyclesLower, cyclesUpper);
                response.Range = cycleRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
                response.FullCycleCount = cycles.Count ();
                var adjustedCycles = cycles.Where (x => x.Quantity.HasValue).ToList ();
                var adjustedCount = adjustedCycles.Count ();
                response.TotalCount = operationSlot.Operation.GetTotalNumberOfIntermediateWorkPieces () * (response.FullCycleCount - adjustedCount)
                  + adjustedCycles.Sum (x => x.Quantity.Value);
                response.SetOperationSlot (operationSlot, currentOperationSlot);
                return response;
              }
              else {
                log.Warn ($"GetAsync: no cycle at {at} between two scrap reports");
                response.Range = betweenRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
                response.ExtendedRange = betweenRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
                response.FullCycleCount = 0;
                response.TotalCount = 0;
                response.SetOperationSlot (operationSlot, currentOperationSlot);
                return response;
              }
            }
          }
        }
      }
    }
  }
}
