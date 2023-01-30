// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Utility static methods to get a date/time in UTC
  /// </summary>
  public static class UtcDateTime
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UtcDateTime).FullName);

    /// <summary>
    /// Get a UTC date/time
    /// </summary>
    /// <param name="ticks"></param>
    /// <returns></returns>
    public static DateTime From (long ticks)
    {
      return new DateTime (ticks, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Get a UTC date/time
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    public static DateTime From (int year, int month, int day)
    {
      return new DateTime (year, month, day, 00, 00, 00, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Get a UTC date/time
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static DateTime From (int year, int month, int day, int hour, int minute, int second)
    {
      return new DateTime (year, month, day, hour, minute, second, DateTimeKind.Utc);
    }
    
    /// <summary>
    /// Parse a UTC date/time
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime Parse (string s)
    {
      return DateTime.Parse (s).ToUniversalTime ();
    }
  }
}
