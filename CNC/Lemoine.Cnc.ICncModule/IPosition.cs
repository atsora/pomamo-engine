// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.Cnc
{
  /// <summary>
  /// IPosition interface
  /// </summary>
  public interface IPosition
  {
    /// <summary>
    /// Get an axis value
    /// 
    /// null if not defined
    /// </summary>
    /// <param name="axisName">X, Y, Z, A, B, C, U , V, W</param>
    /// <returns></returns>
    double? GetAxisValue (string axisName);

    /// <summary>
    /// Set an axis value
    /// </summary>
    /// <param name="v"></param>
    /// <param name="axisName">X, Y, Z, A, B, C, U, V, W</param>
    void SetAxisValue (double v, string axisName);

    /// <summary>
    /// UTC Date/time of the position
    /// 
    /// It is used to compute the feedrate
    /// </summary>
    DateTime Time { get; }
  }
}
