// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using Lemoine.Core.Log;

namespace Pulse.Graphql
{
  /// <summary>
  /// <see cref="IErrorInfoProvider"/> implementation
  /// </summary>
  public class ErrorInfoProvider : GraphQL.Execution.ErrorInfoProvider, IErrorInfoProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ErrorInfoProvider).FullName);

    bool m_exposeExceptionDetails;

    /// <summary>
    /// Constructor
    /// </summary>
    public ErrorInfoProvider (bool exposeExceptionDetails = false)
      : base (options => options.ExposeExceptionDetails = exposeExceptionDetails)
    {
      m_exposeExceptionDetails = exposeExceptionDetails;
    }

    /// <summary>
    /// <see cref="IErrorInfoProvider"/>
    /// </summary>
    /// <param name="executionError"></param>
    /// <returns></returns>
    public override ErrorInfo GetInfo (ExecutionError executionError)
    {
      var info = base.GetInfo (executionError);

      var extensions = info.Extensions;
      if (extensions is not null) {

        if (extensions.TryGetValue ("data", out object? dataObject)) {
          if (dataObject is IDictionary dataDictionary) {
            if (dataDictionary.Contains ("ErrorStatus")) {
              var errorStatusObject = dataDictionary["ErrorStatus"];
              if (errorStatusObject is string errorStatus) {
                var code = $"Pomamo.{errorStatus}";
                extensions["code"] = code;
                if (extensions.TryGetValue ("codes", out object? existingCodesObject)) {
                  if (existingCodesObject is IList<string> existingCodes) {
                    existingCodes.Insert (0, code);
                  }
                  else {
                    log.Error ($"GetInfo: Codes of type {existingCodesObject?.GetType ()}");
                  }
                }
                else {
                  extensions["codes"] = new List<string> { code };
                }
                return info;
              }
            }
          }
        }

        if (executionError.InnerException is DataProcessingException ex) {
          var code = $"Pomamo.{ex.ErrorStatus.ToString ()}";
          extensions["code"] = code;
          if (extensions.TryGetValue ("codes", out object? existingCodesObject)) {
            if (existingCodesObject is IList<string> existingCodes) {
              existingCodes.Insert (0, code);
            }
            else {
              log.Error ($"GetInfo: Codes of type {existingCodesObject?.GetType ()}");
            }
          }
          else {
            extensions["codes"] = new List<string> { code };
          }
        }
      }

      return info;
    }
  }
}
