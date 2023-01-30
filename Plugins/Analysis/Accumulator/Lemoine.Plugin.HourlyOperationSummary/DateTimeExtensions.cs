// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  /// <summary>
  /// DateTimeExtensions
  /// </summary>
  public static class DateTimeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DateTimeExtensions).FullName);

    /// <summary>
    /// Convert a date/time to a local date truncated to the hour
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ToLocalDateHour (this DateTime dateTime)
    {
      var localDateTime = dateTime.ToLocalTime ();
      return new DateTime (localDateTime.Year, localDateTime.Month, localDateTime.Day,
        localDateTime.Hour, 00, 00, DateTimeKind.Local);
    }
  }
}
