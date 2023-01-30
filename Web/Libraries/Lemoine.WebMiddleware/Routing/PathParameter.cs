// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.WebMiddleware.Routing
{
  /// <summary>
  /// Path parameter
  /// </summary>
  public class PathParameter
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pathIndex"></param>
    /// <param name="mappedPropertyName"></param>
    /// <param name="parameterValue"></param>
    public PathParameter (int pathIndex, string? mappedPropertyName, string? parameterValue = null)
    {
      this.PathIndex = pathIndex;
      this.MappedPropertyName = mappedPropertyName;
      this.ParameterValue = parameterValue;
    }

    /// <summary>
    /// 
    /// </summary>
    public int PathIndex { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string? MappedPropertyName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? ParameterValue { get; private set; }
  }
}
