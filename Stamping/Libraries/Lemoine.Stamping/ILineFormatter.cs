// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Stamping
{
  /// <summary>
  /// 
  /// </summary>
  public interface ILineFormatter
  {
    /// <summary>
    /// Is the variable optional ?
    /// </summary>
    bool OptionalVariable { get; }

    /// <summary>
    /// Create a stamp line from a variable and a value
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    string CreateLine (string variable, object v);
  }

  /// <summary>
  /// Extensions to interface <see cref="ILineFormatter"/>
  /// </summary>
  public static class LineFormatterExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (LineFormatterExtensions).FullName);

    /// <summary>
    /// Create a line
    /// </summary>
    /// <param name="lineFormatter"></param>
    /// <param name="variable"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string CreateLineWithVariableCheck (this ILineFormatter lineFormatter, string variable, double v, string variableKind = "")
    {
      if (string.IsNullOrEmpty (variable)) {
        if (lineFormatter.OptionalVariable) {
          log.Info ($"CreateLine: variable {variableKind} is not defined but optional");
          return lineFormatter.CreateLine ("", v);
        }
        else {
          log.Error ($"CreateLine: variable is not defined although mandatory");
          if (string.IsNullOrEmpty (variableKind)) {
            throw new Exception ($"Missing variable");
          }
          else {
            throw new Exception ($"Missing variable for {variableKind}");
          }
        }
      }
      else {
        return lineFormatter.CreateLine (variable, v);
      }
    }
  }
}
