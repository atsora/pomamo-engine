// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.CncEngine;
using Lemoine.Cnc.Asp;
using Lemoine.Core.Extensions.Web;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;

namespace Lem_CncCoreService.Asp
{
  /// <summary>
  /// CncMiddleware
  /// </summary>
  public class CheckApiKeyMiddleware
  {
    readonly ILog log = LogManager.GetLogger<CheckApiKeyMiddleware> ();

    static readonly string API_KEY_CONFIG_KEY = "Cnc.Remote.ApiKey";

    readonly RequestDelegate m_next;
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public CheckApiKeyMiddleware (RequestDelegate next, ResponseWriter responseWriter)
    {
      m_next = next;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      if (!CheckApiKey (context)) {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await m_responseWriter.WriteToBodyAsync (context, "API key not valid", "text/plain");
        return;
      }

      await m_next.Invoke (context);
    }

    bool CheckApiKey (HttpContext context)
    {
      string apiKey;
      try {
        apiKey = Lemoine.Info.ConfigSet.Get<string> (API_KEY_CONFIG_KEY);
      }
      catch (Exception ex) {
        log.Error ($"CheckApiKey: no api key is defined. Please set one in {API_KEY_CONFIG_KEY} => connection refused", ex);
        return false;
      }
      if (string.IsNullOrEmpty (apiKey)) {
        if (log.IsWarnEnabled) {
          log.Warn ($"CheckApiKey: an empty api key is defined. Please set one in {API_KEY_CONFIG_KEY}");
        }
        return true;
      }

      var apiKeyHeader = context.Request.Headers["X-API-KEY"];
      if (apiKeyHeader.Any () && string.Equals (apiKeyHeader.Single (), apiKey)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckApiKey: success");
        }
        return true;
      }

      log.Error ("CheckApiKey: missing or invalid api key");
      return false;
    }
  }
}
