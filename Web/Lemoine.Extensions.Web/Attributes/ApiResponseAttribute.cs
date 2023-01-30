// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Extensions.Web.Attributes
{
  /// <summary>
  /// ApiResponse attribute in a web service definition
  /// </summary>
  //TODO define attribute scope
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
  public class ApiResponseAttribute : Attribute
  {
    /// <summary>
    /// </summary>
    /// <param name="httpStatusCode"></param>
    /// <param name="description"></param>
    public ApiResponseAttribute (System.Net.HttpStatusCode httpStatusCode, string description)
    {
#if DEBUG
      if (string.IsNullOrWhiteSpace (description)) {
        throw new ArgumentException ("description", nameof (description));
      }
#endif
      HttpStatusCode = httpStatusCode;
      Description = description;
    }

    /// <summary>
    /// Returned status code
    /// </summary>
    public System.Net.HttpStatusCode HttpStatusCode { get; set; }

    /// <summary>
    /// Description of the response
    /// </summary>
    public string Description { get; set; }
  }
}
