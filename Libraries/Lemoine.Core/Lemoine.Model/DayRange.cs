// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Day range (from cut-off time)
  /// 
  /// By default, the inclusivity parameter is "[]"
  /// 
  /// It is considered as a discrete range (IsAdjacentTo was overriden)
  /// </summary>
  [Serializable]
  public class DayRange : Range<DateTime>
  {
    readonly ILog log = LogManager.GetLogger (typeof (DayRange).FullName);

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public DayRange (LowerBound<DateTime> begin, UpperBound<DateTime> end)
      : base (begin, end, "[]")
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    public DayRange (LowerBound<DateTime> begin, UpperBound<DateTime> end, bool lowerInclusive, bool upperInclusive)
      : base (begin, end, lowerInclusive, upperInclusive)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="s"></param>
    public DayRange (string s)
      : base (s)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="range"></param>
    public DayRange (Range<DateTime> range)
      : base (range.IsEmpty (),
              new LowerBound<DateTime> ((!range.IsEmpty () && range.Lower.HasValue) ? range.Lower.Value.Date : default (DateTime?)),
              new UpperBound<DateTime> ((!range.IsEmpty () && range.Upper.HasValue) ? range.Upper.Value.Date : default (DateTime?)),
              range.LowerInclusive,
              range.UpperInclusive)
    {
    }

    /// <summary>
    /// Convert the bounds to inclusive bounds
    /// 
    /// <item>(x is converted into [x+1</item>
    /// <item>x) is converted into x-1]</item>
    /// </summary>
    protected override void GetCanonical ()
    {
      if (!this.IsEmpty ()) {
        if (!this.UpperInclusive && this.Upper.HasValue) {
          this.Upper = this.Upper.Value.AddDays (-1);
          this.UpperInclusive = true;
        }
        if (!this.LowerInclusive && this.Lower.HasValue) {
          this.Lower = this.Lower.Value.AddDays (+1);
          this.LowerInclusive = true;
        }
      }
    }

    /// <summary>
    /// Parse a string into a UTC date/time
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected override DateTime ParseBound (string s)
    {
      IFormatProvider provider = CultureInfo.InvariantCulture;
      return System.DateTime.Parse (s, provider);
    }

    /// <summary>
    /// Convert a Date/Time to a string in UTC
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected override string ConvertBoundToString (DateTime bound)
    {
      return bound.ToString ("yyyy-MM-dd");
    }

    /// <summary>
    /// Is adjacent to operator
    /// 
    /// Corresponds to Operator -|- in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool IsAdjacentTo (Range<DateTime> other)
    {
      if (this.IsEmpty () || other.IsEmpty ()) {
        log.WarnFormat ("IsAdjacentTo: " +
                        "empty, return false");
        return false;
      }
      else if (this.Upper.HasValue
               && other.Lower.HasValue
               && object.Equals (this.Upper.Value, other.Lower.Value)
               && (this.UpperInclusive || other.LowerInclusive)
               && !(this.UpperInclusive && other.LowerInclusive)) {
        return true;
      }
      else if (this.Lower.HasValue
               && other.Upper.HasValue
               && object.Equals (this.Lower.Value, other.Upper.Value)
               && (this.LowerInclusive || other.UpperInclusive)
               && !(this.LowerInclusive && other.UpperInclusive)) {
        return true;
      }
      else if (this.Upper.HasValue
               && other.Lower.HasValue
               && this.UpperInclusive
               && other.LowerInclusive
               && object.Equals (this.Upper.Value.AddDays (1), other.Lower.Value)) {
        return true;
      }
      else if (this.Lower.HasValue
               && other.Upper.HasValue
               && this.LowerInclusive
               && other.UpperInclusive
               && object.Equals (other.Upper.Value.AddDays (1), this.Lower.Value)) {
        return true;
      }
      else {
        return false;
      }
    }
  }

  /// <summary>
  /// DayRange
  /// </summary>
  public static class DayRangeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DayRangeExtensions).FullName);

    /// <summary>
    /// Get an enumerable on days from a day range
    /// </summary>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> GetDays (this IRange<DateTime> dayRange)
    {
      Debug.Assert (dayRange.Lower.HasValue);
      Debug.Assert (dayRange.Upper.HasValue);

      if (!dayRange.Lower.HasValue) {
        log.Error ($"GetDays: {dayRange} has not a finite lower value");
        throw new ArgumentException ("Invalid lower value", "dayRange");
      }
      if (!dayRange.Upper.HasValue) {
        log.Error ($"GetDays: {dayRange} has not a finite upper value");
        throw new ArgumentException ("Invalid upper value", "dayRange");
      }

      var min = dayRange.LowerInclusive
        ? dayRange.Lower.Value
        : dayRange.Lower.Value.AddDays (1);
      var max = dayRange.UpperInclusive
        ? dayRange.Upper.Value
        : dayRange.Upper.Value.AddDays (-1);
      for (var day = min; day <= max; day = day.AddDays (1)) {
        yield return day;
      }
    }
  }
}
