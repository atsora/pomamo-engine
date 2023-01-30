// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Utility static methods to get a day
  /// </summary>
  public static class Day
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Day).FullName);

    /// <summary>
    /// Get a day
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public static DateTime From (int year, int month, int day)
    {
      return new DateTime (year, month, day);
    }
  }
}
