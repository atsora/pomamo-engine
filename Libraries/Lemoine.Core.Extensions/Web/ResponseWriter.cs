// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Http;

namespace Lemoine.Core.Extensions.Web
{
  /// <summary>
  /// ResponseWriter
  /// </summary>
  public class ResponseWriter
  {
    readonly ILog log = LogManager.GetLogger<ResponseWriter> ();

    const string ADD_STACK_TRACE_KEY = "ResponseWriter.Exception.StackTrace";
    const bool ADD_STACK_TRACE_DEFAULT = false;

    const string INCLUDE_INNER_EXCEPTION_KEY = "ResponseWriter.Exception.InnerException";
    const bool INCLUDE_INNER_EXCEPTION_DEFAULT = true;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ResponseWriter ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Write the body of the http context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="responseObject"></param>
    /// <param name="effectiveContentType"></param>
    /// <param name="defaultContentType">content type to consider if content type is empty or null</param>
    /// <returns></returns>
    public async Task WriteToBodyAsync (Microsoft.AspNetCore.Http.HttpContext context, object responseObject, string? contentType = "", string defaultContentType = "application/json")
    {
      var effectiveContentType = string.IsNullOrEmpty (contentType)
        ? defaultContentType
        : contentType;

      if (effectiveContentType is null) {
        log.Error ($"WriteToBodyAsync: null defaultContentType");
        throw new ArgumentNullException (nameof(defaultContentType));
      }

      //context.Response.Headers["Content-Type"] = contentType;
      context.Response.ContentType = effectiveContentType;

      string s;
      if (effectiveContentType.Equals ("application/json")) {
        // Note: because the following code does not work:
        /*
        JsonSerializer.SerializeAsync (context.Response.Body, responseObject);
        */
        // here is a work around configure the KestrelServerOptions the following way:
        // services.Configure<KestrelServerOptions> (options => { options.AllowSynchronousIO = true; });

        var options = new JsonSerializerOptions {
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add (new JsonStringEnumConverter ());
        s = JsonSerializer.Serialize (responseObject, options);
      }
      else if (effectiveContentType.Equals ("application/octet-stream")) {
        if (log.IsDebugEnabled) {
          log.Debug ($"WriteToBodyAsync: content type is {effectiveContentType} => send directly the bytes");
        }
        if (responseObject is byte[]) {
          byte[] responseBytes = (byte[])responseObject;
          await using (var streamWriter = new BinaryWriter (context.Response.Body)) {
            streamWriter.Write (responseBytes);
          }
          return;
        }
        else {
          s = responseObject?.ToString () ?? "";
          log.Error ($"WriteToBodyAsync: content type is {effectiveContentType} while the type {responseObject?.GetType ()} is not byte[] => fallback to {s}");
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"WriteToBodyAsync: content type is {effectiveContentType} => use ToString () directly");
        }
        s = responseObject?.ToString () ?? "";
      }
      await using (var streamWriter = new StreamWriter (context.Response.Body)) {
        await streamWriter.WriteAsync (s);
      }
    }

  }
}

#endif // NETCOREAPP
