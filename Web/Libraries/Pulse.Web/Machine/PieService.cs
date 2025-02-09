// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;
using Pulse.Extensions.Web;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Description of PieService
  /// </summary>
  public class PieService
    : GenericCachedService<PieRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PieService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public PieService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (PieRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.GroupId)) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: empty groupId");
        }
        return new ErrorDTO ("Empty group id", ErrorStatus.WrongRequestParameter);
      }

      var groupRequest = new Lemoine.Business.Machine.GroupFromId (request.GroupId);
      IGroup group;
      try {
        group = ServiceProvider.Get (groupRequest);
      }
      catch (Exception ex) {
        log.Error ("GetWithoutCache: exception trying to get group from an id", ex);
        throw;
      }
      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetWithoutCache: no group with id {request.GroupId}");
        }
        return new ErrorDTO ("Invalid group id", ErrorStatus.WrongRequestParameter);
      }

      var pieExtensionsRequest = new Lemoine.Business.Extension
        .GroupExtensions<IPieExtension> (group, (ext, g) => ext.Initialize (g));
      var pieExtensions = ServiceProvider.Get (pieExtensionsRequest)
        .OrderByDescending (ext => ext.Score);
      var first = pieExtensions.FirstOrDefault ();
      var response = new PieResponseDTO ();
      if (null != first) {
        response.PieType = first.PieType;
        response.Permanent = first.Permanent;
      }
      else {
        response.Permanent = true;
      }
      return response;
    }
  }
}
