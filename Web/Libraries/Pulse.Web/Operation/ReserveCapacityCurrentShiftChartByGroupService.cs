// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business.Operation;
using Lemoine.Collections;
using Lemoine.Conversion;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class ReserveCapacityCurrentShiftChartByGroupService
    : GenericAsyncCachedService<ReserveCapacityCurrentShiftChartByGroupRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReserveCapacityCurrentShiftChartByGroupService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReserveCapacityCurrentShiftChartByGroupService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (ReserveCapacityCurrentShiftChartByGroupRequestDTO request)
    {
      Debug.Assert (null != request);

      IEnumerable<IGroup> groups;
      if (!string.IsNullOrEmpty (request.ParentGroupId)) {
        try {
          groups = await GetGroupsAsync (request.ParentGroupId);
        }
        catch (InvalidOperationException) {
          log.FatalFormat ("Get: unexpected business response");
          return new ErrorDTO ("Unexpected response", ErrorStatus.UnexpectedError);
        }
      }
      else if (string.IsNullOrEmpty (request.GroupIds)) {
        log.Error ($"Get: no parent group {request.ParentGroupId} or individual group {request.GroupIds} was selected");
        return new ErrorDTO ("No parent or individual group in selection", ErrorStatus.WrongRequestParameter);
      }
      else { // request.GroupIds
        var groupIds = request.GroupIds.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (!groupIds.Any ()) {
          log.Error ($"Get: no parent group {request.ParentGroupId} or individual group {request.GroupIds} was selected");
          return new ErrorDTO ("No parent or individual group in selection", ErrorStatus.WrongRequestParameter);
        }
        groups = await Task.WhenAll (groupIds.Select (x => GetGroupAsync (x)));
      }

      if (groups.Any (g => g is null)) {
        if (log.IsErrorEnabled) {
          log.Error ($"Get: on of the group in {request.GroupIds} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }

      if (!groups.Any ()) {
        log.Info ($"Get: no group is part of parent group id {request.ParentGroupId}");
        return new ReserveCapacityCurrentShiftChartByGroupResponseDTO () {
          DateTime = DateTime.UtcNow,
          ChartData = new List<ReserveCapacityChartByGroupDataDto> ()
        };
      }

      IEnumerable<(IGroup, ReserveCapacityCurrentShiftResponse)> responses = await Task.WhenAll (groups
        .Select (async g => await GetReserveCapacityCurrentShiftAsync (g)));
      responses = responses.Where (x => null != x.Item2);
      if (!responses.Any ()) {
        log.Info ($"Get: no group returned a reserved capacity");
        return new ReserveCapacityCurrentShiftChartByGroupResponseDTO () {
          DateTime = DateTime.UtcNow,
          ChartData = new List<ReserveCapacityChartByGroupDataDto> ()
        };
      }

      var responseDto = new ReserveCapacityCurrentShiftChartByGroupResponseDTO () {
        DateTime = responses.Min (x => x.Item2.DateTime),
        Day = responses.UniqueOrDefault (x => x.Item2.Day)?.DayToIsoString (),
        Shift = responses.UniqueOrDefault (x => x.Item2.Shift, new EqualityComparerFromId<IShift, int> (x => x.Id))?.Pipe (x => new ShiftDTOAssembler ().Assemble (x)),
        ChartData = responses
          .Where (x => x.Item2.ReserveCapacity.HasValue)
          .Select (x => new ReserveCapacityChartByGroupDataDto () {
            GroupId = x.Item1.Id,
            GroupDisplay = x.Item1.Name,
            ReserveCapacity = x.Item2.ReserveCapacity.Value
          })
          .ToList ()
      };

      return responseDto;
    }

    async Task<IGroup> GetGroupAsync (string groupId)
    {
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);
      if (group is null) {
        log.Error ($"GetGroup: group with id {groupId} is not valid");
      }
      return group;
    }

    async Task<IEnumerable<IGroup>> GetGroupsAsync (string parentGroupId)
    {
      var businessRequest = new Lemoine.Business.Machine.GroupZoomIn (parentGroupId);
      var businessResponse = await Lemoine.Business.ServiceProvider
        .GetAsync (businessRequest);
      if (null == businessResponse) {
        log.FatalFormat ("GetWithoutCache: unexpected null business response");
        throw new InvalidOperationException ("Unexpected response");
      }
      return await Task.WhenAll (businessResponse.Children
        .Select (GetGroupAsync));
    }

    async Task<(IGroup,ReserveCapacityCurrentShiftResponse)> GetReserveCapacityCurrentShiftAsync (IGroup group)
    {
      var businessRequest = new ReserveCapacityCurrentShift (group);
      var reserveCapacityCurrentShift = await Lemoine.Business.ServiceProvider
        .GetAsync (businessRequest);
      return (group, reserveCapacityCurrentShift);
    }
    #endregion // Methods
  }
}
