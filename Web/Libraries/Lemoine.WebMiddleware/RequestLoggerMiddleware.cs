// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Cache;
using Lemoine.WebMiddleware.HttpContext;
using Lemoine.WebMiddleware.Log;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// RequestLoggerMiddleware
  /// </summary>
  public class RequestLoggerMiddleware
  {
    readonly ILog log = LogManager.GetLogger<RequestLoggerMiddleware> ();

    readonly RequestDelegate m_next;

    /// <summary>
    /// Constructor
    /// </summary>
    public RequestLoggerMiddleware (RequestDelegate next)
    {
      m_next = next;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      await using (var logRequest = new LogRequest (context)) {
        try {
          await m_next.Invoke (context);
          var customData = context.GetCustomData ();
          logRequest.CacheAction = customData.CacheAction;
          logRequest.CacheHit = customData.CacheHit;
        }
        catch (Exception ex) {
          logRequest.ThrownException = ex;
          throw;
        }
      }
    }
  }
}
