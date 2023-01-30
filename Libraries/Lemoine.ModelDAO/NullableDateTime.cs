// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Class to make operations on the nullable date/times
  /// By default, it corresponds to an upper bound: null corresponds to +oo
  /// </summary>
  public static class NullableDateTime
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NullableDateTime).FullName);

    /// <summary>
    /// Compare two nullable date/times considering their bound
    /// 
    /// If one of the two date/times is local while the other one is universal,
    /// do the necessary conversions
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int Compare (DateTime? t1, UpperBound<DateTime> t2)
    {
      return Compare ((UpperBound<DateTime>) t1, t2);
    }

    /// <summary>
    /// Compare two nullable date/times considering their bound
    /// 
    /// If one of the two date/times is local while the other one is universal,
    /// do the necessary conversions
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int Compare (UpperBound<DateTime> t1, DateTime? t2)
    {
      return Compare (t1, (UpperBound<DateTime>) t2);
    }

    /// <summary>
    /// Compare two nullable date/times considering their bound
    /// 
    /// If one of the two date/times is local while the other one is universal,
    /// do the necessary conversions
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int Compare (Bound<DateTime> t1, Bound<DateTime> t2)
    {
      if (t1.HasValue && t2.HasValue) {
        if (t1.Value.Kind.Equals (t2.Value.Kind)) {
          return DateTime.Compare (t1.Value, t2.Value);
        }
        else if (t1.Value.Kind.Equals (DateTimeKind.Unspecified)) {
          log.WarnFormat ("Compare: " +
                          "try to compare t1 {0} that unspecified with t2 {1}",
                          t1, t2);
          return DateTime.Compare (t1.Value, t2.Value);
        }
        else if (t2.Value.Kind.Equals (DateTimeKind.Unspecified)) {
          log.WarnFormat ("Compare: " +
                          "try to compare t2 {0} that unspecified with t1 {1}",
                          t2, t1);
          return DateTime.Compare (t1.Value, t2.Value);
        }
        else {
          log.InfoFormat ("Compare: " +
                          "compare two date times {0} and {1} of different types, " +
                          "convert them first to UTC",
                          t1, t2);
          return DateTime.Compare (t1.Value.ToUniversalTime (), t2.Value.ToUniversalTime ());
        }
      }
      
      return Bound.Compare<DateTime> (t1, t2);
    }

    /// <summary>
    /// Get the maximum value of two nullable date/times
    /// considering that null corresponds to +oo
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static Bound<DateTime> GetMaximum (Bound<DateTime> t1, Bound<DateTime> t2)
    {
      if (Compare (t1, t2) <= 0) {
        return t2;
      }
      else {
        return t1;
      }
    }

    /// <summary>
    /// Get the minimum value of two nullable date/times
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static Bound<DateTime> GetMinimum (Bound<DateTime> t1, Bound<DateTime> t2)
    {
      if (Compare (t1, t2) <= 0) {
        return t1;
      }
      else {
        return t2;
      }
    }
    
    /// <summary>
    /// Truncate the date/time to a second precision
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static DateTime TruncateToSeconds (DateTime t)
    {
      return new DateTime(t.Year, t.Month, t.Day,
                          t.Hour, t.Minute, t.Second, t.Kind);
    }

    /// <summary>
    /// Truncate the date/time to a second precision
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Bound<DateTime> TruncateToSeconds (Bound<DateTime> t)
    {
      if (!t.HasValue) {
        return t;
      }
      else {
        return new Bound<DateTime> (new DateTime(t.Value.Year, t.Value.Month, t.Value.Day,
                                                 t.Value.Hour, t.Value.Minute, t.Value.Second, t.Value.Kind),
                                    t.BoundType);
      }
    }

    /// <summary>
    /// Truncate the date/time to a second precision
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static DateTime? TruncateToSeconds (DateTime? t)
    {
      if (!t.HasValue) {
        return t;
      }
      else {
        return new DateTime(t.Value.Year, t.Value.Month, t.Value.Day,
                            t.Value.Hour, t.Value.Minute, t.Value.Second, t.Value.Kind);
      }
    }
  }
}
