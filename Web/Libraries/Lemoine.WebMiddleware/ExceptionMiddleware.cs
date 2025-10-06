// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Log;
using Lemoine.WebMiddleware.Response;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;
using Lemoine.Core.ExceptionManagement;
using Microsoft.IdentityModel.Tokens;
using Lemoine.WebMiddleware.HttpContext;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// ExceptionMiddleware
  /// </summary>
  public class ExceptionMiddleware
  {
    readonly ILog log = LogManager.GetLogger<ExceptionMiddleware> ();

    readonly RequestDelegate m_next;
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public ExceptionMiddleware (RequestDelegate next, ResponseWriter responseWriter)
    {
      m_next = next;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      try {
        await m_next.Invoke (context);
      }
      catch (Exception ex) {
        if (ExceptionTest.IsNotError (ex, log)) {
          log.Info ("InvokeAysnc: exception not really in error", ex);
        }
        else {
          log.Error ("InvokeAsync: exception", ex);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await m_responseWriter.WriteExceptionToBodyAsync (context, ex);

        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          try {
            log.Fatal ("Exception requires to exit", ex);
          }
          catch (Exception) { }

          Lemoine.Core.Environment.LogAndForceExit (ex, log);
        }
      }
    }
  }
}
