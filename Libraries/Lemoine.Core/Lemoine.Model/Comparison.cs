// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Utility class for object comparison
  /// </summary>
  public static class Comparison
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Comparison).FullName);

    /// <summary>
    /// Apply the equals function only if the two objects are not null, else return true only if they are both null
    /// </summary>
    /// <returns></returns>
    public static bool EqualsNullable<T, U> (T a, U b, Func<T, U, bool> equals)
    {
      if (null == a) {
        return (null == b);
      }
      if (null == b) {
        return false;
      }
      Debug.Assert ((null != a) && (null != b));
      return equals (a, b);
    }
  }
}
