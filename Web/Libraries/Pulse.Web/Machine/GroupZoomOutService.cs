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
  public class GroupZoomOutService
    : GenericCachedService<GroupZoomOutRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GroupZoomOutService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public GroupZoomOutService ()
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
    public override object GetWithoutCache (GroupZoomOutRequestDTO request)
    {
      if (null == request.GroupId) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: requested group id is null");
        }
        return new ErrorDTO ("No group id is defined", ErrorStatus.WrongRequestParameter);
      }

      var businessRequest = new Lemoine.Business.Machine.GroupZoomOut (request.GroupId);
      var businessResponse = Lemoine.Business.ServiceProvider
        .Get (businessRequest);
      if (null == businessResponse) {
        log.FatalFormat ("GetWithoutCache: unexpected null business response");
        return new ErrorDTO ("Unexpected null response", ErrorStatus.UnexpectedError);
      }
      var responseDto = new GroupZoomOutResponseDTO ();
      responseDto.Dynamic = businessResponse.Dynamic;
      responseDto.Parent = businessResponse.Parent;
      if (log.IsDebugEnabled) {
        log.DebugFormat ("GetWithoutCache: parent of {0}: {1}",
          request.GroupId, responseDto.Parent);
      }
      return responseDto;
    }
    #endregion // Methods
  }
}
