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
  public class CreateApiKeyMiddleware
  {
    readonly ILog log = LogManager.GetLogger<CreateApiKeyMiddleware> ();

    static readonly string API_KEY_CONFIG_KEY = "Cnc.Remote.ApiKey";

    readonly RequestDelegate m_next; // Won't be used: this is the last middleware to call
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public CreateApiKeyMiddleware (RequestDelegate next, ResponseWriter responseWriter)
    {
      m_next = next;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var apiKey = Lemoine.Info.ConfigSet.LoadAndGet (API_KEY_CONFIG_KEY, "");
      if (!string.IsNullOrEmpty (apiKey)) {
        await m_responseWriter.WriteToBodyAsync (context, "API Key is already set", "text/plain");
        return;
      }

      apiKey = System.Guid.NewGuid ().ToString ();
      Lemoine.Info.ConfigSet.SetPersistentConfig (API_KEY_CONFIG_KEY, apiKey);

      await m_responseWriter.WriteToBodyAsync (context, apiKey, "text/plain");
      return;
    }
  }
}
