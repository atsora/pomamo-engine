// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.WebMiddleware.HttpContext;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware.Response
{
  /// <summary>
  /// ResponseWriter
  /// </summary>
  public class ResponseWriter: Lemoine.Core.Extensions.Web.ResponseWriter
  {
    readonly ILog log = LogManager.GetLogger<ResponseWriter> ();

    const string ADD_STACK_TRACE_KEY = "ResponseWriter.Exception.StackTrace";
    const bool ADD_STACK_TRACE_DEFAULT = false;

    const string INCLUDE_INNER_EXCEPTION_KEY = "ResponseWriter.Exception.InnerException";
    const bool INCLUDE_INNER_EXCEPTION_DEFAULT = true;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ResponseWriter ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Additional method to directly output an exception in an ExceptionDTO
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ex"></param>
    /// <param name="contentType"></param>
    /// <param name="defaultContentType"></param>
    /// <returns></returns>
    public async Task WriteExceptionToBodyAsync (Microsoft.AspNetCore.Http.HttpContext context, Exception ex, string? contentType = "", string defaultContentType = "application/json")
    {
      Debug.Assert (null != ex);

      bool addStackTrace = Lemoine.Info.ConfigSet.LoadAndGet (ADD_STACK_TRACE_KEY, ADD_STACK_TRACE_DEFAULT);
      bool includeInnerException = Lemoine.Info.ConfigSet.LoadAndGet (INCLUDE_INNER_EXCEPTION_KEY, INCLUDE_INNER_EXCEPTION_DEFAULT);

      var effectiveContentType = string.IsNullOrEmpty (contentType)
        ? defaultContentType
        : contentType;

      switch (effectiveContentType) {
      case "application/json":
        await WriteToBodyAsync (context, new ExceptionDTO (ex, addStackTrace: addStackTrace, includeInnerException: includeInnerException), effectiveContentType);
        break;
      case "text/plain":
        await WriteToBodyAsync (context, GetExceptionString (ex, addStackTrace: addStackTrace, includeInnerException: includeInnerException), effectiveContentType);
        break;
      default:
        await WriteToBodyAsync (context, ex, effectiveContentType);
        break;
      };
    }

    string GetExceptionString (Exception ex, bool addStackTrace = false, bool includeInnerException = false)
    {
      string s = $"{ex.GetType ().FullName}: {ex.Message}";
      if (addStackTrace && !(ex.StackTrace is null)) {
        s += $"\n{ex.StackTrace}";
      }
      if (includeInnerException && !(ex.InnerException is null)) {
        s += "\n";
        s += GetExceptionString (ex.InnerException, addStackTrace, true);
      }
      return s;
    }
  }
}
