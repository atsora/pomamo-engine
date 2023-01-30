// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Description of OperationCycle/At Service
  /// </summary>
  public class OperationCycleAtService
    : GenericAsyncCachedService<OperationCycleAtRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleAtService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public OperationCycleAtService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public override TimeSpan GetCacheTimeOut (string url, object request, object response)
    {
      var responseDto = response as OperationCycleAtResponseDTO;
      if (responseDto is null) {
        if (log.IsWarnEnabled) {
          log.Warn ($"GetCacheTimeout: response {response} is unexpected, because of an error ? => no cache");
        }
        return CacheTimeOut.NoCache.GetTimeSpan ();
      }
      var requestDto = (OperationCycleAtRequestDTO)request;
      var at = ConvertDTO.IsoStringToDateTimeUtc (requestDto.At).Value;
      if (string.IsNullOrEmpty (responseDto.Range)) {
        if (at < DateTime.UtcNow.Subtract (TimeSpan.FromDays (1))) {
          var cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCacheTimeOut: cacheTimeSpan is CurrentShort={cacheTimeSpan} for url={url} since there is no data in the past");
          }
          return cacheTimeSpan;
        }
        else {
          var cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCacheTimeOut: cacheTimeSpan is CurrentShort={cacheTimeSpan} for url={url} since there is no data");
          }
          return cacheTimeSpan;
        }
      }
      else { // responseDto.Range not null
        if (at < DateTime.UtcNow.Subtract (TimeSpan.FromDays (1))) {
          var cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
          }
          return cacheTimeSpan;
        }
        else {
          var cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
          }
          return cacheTimeSpan;
        }
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (OperationCycleAtRequestDTO request)
    {
      DateTime at = ConvertDTO.IsoStringToDateTimeUtc (request.At).Value;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
        if (null == machine) {
          log.Error ($"Get: unknown monitored machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationCycle.At")) {
          var operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (machine, at);
          var response = new OperationCycleAtResponseDTO ();
          if (operationCycle is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: no operation cycle at {at}");
            }
          }
          else { // null != operationCycle
            if (!operationCycle.Begin.HasValue) {
              log.Error ($"Get: FindAt returned an operation cycle {operationCycle} with no start");
              return new ErrorDTO ("Unexpected operation cycle", ErrorStatus.UnexpectedError);
            }
            if (!operationCycle.End.HasValue) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: FindAt returned an operation cycle {operationCycle} with no end");
              }
            }
            if (operationCycle.Begin.Value.Equals (operationCycle.End.Value)) {
              log.Error ($"Get: FindAt returned an operation cycle {operationCycle} with start=end");
              return new ErrorDTO ("Unexpected operation cycle", ErrorStatus.UnexpectedError);
            }
            response.Range = new UtcDateTimeRange (new LowerBound<DateTime> (operationCycle.Begin), new UpperBound<DateTime> (operationCycle.End.Value))
              .ToString (d => ConvertDTO.DateTimeUtcToIsoString (d));
            response.EstimatedStart = operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated);
            response.EstimatedEnd = operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated);
            response.DeliverablePieces = new List<CommonResponseDTO.DeliverablePieceDTO> ();
            var deliverablePieces = await ModelDAOHelper.DAOFactory.DeliverablePieceDAO
              .FindByOperationCycleAsync (operationCycle);
            response.DeliverablePieces = deliverablePieces
              .Select (x => new CommonResponseDTO.DeliverablePieceDTO (x))
              .ToList ();
          }
          return response;
        }
      }
    }


    #endregion // Methods
  }
}
