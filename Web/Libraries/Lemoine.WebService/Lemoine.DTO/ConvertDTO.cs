// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Diagnostics;  // Debug.Assert
using System.Globalization;

namespace Lemoine.DTO
{
  /// <summary>
  /// converts a DateTime value to
  /// IsoString "2014-04-04T10:00:00Z"
  /// OR
  /// its representation as number of ticks from "Java"
  /// origin (01/01/1970) expressed in milliseconds => Should be removed
  /// </summary>
  public class ConvertDTO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConvertDTO).FullName);

    #region Methods
    /// <summary>
    /// Convert an UTC DateTime to IsoString with T and Z
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoString(DateTime utcDate)
    {
      Debug.Assert(utcDate.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
      //string dateString = utcDate.GetDateTimeFormats("s") + "Z";
      string dateString = utcDate.ToString("s") + "Z";
      return dateString;
    }
    
    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoString(Bound<DateTime> utcDate)
    {
      if (utcDate.HasValue) {
        Debug.Assert(utcDate.Value.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
        string dateString = utcDate.Value.ToString("s") + "Z";
        return dateString;
      } else {
        return ""; // or null ?
      }
    }
    
    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoString(DateTime? utcDate)
    {
      if (utcDate.HasValue) {
        Debug.Assert(utcDate.Value.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
        string dateString = utcDate.Value.ToString("s") + "Z";
        return dateString;
      } else {
        return ""; // or null ?
      }
    }
    
    /// <summary>
    /// Convert IsoString with T and Z to an UTC DateTime
    /// </summary>
    /// <param name="isoDate"></param>
    /// <returns>utcDate</returns>
    public static DateTime? IsoStringToDateTimeUtc(string isoDate)
    {
      //DateTime utcDate = DateTime.Parse(isoDate, null, DateTimeStyles.RoundtripKind);
      try {
        DateTime? utcDate = DateTime.ParseExact(isoDate,
                                                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                CultureInfo.InvariantCulture,
                                                DateTimeStyles.AssumeUniversal |
                                                DateTimeStyles.AdjustToUniversal);
        return utcDate;
      } catch {
        try {
          DateTime? utcDate = DateTime.ParseExact(isoDate,
                                                  "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.AssumeUniversal |
                                                  DateTimeStyles.AdjustToUniversal);
          return utcDate;
        } catch {
          return null;
        }
      }
    }
    
    /// <summary>
    /// Convert two date/times in ISO string into a UTC date/time range
    /// </summary>
    /// <param name="isoBegin"></param>
    /// <param name="isoEnd"></param>
    /// <returns></returns>
    public static UtcDateTimeRange IsoStringToUtcDateTimeRange (string isoBegin, string isoEnd)
    {
      LowerBound<DateTime> begin = new LowerBound<DateTime> (IsoStringToDateTimeUtc (isoBegin));
      UpperBound<DateTime> end = new UpperBound<DateTime> (IsoStringToDateTimeUtc (isoEnd));
      return new UtcDateTimeRange (begin, end);
    }
    
    /// <summary>
    /// Convert a day in format yyyy-MM-dd
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static string DayToString (DateTime? day)
    {
      if (!day.HasValue) {
        return null;
      }
      else {
        Debug.Assert (DateTimeKind.Utc != day.Value.Kind);
        Debug.Assert (TimeSpan.FromTicks (0) == day.Value.TimeOfDay);
        
        return day.Value.ToString ("yyyy-MM-dd");
      }
    }
    
    /// <summary>
    /// Convert a string in format yyyy-MM-dd into a day in format DateTime
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static DateTime? StringToDay (string day)
    {
      if (string.IsNullOrEmpty (day)) {
        return null;
      }
      else {
        return DateTime.ParseExact (day, "yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
    }
    
    /// <summary>
    /// Convert an UTC DateTime to the equivalent number of ms since (positive) or until (negative) 1970/1/1
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static long ConvertDateTimeToJavaTotalTicksMilliseconds(DateTime utcDate)
    {
      DateTime javaOriginDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      TimeSpan javaOriginOffsetInTicks = TimeSpan.FromTicks(javaOriginDate.Ticks);
      TimeSpan utcDateOffsetInTicks = TimeSpan.FromTicks(utcDate.Ticks);
      TimeSpan diffOffsets = utcDateOffsetInTicks - javaOriginOffsetInTicks;
      return (long) diffOffsets.TotalMilliseconds;
    }
    
    /// <summary>
    /// Convert a nullable UTC DateTime to the equivalent number of ms since (positive) or until (negative) 1970/1/1
    /// </summary>
    /// <param name="utcDateOpt"></param>
    /// <returns></returns>
    public static Nullable<long> ConvertDateTimeToJavaTotalTicksMilliseconds(Bound<DateTime> utcDateOpt) {
      if (utcDateOpt.HasValue) {
        return ConvertDateTimeToJavaTotalTicksMilliseconds(utcDateOpt.Value);
      } else {
        return null;
      }
    }
    
    /// <summary>
    /// Convert a nullable UTC DateTime to the equivalent number of ms since (positive) or until (negative) 1970/1/1
    /// </summary>
    /// <param name="utcDateOpt"></param>
    /// <returns></returns>
    public static Nullable<long> ConvertDateTimeToJavaTotalTicksMilliseconds(DateTime? utcDateOpt) {
      if (utcDateOpt.HasValue) {
        return ConvertDateTimeToJavaTotalTicksMilliseconds(utcDateOpt.Value);
      } else {
        return null;
      }
    }
    
    /// <summary>
    /// Convert an "offset in ms from the java origin of datetime" to a datetime
    /// </summary>
    /// <param name="javaOffsetInMs"></param>
    /// <returns></returns>
    public static DateTime ConvertJavaTotalTicksMillisecondsToDateTime(long javaOffsetInMs) {
      // Beware: do not leave DateTimeKind Unspecified, it would be considered Local for ToUniversalTime() !
      DateTime javaOriginDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      return javaOriginDate.AddMilliseconds(javaOffsetInMs);
    }

    /// <summary>
    /// Convert a nullable "offset in ms from the java origin of datetime" to a datetime
    /// </summary>
    /// <param name="javaOffsetInMsOpt"></param>
    /// <returns></returns>
    public static DateTime? ConvertJavaTotalTicksMillisecondsToDateTime(long? javaOffsetInMsOpt) {
      if (javaOffsetInMsOpt.HasValue) {
        return ConvertJavaTotalTicksMillisecondsToDateTime(javaOffsetInMsOpt.Value);
      } else {
        return null;
      }
    }
    
    /// <summary>
    /// Convert a pairt of nullable offset in ms from the java origin of data/time to a UTC date/time range
    /// </summary>
    /// <param name="beginOffset"></param>
    /// <param name="endOffset"></param>
    /// <returns></returns>
    public static UtcDateTimeRange JavaTotalTicksMillisecondsToUtcDateTimeRange (long? beginOffset, long? endOffset)
    {
      LowerBound<DateTime> begin = new LowerBound<DateTime> (ConvertJavaTotalTicksMillisecondsToDateTime (beginOffset));
      UpperBound<DateTime> end = new UpperBound<DateTime> (ConvertJavaTotalTicksMillisecondsToDateTime (endOffset));
      return new UtcDateTimeRange (begin, end);
    }
    #endregion // Methods
  }
}
