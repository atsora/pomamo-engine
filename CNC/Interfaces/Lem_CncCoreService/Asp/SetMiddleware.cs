// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;
using Lemoine.CncEngine;
using Lemoine.Core.Extensions.Web;
using Lemoine.Cnc.Asp;

namespace Lem_CncCoreService.Asp
{
  /// <summary>
  /// CncMiddleware
  /// </summary>
  public class SetMiddleware
  {
    readonly ILog log = LogManager.GetLogger<SetMiddleware> ();

    readonly RequestDelegate m_next; // Won't be used: this is the last middleware to call
    readonly Lemoine.CncEngine.Asp.SetService m_service;
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public SetMiddleware (RequestDelegate next, IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder, ResponseWriter responseWriter)
    {
      m_next = next;
      m_service = new Lemoine.CncEngine.Asp.SetService (acquisitionSet, acquisitionFinder);
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var acquisitionIdentifier = context.Request.Query["acquisition"].FirstOrDefault ();
      var moduleref = context.Request.Query["moduleref"].First ();
      var method = context.Request.Query["method"].FirstOrDefault ();
      var property = context.Request.Query["property"].FirstOrDefault ();
      var param = context.Request.Query["param"].FirstOrDefault ();
      var v = context.Request.Query["v"].FirstOrDefault ();
      var stringvalue = context.Request.Query["string"].FirstOrDefault ();
      var longvalue = context.Request.Query["long"].FirstOrDefault ();
      var intvalue = context.Request.Query["int"].FirstOrDefault ();
      var doublevalue = context.Request.Query["double"].FirstOrDefault ();
      var boolvalue = context.Request.Query["boolean"].FirstOrDefault ();

      var cancellationToken = context.RequestAborted;

      var singleResponse = await m_service.SetAsync (cancellationToken, acquisitionIdentifier, moduleref, method, property, param, v, stringvalue, longvalue, intvalue, doublevalue, boolvalue);
      await m_responseWriter.WriteToBodyAsync (context, singleResponse);
      return;
    }
  }
}
