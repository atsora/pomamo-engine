// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Lemoine.WebMiddleware.HttpContext
{
  /// <summary>
  /// Extensions for HttpContext
  /// to store custom data into Items
  /// </summary>
  public static class HttpContextExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (HttpContextExtensions).FullName);

    /// <summary>
    /// Get the custom context data
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static CustomContextData GetCustomData (this Microsoft.AspNetCore.Http.HttpContext context)
    {
      return new CustomContextData (context);
    }

    /// <summary>
    /// Get the responseType parameter in URL
    /// If it is not set, an empty string is returned (default)
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetResponseContentType (this Microsoft.AspNetCore.Http.HttpContext context)
    {
      {
        var queryResponseType = context.Request.Query.ExtractResponseType ();
        if (!string.IsNullOrEmpty (queryResponseType)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetResponseContentType: {queryResponseType} from query");
          }
          return queryResponseType;
        }
      }

      {
        var customResponseType = context.GetCustomData ().ResponseContentType;
        if (!string.IsNullOrEmpty (customResponseType)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetResponseContentType: {customResponseType} from custom data");
          }
          return customResponseType;
        }
      }

      return string.Empty;
      ;
    }

    /// <summary>
    /// Extract the responseType parameter
    /// </summary>
    /// <param name="queryCollection"></param>
    /// <returns></returns>
    public static string? ExtractResponseType (this IQueryCollection queryCollection)
    {
      KeyValuePair<string, StringValues> query;
      try {
        query = queryCollection.First (q => q.Key.Equals ("responseType", StringComparison.InvariantCultureIgnoreCase));
      }
      catch (Exception) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ExtractResponseType: no responseType parameter found => use default (return null)");
        }
        return null;
      }
      var responseType = query.Value.FirstOrDefault () ?? "";
      if (log.IsDebugEnabled) {
        log.Debug ($"ExtractResponseType: response type is {responseType}");
      }
      return responseType;
    }
  }
}
