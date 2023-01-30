// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Class to help the conversions
  /// </summary>
  public static class Converter
  {
    /// <summary>
    /// Convert a value to a metric value
    /// </summary>
    /// <param name="myvalue">Value to convert</param>
    /// <param name="metric">Is the value metric ?</param>
    /// <returns>The metric value</returns>
    public static double ConvertToMetric (double myvalue, bool metric)
    {
      if (metric) {
        return myvalue;
      }
      else {
        return myvalue * 25.4;
      }
    }

    /// <summary>
    /// Convert a value in mm to inches
    /// </summary>
    /// <param name="myvalue">Value to convert</param>
    /// <returns>The value in inches</returns>
    public static double ConvertToInches (double myvalue)
    {
      return myvalue / 25.4;
    }
  }
}
