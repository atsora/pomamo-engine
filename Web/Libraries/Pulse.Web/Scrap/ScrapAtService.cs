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

    /// <summary>
    /// Get the period to classify at a date/time:
    /// <item>the latest version of the scrap report that contains it, if any, with its own range:
    /// the dates of a report never change, an update creates a new report with the same range</item>
    /// <item>else the period after the previous report (or from the start of the operation slot),
    /// until the end of the last completed cycle (current: the latest cycles)</item>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at">date/time of the period (now if current)</param>
    /// <param name="current">no date/time in the request: period of the latest cycles</param>
    /// <returns></returns>
    async Task<object> GetAsync (IMonitoredMachine machine, DateTime at, bool current)
    {
      var response = new ScrapAtResponseDTO ();
      if (current) {
        response.Current = true;
      }

      var operationSlotDateTime = at;
      var recent = current || DateTime.UtcNow.AddHours (-12) <= at;
      // The cycles after the operation or cycle detection date/time may still change
      DateTime? detectionLimit = null;
      if (recent) {
        var operationDetectionStatusRequest = new Lemoine.Business.Operation.OperationDetectionStatus (machine);
        var operationDetectionStatus = await Lemoine.Business.ServiceProvider.GetAsync (operationDetectionStatusRequest);
        if (operationDetectionStatus.HasValue) {
          detectionLimit = operationDetectionStatus.Value;
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
        var cycleDetectionStatusRequest = new Lemoine.Business.Operation.CycleDetectionStatus (machine);
        var cycleDetectionStatus = await Lemoine.Business.ServiceProvider.GetAsync (cycleDetectionStatusRequest);
        if (cycleDetectionStatus.HasValue) {
          if (!detectionLimit.HasValue || (cycleDetectionStatus.Value < detectionLimit.Value)) {
            detectionLimit = cycleDetectionStatus.Value;
          }
        }
        else {
          log.Error ($"GetAsync: cycle detection status can't be retrieved");
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
          if (!operationSlot.DateTimeRange.Lower.HasValue) {
            log.Error ($"GetAsync: operation slot {operationSlot.DateTimeRange} with no start");
            return new ErrorDTO ("Operation slot with no start",
              ErrorStatus.NotApplicable);
          }
          bool currentOperationSlot = recent && (operationSlotDateTime <= operationSlot.DateTimeRange.Upper);
          response.SetOperationSlot (operationSlot, currentOperationSlot);
          // Active reports only (not replaced by a newer version), bounded
          var existingReports = (await ModelDAOHelper.DAOFactory.ScrapReportDAO
            .FindOverlapsRangeAsync (machine, operationSlot.DateTimeRange))
            .Where (x => x.DateTimeRange.Lower.HasValue && x.DateTimeRange.Upper.HasValue)
            .ToList ();

          // A report that contains At (not for the current period, the period of the latest cycles):
          // its latest version, with its own range
          if (!current) {
            var matchingReport = existingReports
              .Where (x => x.DateTimeRange.ContainsElement (at))
              .OrderByDescending (x => x.Id)
              .FirstOrDefault ();
            if (matchingReport is not null) {
              response.Current = false;
              response.Range = matchingReport.DateTimeRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
              response.ExtendedRange = response.Range;
              response.FullCycleCount = matchingReport.NbCycles;
              response.TotalCount = matchingReport.NbParts;
              response.ValidCount = matchingReport.NbValid;
              response.SetupCount = matchingReport.NbSetup;
              response.ScrapCount = matchingReport.NbScrap;
              response.FixableCount = matchingReport.NbFixable;
              response.UnclassifiedCount = matchingReport.NbParts
                - matchingReport.NbValid - matchingReport.NbSetup - matchingReport.NbScrap - matchingReport.NbFixable;
              response.ScrapReport = new ScrapReportDTO ();
              response.ScrapReport.Id = matchingReport.Id;
              response.ScrapReport.Reasons = matchingReport.Reasons
                .Select (x => new ScrapReasonDTO (x)).ToList ();
              return response;
            }
          }

          // New report: from the end of the previous report (excluded), or from the start of the operation slot
          var before = existingReports
            .Where (x => x.DateTimeRange.Upper.Value <= at)
            .OrderByDescending (x => x.DateTimeRange.Upper.Value)
            .ThenByDescending (x => x.Id)
            .FirstOrDefault ();
          var lower = before?.DateTimeRange.Upper.Value ?? operationSlot.DateTimeRange.Lower.Value;

          // ... until the start of the next report (not for the current period: no report in the future),
          // the end of the operation slot and the detection date/times
          DateTime? limit = null;
          void Restrict (DateTime? d)
          {
            if (d.HasValue && (!limit.HasValue || (d.Value < limit.Value))) {
              limit = d.Value;
            }
          }
          IScrapReport after = null;
          if (!current) {
            after = existingReports
              .Where (x => at <= x.DateTimeRange.Lower.Value)
              .OrderBy (x => x.DateTimeRange.Lower.Value)
              .FirstOrDefault ();
            Restrict (after?.DateTimeRange.Lower.Value);
          }
          var operationSlotUpper = operationSlot.DateTimeRange.Upper.NullableValue;
          Restrict (operationSlotUpper);
          Restrict (detectionLimit);
          Restrict (DateTime.UtcNow);
          response.Current = (after is null) && currentOperationSlot;

          // ... more precisely until the end of the last completed cycle: a cycle in progress is for the next report
          var cycles = (lower < limit.Value)
            ? await ScrapReportCycles.FindAsync (machine, lower, limit.Value)
            : new List<IOperationCycle> ();
          if (!cycles.Any ()) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAsync: no completed cycle to declare in ({lower},{limit}]");
            }
            // Nothing to declare. The extended range is only for the navigation
            response.Range = "";
            response.ExtendedRange = (lower < limit.Value)
              ? new UtcDateTimeRange (lower, limit.Value, "(]").ToString (ConvertDTO.DateTimeUtcToIsoString)
              : "";
            response.FullCycleCount = 0;
            response.TotalCount = 0;
            return response;
          }

          var lastCycleEnd = cycles.Last ().End.Value;
          var upper = ScrapReportCycles.CeilingToSecond (lastCycleEnd);
          // A report must be included in its operation slot, whose upper bound is excluded
          var reportRange = (operationSlotUpper.HasValue && (operationSlotUpper.Value <= upper))
            ? new UtcDateTimeRange (lower, operationSlotUpper.Value, "()")
            : new UtcDateTimeRange (lower, upper, "(]");
          response.ExtendedRange = reportRange.ToString (ConvertDTO.DateTimeUtcToIsoString);
          var firstCycle = cycles.First ();
          var cyclesLower = firstCycle.Begin.HasValue ? firstCycle.Begin.Value : firstCycle.DateTime;
          response.Range = new UtcDateTimeRange (cyclesLower, lastCycleEnd, "[]")
            .ToString (ConvertDTO.DateTimeUtcToIsoString);
          response.FullCycleCount = cycles.Count;
          response.TotalCount = ScrapReportCycles.GetNbParts (operationSlot.Operation, cycles);
          response.UnclassifiedCount = response.TotalCount;
          return response;
        }
      }
    }
  }
}
