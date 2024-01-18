// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Lemoine.Web;
using Pulse.Extensions.Web.Responses;
using Lemoine.WebMiddleware.HttpContext;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Get the details on a group
  /// </summary>
  public class GroupService
    : GenericCachedService<GroupRequestDTO>
    , IHttpContextSupport
    , IUserSpecificResponseSupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupService).FullName);

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
    public GroupService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, GroupRequestDTO request)
    {
      TimeSpan cacheTimeSpan;

      if (request.MachineList) {
        cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan (); // Because of the dynamic groups
      }
      else {
        cacheTimeSpan = CacheTimeOut.Config.GetTimeSpan ();
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
    public override object GetWithoutCache (GroupRequestDTO request)
    {
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }

      var response = new GroupDTO (group);
      if (request.MachineList) {
        // Filter machines if the user is logged or if a login is required
        var businessRequest = new Lemoine.Business.Machine.AuthorizedMachines (this.HttpContext?.User);
        var authorizedMachines = Lemoine.Business.ServiceProvider.Get (businessRequest);
        if (authorizedMachines.UserSpecific) {
          this.SpecificUser = this.HttpContext?.User;
        }
        response.Machines = group
          .GetMachines ()
          .Where (m => authorizedMachines.IsAuthorized (m))
          .Select (m => new MachineDTO (m))
          .ToList ();
      }
      return response;
    }
    #endregion // Methods
  }
}
