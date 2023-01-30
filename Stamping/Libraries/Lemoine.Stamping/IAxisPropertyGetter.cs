// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Axis unit
  /// </summary>
  public enum AxisUnit
  { 
    /// <summary>
    /// Unknown / default
    /// 
    /// No conversion is applied
    /// </summary>
    Default = 0,
    /// <summary>
    /// Millimeters
    /// </summary>
    Mm = 1,
    /// <summary>
    /// Inches
    /// </summary>
    In = 2,
    /// <summary>
    /// Degrees
    /// </summary>
    Degree = 3,
  }

  /// <summary>
  /// 
  /// </summary>
  public interface IAxisPropertyGetter
  {
    /// <summary>
    /// Default unit
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    AxisUnit GetDefaultUnit (string axis);

    /// <summary>
    /// Get the maximum velocity in mm/s
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    double? GetMaxVelocity (string axis);

    /// <summary>
    /// Get the maximum acceleration in mm/s2
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    double? GetMaxAcceleration (string axis);

    /// <summary>
    /// Get the maximum deceleration in mm/s2
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    double? GetMaxDeceleration (string axis);
  }
}
