// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class PartProductionRangeService
    : GenericAsyncCachedService<PartProductionRangeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PartProductionRangeService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public PartProductionRangeService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override TimeSpan GetCacheTimeOut (string url, PartProductionRangeRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.Range)) {
        log.Error ($"GetCacheTimeOut: no range in request");
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      var range = new UtcDateTimeRange (request.Range);
      TimeSpan cacheTimeSpan;
      if (Bound.Compare<DateTime> (range.Upper, DateTime.UtcNow.AddHours (-8)) < 0) { // Past
        cacheTimeSpan = CacheTimeOut.Config.GetTimeSpan ();
      }
      else if (Bound.Compare<DateTime> (range.Upper, DateTime.UtcNow.AddHours (-1)) < 0) {
        cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
      }
      else {
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
    public override async System.Threading.Tasks.Task<object> Get (PartProductionRangeRequestDTO request)
    {
      Debug.Assert (null != request);

      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      var range = new UtcDateTimeRange (request.Range);

      if (range.IsEmpty ()) {
        log.Warn ($"Get: empty range");
      }

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"Get: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (group.SingleMachine) {
        var machine = group.GetMachines ().First ();
        return await GetByMachineAsync (machine, range);
      }
      else {
        return GetByGroup (group, range);
      }
    }
    object GetByGroup (IGroup group, UtcDateTimeRange range)
    {
      Debug.Assert (null != group);

      if (null == group.PartProductionRange) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByGroup: no production by range status available for group {group.Name}");
        }
        return new ErrorDTO ($"Production by range status is not applicable for group {group.Name}", ErrorStatus.NotApplicable);
      }

      var partProductionRange = group.PartProductionRange (range, null);

      var response = new PartProductionRangeResponseDTO ();
      response.Range = range.ToString (ConvertDTO.DateTimeUtcToIsoString);
      response.NbPieces = partProductionRange.NbPieces;
      response.Goal = partProductionRange.Goal;
      response.InProgress = partProductionRange.InProgress;

      return response;
    }

    async System.Threading.Tasks.Task<object> GetByMachineAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      var monitoredMachineFromIdRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (machine.Id);
      var monitoredMachine = await Lemoine.Business.ServiceProvider
        .GetAsync (monitoredMachineFromIdRequest);
      if (null == monitoredMachine) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByMachine: machine with id {machine.Id} is not monitored");
        }
        return new ErrorDTO ("Invalid machine", ErrorStatus.WrongRequestParameter);
      }

      return await GetByMonitoredMachineAsync (monitoredMachine, range);
    }

    async System.Threading.Tasks.Task<object> GetByMonitoredMachineAsync (IMonitoredMachine machine, UtcDateTimeRange range)
    {
      var partProductionRangeRequest = new Lemoine.Business.Operation
        .PartProductionRange (machine, range, null, null);
      var partProductionRange = await Lemoine.Business.ServiceProvider
        .GetAsync (partProductionRangeRequest);

      var response = new PartProductionRangeResponseDTO ();
      response.Range = range.ToString (ConvertDTO.DateTimeUtcToIsoString);
      response.NbPieces = partProductionRange.NbPieces;
      response.Goal = partProductionRange.Goal;
      response.InProgress = partProductionRange.InProgress;

      return response;
    }
  }
}
