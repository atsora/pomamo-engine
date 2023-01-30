// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Lemoine.CncEngine;
using Lemoine.Core.Extensions.Web;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;

namespace Lem_CncCoreService.Asp
{
  /// <summary>
  /// CncMiddleware
  /// </summary>
  public class XmlPostMiddleware
  {
    readonly ILog log = LogManager.GetLogger<GetMiddleware> ();

    readonly RequestDelegate m_next; // Won't be used: this is the last middleware to call
    readonly Lemoine.CncEngine.Asp.XmlPostService m_service;
    readonly ResponseWriter m_responseWriter;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public XmlPostMiddleware (RequestDelegate next, IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder, ResponseWriter responseWriter)
    {
      m_next = next;
      m_service = new Lemoine.CncEngine.Asp.XmlPostService (acquisitionSet, acquisitionFinder);
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var acquisitionIdentifier = context.Request.Query["acquisition"].FirstOrDefault ();

      var cancellationToken = context.RequestAborted;

      var body = context.Request.Body;

      IDictionary<string, object> data;
      try {
        data = m_service.PostXml (cancellationToken, acquisitionIdentifier, body);
      }
      catch (Lemoine.CncEngine.Asp.UnknownAcquisitionException ex) {
        log.Error ($"InvokeAsync: unknown acquisition {acquisitionIdentifier}", ex);
        await m_responseWriter.WriteToBodyAsync (context, ex.Message);
        return;
      }
      catch (Lemoine.CncEngine.Asp.FinalDataNullException ex) {
        log.Warn ($"InvokeAsync: final data null, initializing ?", ex);
        await m_responseWriter.WriteToBodyAsync (context, ex.Message);
        return;
      }
      await m_responseWriter.WriteToBodyAsync (context, data);
    }
    #endregion // Constructors
  }
}
