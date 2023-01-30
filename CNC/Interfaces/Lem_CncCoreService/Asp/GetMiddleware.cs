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
  public class GetMiddleware
  {
    readonly ILog log = LogManager.GetLogger<GetMiddleware> ();

    readonly RequestDelegate m_next; // Won't be used: this is the last middleware to call
    readonly Lemoine.CncEngine.Asp.GetService m_service;
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public GetMiddleware (RequestDelegate next, IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder, ResponseWriter responseWriter)
    {
      m_next = next;
      m_service = new Lemoine.CncEngine.Asp.GetService (acquisitionSet, acquisitionFinder);
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var acquisitionIdentifier = context.Request.Query["acquisition"].FirstOrDefault ();
      var moduleref = context.Request.Query["moduleref"].FirstOrDefault ();
      var method = context.Request.Query["method"].FirstOrDefault ();
      var property = context.Request.Query["property"].FirstOrDefault ();
      var param = context.Request.Query["param"].FirstOrDefault ();

      var singleResponse = m_service.Get (context.RequestAborted, acquisitionIdentifier, moduleref, method, property, param);
      await m_responseWriter.WriteToBodyAsync (context, singleResponse);
      return;
    }
  }
}
