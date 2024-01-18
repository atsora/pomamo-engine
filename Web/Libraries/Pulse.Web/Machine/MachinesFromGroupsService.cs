// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Business;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Lemoine.Web;
using Pulse.Extensions.Web.Responses;
using Lemoine.Business.Machine;
using Lemoine.WebMiddleware.HttpContext;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Service to get the machines that are parts of the requested groups
  /// </summary>
  public class MachinesFromGroupsService
    : GenericCachedService<MachinesFromGroupsRequestDTO>
    , IHttpContextSupport
    , IUserSpecificResponseSupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachinesFromGroupsService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public MachinesFromGroupsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// <see cref="IUserSpecificResponseSupport"/>
    /// </summary>
    public ClaimsPrincipal SpecificUser { get; set; }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (MachinesFromGroupsRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.GroupIds)) {
        log.ErrorFormat ("GetWithoutCache: No group id was specified");
        return new ErrorDTO ("No group id specified", ErrorStatus.WrongRequestParameter);
      }

      var groupIds = request.GroupIds.Split (new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
        .Distinct ();
      if (!groupIds.Any ()) {
        log.ErrorFormat ("GetWithoutCache: No group id was specified (only a separator)");
        return new ErrorDTO ("No group id specified", ErrorStatus.WrongRequestParameter);
      }
      IList<IGroup> groups = new List<IGroup> ();
      foreach (var groupId in groupIds) {
        var groupFromId = new Lemoine.Business.Machine.GroupFromId (groupId);
        var group = ServiceProvider.Get (groupFromId);
        if (null == group) {
          log.ErrorFormat ("GetWithoutCache: no group with id {0}", groupId);
          return new ErrorDTO ("No group with id " + groupId, ErrorStatus.WrongRequestParameter);
        }
        else { // null != group
          groups.Add (group);
        }
      }

      if (!groups.Any ()) {
        Debug.Assert (false);
        log.FatalFormat ("GetWithoutCache: no group was found"); // Normally already processed above
        return new ErrorDTO ("No group found", ErrorStatus.WrongRequestParameter);
      }

      // Note: union in the same group extension, else intersection
      var groupedGroups = groups
        .GroupBy (g => g.CategoryReference);
      var sortedGroupedGroups = groupedGroups.OrderByDescending (gs => gs.Max (g => g.SortKind));
      IList<IMachine> machines = sortedGroupedGroups
        .First ()
        .OrderByDescending (g => g.SortKind)
        .SelectMany (g => g.GetMachines ())
        .Where (m => FilterMachineByMachineMonitoringType (m, request))
        .Distinct ()
        .ToList (); // Not to request twice the group
      foreach (var otherSortedGroupedGroups in sortedGroupedGroups.Skip (1)) {
        machines = machines.Intersect (otherSortedGroupedGroups
          .OrderByDescending (g => g.SortKind)
          .SelectMany (g => g.GetMachines (machines))
          .Distinct ())
          .ToList ();
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetWithoutCache: return {0} machines for groups {1}",
          machines.Count (), request.GroupIds);
      }

      // Filter machines if the user is logged or if a login is required
      AuthorizedMachinesResponse authorizedMachines;
      if (!string.IsNullOrEmpty (request.Login) && string.IsNullOrEmpty (this.HttpContext?.User?.GetUserIdString ())) {
        var authorizedMachinesRequest = new Lemoine.Business.Machine.AuthorizedMachinesFromLogin (request.Login);
        authorizedMachines = Lemoine.Business.ServiceProvider.Get (authorizedMachinesRequest);
      }
      else {
        var authorizedMachinesRequest = new Lemoine.Business.Machine.AuthorizedMachines (this.HttpContext?.User);
        authorizedMachines = Lemoine.Business.ServiceProvider.Get (authorizedMachinesRequest);
      }
      if (authorizedMachines.UserSpecific) {
        this.SpecificUser = this.HttpContext?.User;
      }
      var response = new MachinesFromGroupsResponseDTO ();
      response.MachineIds = machines
        .Where (m => authorizedMachines.IsAuthorized (m))
        .Select (m => m.Id)
        .ToList ();

      var sortedGroups = groups.Cast<IGroup> ().OrderByDescending (g => g.SortKind).Distinct ();

      if (1 == sortedGroups.Count ()) {
        response.SortKind = sortedGroups.First ().SortKind;
      }
      else {
        if (sortedGroups.Any (g => 2 <= g.SortKind)) {
          if (1 < sortedGroups.Count (g => 2 <= g.SortKind)) {
            log.WarnFormat ("GetWithoutCache: at least two different groups with a sort kind greater than 1");
            response.SortKind = null;
          }
          else {
            response.SortKind = sortedGroups.Max (g => g.SortKind);
          }
        }
        else {
          response.SortKind = sortedGroups.Max (g => g.SortKind);
        }
      }
      response.SortKindTip = GetSortKindTip (response.SortKind);

      response.Dynamic = groups.Any (g => g.Dynamic);

      if (request.MachineDetails) {
        response.MachineDetails = machines
          .Select (m => new MachineDTO (m))
          .ToList ();
      }

      return response;
    }

    bool FilterMachineByMachineMonitoringType (IMachine machine, MachinesFromGroupsRequestDTO request)
    {
      return (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored)
        || (request.IncludeNotMonitored && (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.NotMonitored))
        || (request.IncludeObsolete && (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Obsolete));
    }

    string GetSortKindTip (double? sortKind)
    {
      if (!sortKind.HasValue) {
        return "incompatible";
      }
      else {
        if (2.0 <= sortKind.Value) {
          return "sorted";
        }
        else if (1.0 <= sortKind.Value) {
          return "minor";
        }
        else {
          return "unsorted";
        }
      }
    }
  }
}
