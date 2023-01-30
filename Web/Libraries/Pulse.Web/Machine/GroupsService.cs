// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Business.Group;
using Lemoine.Business;
using Lemoine.Business.Machine;
using Lemoine.ModelDAO;
using Lemoine.Model;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Extensions.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Lemoine.Web;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Service to get list the available groups
  /// </summary>
  public class GroupsService : GenericCachedService<GroupsRequestDTO>
    , IHttpContextSupport
    , IUserSpecificResponseSupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupsService).FullName);

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// <see cref="IUserSpecificResponseSupport"/>
    /// </summary>
    public ClaimsPrincipal SpecificUser { get; set; }

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public GroupsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (GroupsRequestDTO request)
    {
      var response = new GroupsResponseDTO ();

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

      var groupExtensions = ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IGroupExtension> (ext => ext.Initialize ()));
      foreach (var groupExtension in groupExtensions
        .Where (ext => request.All || !ext.OmitInMachineSelection)
        .OrderBy (ext => ext.GroupCategoryName)
        .OrderBy (ext => ext.GroupCategorySortPriority)) {
        var groups = groupExtension.Groups
          .Where (g => IncludeGroupFromMachineMonitoringType (g, request));
        if (groups.Any ()) {
          var groupCategoryDTO = new GroupCategoryDTO (groupExtension.GroupCategoryName,
            groupExtension.GroupCategorySortPriority, groupExtension.OmitGroupCategory, request.Zoom, groups, m => FilterMachineByMachineMonitoringType (m, request) && authorizedMachines.IsAuthorized (m));
          response.GroupCategories.Add (groupCategoryDTO);
        }
      }

      // Group the categories with the same name and sort priority 
      response.GroupCategories = response.GroupCategories
        .GroupBy (gc => gc.Display + gc.SortPriority, (k, gcs) => GroupGroupCategories (gcs))
        .ToList ();

      if (request.MachineList) {
        // List of all machines
        response.MachineList = new List<MachineDTO> ();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machines = ModelDAOHelper.DAOFactory.MachineDAO
            .FindAll ()
            .Where (m => FilterMachineByMachineMonitoringType (m, request) && authorizedMachines.IsAuthorized (m));
          response.MachineList = machines
            .Select (m => new MachineDTO (m))
            .ToList ();
        }

        // Browse all groups
        foreach (GroupCategoryDTO gc in response.GroupCategories) {
          foreach (GroupDTO g in gc.Groups.Where (g => !g.Dynamic)) {
            // Associate machines when it's not dynamic
            var groupFromIdBusinessRequest = new Lemoine.Business.Machine.GroupFromId (g.Id);
            var group = ServiceProvider.Get (groupFromIdBusinessRequest);
            if (null == group) {
              log.Error ($"GetWithoutCache: group with id {g.Id} does not exist");
            }
            else { // null != group
              var machines = group.GetMachines ()
                .Where (m => FilterMachineByMachineMonitoringType (m, request) && authorizedMachines.IsAuthorized (m));
              g.Machines = machines
                .Select (m => new MachineDTO (m))
                .ToList ();
            }
          }

          // Keep only the groups that are dynamic or that have at least one machine
          gc.Groups = gc.Groups
            .Where (g => g.Dynamic || (g.Machines != null && g.Machines.Any ()))
            .ToList ();
        }
      }
      else if (!authorizedMachines.All) { // Static groups needs to be filtered
        foreach (GroupCategoryDTO gc in response.GroupCategories) {
          gc.Groups = gc.Groups
            .Where (g => g.Dynamic || IsStaticGroupAccessible (request, g, authorizedMachines))
            .ToList ();
        }
        response.GroupCategories = response.GroupCategories
          .Where (gc => gc.Groups.Any ())
          .ToList ();
      }

      return response;
    }

    bool IsStaticGroupAccessible (GroupsRequestDTO request, GroupDTO groupDto, AuthorizedMachinesResponse authorizedMachines)
    {
      if (authorizedMachines.All) {
        return true;
      }

      var groupFromIdBusinessRequest = new Lemoine.Business.Machine.GroupFromId (groupDto.Id);
      var group = ServiceProvider.Get (groupFromIdBusinessRequest);
      if (null == group) {
        log.Error ($"IsStaticGroupAccessible: group with id {groupDto.Id} does not exist => return false");
        return false;
      }
      else { // null != group
        return group.GetMachines ()
          .Any (m => authorizedMachines.IsAuthorized (m) && FilterMachineByMachineMonitoringType (m, request));
      }
    }

    bool FilterMachineByMachineMonitoringType (IMachine machine, GroupsRequestDTO request)
    {
      return (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Monitored)
        || (request.IncludeNotMonitored && (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.NotMonitored))
        || (request.IncludeObsolete && (machine.MonitoringType.Id == (int)MachineMonitoringTypeId.Obsolete));
    }

    bool IncludeGroupFromMachineMonitoringType (IGroup group, GroupsRequestDTO request)
    {
      return IncludeGroupFromMachineMonitoringType (group, MachineMonitoringTypeId.Monitored)
        || (request.IncludeNotMonitored && IncludeGroupFromMachineMonitoringType (group, MachineMonitoringTypeId.NotMonitored))
        || (request.IncludeObsolete && IncludeGroupFromMachineMonitoringType (group, MachineMonitoringTypeId.Obsolete));
    }

    bool IncludeGroupFromMachineMonitoringType (IGroup group, MachineMonitoringTypeId machineMonitoringTypeId)
    {
      var v = group.IncludeSpecificMonitoringType (machineMonitoringTypeId);
      return !v.HasValue || v.Value;
    }

    GroupCategoryDTO GroupGroupCategories (IEnumerable<GroupCategoryDTO> categories)
    {
      Debug.Assert (categories.Any ());

      if (1 == categories.Count ()) {
        return categories.First ();
      }
      else {
        return new GroupCategoryDTO (categories.Select (c => c.Display).First (),
          categories.Min (c => c.SortPriority),
          categories.All (c => c.OmitGroupCategory),
          categories.SelectMany (c => c.Groups));
      }
    }
    #endregion // Methods
  }
}
