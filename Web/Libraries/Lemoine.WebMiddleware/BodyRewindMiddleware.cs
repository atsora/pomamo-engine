// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// BodyRewindMiddleware
  /// 
  /// It requires package Microsoft.AspNetCore.Http
  /// </summary>
  public class BodyRewindMiddleware
  {
    readonly ILog log = LogManager.GetLogger<BodyRewindMiddleware> ();

    readonly RequestDelegate m_next;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public BodyRewindMiddleware (RequestDelegate next)
    {
      m_next = next;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      if (context.Request.Method.Equals ("POST")) {
        try {
          context.Request.EnableBuffering ();
        }
        catch (Exception ex) {
          log.Error ("InvokeAsync: EnableBuffering failed", ex);
        }
      }
      await m_next.Invoke (context);
    }
    #endregion // Constructors
  }
}
