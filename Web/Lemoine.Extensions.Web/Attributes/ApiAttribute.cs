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
  /// Api attribute in a web service definition
  /// </summary>
  //TODO define attribute scope
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
  public class ApiAttribute : Attribute
  {
    /// <summary>
    /// </summary>
    /// <param name="description"></param>
    public ApiAttribute (string description)
    {
#if DEBUG
      if (string.IsNullOrWhiteSpace (description)) {
        throw new ArgumentException ("description", nameof (description));
      }
#endif
      Description = description;
    }

    /// <summary>
    /// Description of web API
    /// </summary>
    public string Description { get; set; }
  }
}
