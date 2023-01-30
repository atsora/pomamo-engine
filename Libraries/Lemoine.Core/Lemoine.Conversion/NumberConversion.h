// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#pragma once

#include <string>

namespace Lemoine
{
  namespace Conversion
  {
    /// <summary>
    /// Convert a value to a metric value
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <param name="metric">Is the value metric ?</param>
    /// <returns>The metric value</returns>
    inline double ConvertToMetric (double value, bool metric)
    {
      if (metric) {
        return value;
      }
      else {
        return value * 25.4;
      }
    }
  }
}
