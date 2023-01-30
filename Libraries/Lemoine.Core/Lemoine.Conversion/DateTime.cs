// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lemoine.Core.Log;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Conversion extensions to the DateTime type
  /// </summary>
  public static class DateTimeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DateTimeExtensions));

    /// <summary>
    /// Convert a day to Iso String: yyy-MM-dd
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static string DayToIsoString (this DateTime day)
    {
      Debug.Assert (day.Kind != DateTimeKind.Utc);
      string dayString = day.ToString ("yyyy'-'MM'-'dd");
      return dayString;
    }

    /// <summary>
    /// Convert a date/time to an ISO string in UTC with T and Z
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToIsoString (this DateTime dateTime)
    {
      return dateTime.ToUniversalTime ().ToString ("s") + "Z";
    }

    /// <summary>
    /// Convert a date/time to an ISO string considering the input date/time is already in Utc (even if it is unspecified)
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string UtcToIsoString (this DateTime utcDateTime)
    {
      Debug.Assert (utcDateTime.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
      string dateString = utcDateTime.ToString ("s") + "Z";
      return dateString;
    }

    /// <summary>
    /// Convert a date/time to an ISO String local (with T and no Z)
    /// 
    /// If the input date/time is in UTC it is converted to a local time first
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToLocalIsoString (this DateTime dateTime)
    {
      if (DateTimeKind.Utc == dateTime.Kind) {
        log.WarnFormat ("DateTimeLocalToIsoString: " +
                        "convert UTC time {0} into local",
                        dateTime);
        string dateString = dateTime.ToLocalTime ().ToString ("s");
        return dateString;
      }
      else {
        string dateString = dateTime.ToString ("s");
        return dateString;
      }
    }

    /// <summary>
    /// If the DateTimeKind is Unspecified, then the date/time is converted to ISO String with no specific suffix.
    /// 
    /// Else the Date/Time is converted in UTC and suffixed with Z
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToCacheString (this DateTime dateTime)
    {
      switch (dateTime.Kind) {
      case DateTimeKind.Utc:
      case DateTimeKind.Local:
        return dateTime.ToIsoString ();
      case DateTimeKind.Unspecified:
        return dateTime.ToString ("s");
      default:
        throw new InvalidOperationException ("ToCacheString with an unexpected DateTimeKind");
      }
    }
  }
}
