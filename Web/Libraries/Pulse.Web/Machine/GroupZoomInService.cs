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
using Lemoine.Web;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class GroupZoomInService
    : GenericCachedService<GroupZoomInRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupZoomInService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public GroupZoomInService ()
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
    public override object GetWithoutCache (GroupZoomInRequestDTO request)
    {
      if (null == request.GroupId) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: requested group id is null");
        }
        return new ErrorDTO ("No group id is defined", ErrorStatus.WrongRequestParameter);
      }

      var businessRequest = new Lemoine.Business.Machine.GroupZoomIn (request.GroupId);
      var businessResponse = Lemoine.Business.ServiceProvider
        .Get (businessRequest);
      if (null == businessResponse) {
        log.FatalFormat ("GetWithoutCache: unexpected null business response");
        return new ErrorDTO ("Unexpected null response", ErrorStatus.UnexpectedError);
      }
      var responseDto = new GroupZoomInResponseDTO ();
      responseDto.Dynamic = businessResponse.Dynamic;
      responseDto.Children = businessResponse.Children.ToList ();
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetWithoutCache: children of {0}: {1}",
          request.GroupId, string.Join (",", businessResponse.Children.ToArray ()));
      }
      if (request.Details) {
        responseDto.ChildrenDetails = businessResponse.Children
          .Select (groupId => GetGroupDetails (groupId))
          .Where (d => null != d)
          .ToList ();
      }
      return responseDto;
    }

    GroupDTO GetGroupDetails (string groupId)
    {
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);
      if (null == group) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetGroupDetails: unknown group with id {0}", groupId);
        }
        return null;
      }
      return new GroupDTO (group);
    }
    #endregion // Methods
  }
}
