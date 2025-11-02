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

      var operationSlotDateTime = at;
      if (current) {
        var operationDetectionStatusRequest = new Lemoine.Business.Operation.OperationDetectionStatus (machine);
        var operationDetectionStatus = await Lemoine.Business.ServiceProvider.GetAsync (operationDetectionStatusRequest);
        if (operationDetectionStatus.HasValue) {
          if (operationDetectionStatus.Value < at) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GeAsync: operationDetectionStatus {operationDetectionStatus.Value} before at={at}");
            }
            operationSlotDateTime = operationDetectionStatus.Value.AddSeconds (-1);
          }
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
          var existingReports = await ModelDAOHelper.DAOFactory.ScrapReportDAO
            .FindOverlapsRange (machine, operationSlot.DateTimeRange);
          if (!existingReports.Any ()) {
            response.ExtendedRange = operationSlot.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
            if (0 == operationSlot.TotalCycles) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetAsync: no cycle in operation slot");
              }
              response.Range = "";
              response.NbCycles = 0;
              response.NbParts = 0;
              response.SetOperationSlot (operationSlot);
              return response;
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetAsync: {operationSlot.AdjustedCycles} adjusted cycles in operation slot");
              }
              var cycleRange = GetCycleRange (operationSlot);
              response.Range = cycleRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              response.NbCycles = operationSlot.TotalCycles;
              response.NbParts = operationSlot.Operation.GetTotalNumberOfIntermediateWorkPieces () * (operationSlot.TotalCycles - operationSlot.AdjustedCycles)
                + operationSlot.AdjustedQuantity;
              response.SetOperationSlot (operationSlot);
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
              response.NbCycles = 0;
              response.NbParts = 0;
              response.SetOperationSlot (operationSlot);
              return response;
            }

            var betweenRange = operationSlot.DateTimeRange;
            var before = existingReports
              .Where (x => x.DateTimeRange.Upper <= operationSlotDateTime)
              .OrderByDescending (x => x.DateTimeRange.Lower.Value)
              .FirstOrDefault ();
            if (null != before) {
              betweenRange = new UtcDateTimeRange (before.DateTimeRange.Upper.Value, betweenRange.Upper);
            }
            if (!current) {
              var after = existingReports
                .Where (x => operationSlotDateTime <= x.DateTimeRange.Lower)
                .OrderBy (x => x.DateTimeRange.Lower.Value)
                .FirstOrDefault ();
              if (null != after) {
                betweenRange = new UtcDateTimeRange (betweenRange.Lower, after.DateTimeRange.Lower.Value);
              }
            }
            response.ExtendedRange = betweenRange.ToString (ConvertDTO.DateTimeUtcToIsoString);

            var cycles = await ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindOverlapsRangeAsync (machine, betweenRange);
            if (cycles.Any ()) {
              var firstCycle = cycles.First ();
              var cyclesLower = firstCycle.Begin.HasValue ? firstCycle.Begin.Value : firstCycle.DateTime;
              var lastCycle = cycles.Last ();
              var cyclesUpper = lastCycle.End.HasValue ? lastCycle.End.Value : lastCycle.DateTime;
              var cycleRange = new UtcDateTimeRange (cyclesLower, cyclesUpper);
              response.Range = cycleRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              response.NbCycles = cycles.Count ();
              var adjustedCycles = cycles.Where (x => x.Quantity.HasValue).ToList ();
              var adjustedCount = adjustedCycles.Count ();
              response.NbParts = operationSlot.Operation.GetTotalNumberOfIntermediateWorkPieces () * (response.NbCycles - adjustedCount)
                + adjustedCycles.Sum (x => x.Quantity.Value);
              response.SetOperationSlot (operationSlot);
              return response;
            }
            else {
              log.Fatal ($"GetAsync: no cycle in {operationSlot.DateTimeRange}");
              return new ErrorDTO ("Missing cycles", ErrorStatus.UnexpectedError);
            }
          }
        }
      }
    }
  }
}
