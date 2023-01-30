// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Web.Interfaces
{
  /// <summary>
  /// To specify a <see cref="Microsoft.AspNetCore.Http.HttpContext"/> supports a request body
  /// (for example to identify the user)
  /// </summary>
  public interface IHttpContextSupport
  {
    /// <summary>
    /// Get/Set the http context
    /// </summary>
    Microsoft.AspNetCore.Http.HttpContext HttpContext { get; set; }
  }

  /// <summary>
  /// Extensions to <see cref="IHttpContextSupport"/>
  /// </summary>
  public static class HttpContextSupportExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HttpContextSupportExtensions).FullName);

    /// <summary>
    /// Get the authenticated user id
    /// </summary>
    /// <param name="httpContextSupport"></param>
    /// <returns></returns>
    public static int? GetAuthenticatedUserId (this IHttpContextSupport httpContextSupport)
    {
      var claim = httpContextSupport.HttpContext?.User;
      var userIdString = claim?.FindFirst (System.Security.Claims.ClaimTypes.PrimarySid)?.Value;
      if (string.IsNullOrEmpty (userIdString)) {
        return null;
      }
      else if (int.TryParse (userIdString, out int userId)) {
        return userId;
      }
      else {
        log.Fatal ($"GetAuthenticatedUserId: user id {userIdString} is not valid");
        return null;
      }
    }
  }
}
