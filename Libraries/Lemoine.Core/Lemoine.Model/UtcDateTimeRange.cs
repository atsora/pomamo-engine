// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// UTC Date/time range
  /// </summary>
  [Serializable]
  public class UtcDateTimeRange : Range<DateTime>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UtcDateTimeRange).FullName);

    /// <summary>
    /// Duration of the range
    /// 
    /// null if Upper or Lower is oo
    /// 
    /// 0 if empty
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get {
        if (this.IsEmpty ()) {
          return TimeSpan.FromTicks (0);
        }
        else if (this.Upper.HasValue && this.Lower.HasValue) {
          return this.Upper.Value.Subtract (this.Lower.Value);
        }
        else {
          log.Debug ("Duration.get: upper or lower is oo => return null because there is no valid duration");
          return null;
        }
      }
    }

    /// <summary>
    /// Constructor for an empty range
    /// </summary>
    public UtcDateTimeRange ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor with no specified end (+oo)
    /// </summary>
    /// <param name="begin"></param>
    public UtcDateTimeRange (LowerBound<DateTime> begin)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToUniversalTime () : default (DateTime?)),
              new UpperBound<DateTime> (null))
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public UtcDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToUniversalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToUniversalTime () : default (DateTime?)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="range"></param>
    public UtcDateTimeRange (Range<DateTime> range)
      : base (range.IsEmpty (),
              new LowerBound<DateTime> ((!range.IsEmpty () && range.Lower.HasValue) ? range.Lower.Value.ToUniversalTime () : default (DateTime?)),
              new UpperBound<DateTime> ((!range.IsEmpty () && range.Upper.HasValue) ? range.Upper.Value.ToUniversalTime () : default (DateTime?)),
              range.LowerInclusive,
              range.UpperInclusive)
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    public UtcDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end, bool lowerInclusive, bool upperInclusive)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToUniversalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToUniversalTime () : default (DateTime?)),
              lowerInclusive, upperInclusive)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="inclusivity"></param>
    public UtcDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end, string inclusivity)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToUniversalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToUniversalTime () : default (DateTime?)),
              inclusivity)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="s"></param>
    public UtcDateTimeRange (string s)
      : base (s)
    {
    }

    /// <summary>
    /// Parse a string into a UTC date/time
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected override DateTime ParseBound (string s)
    {
      IFormatProvider provider = CultureInfo.InvariantCulture;
      return System.DateTime.Parse (s, provider,
                                    DateTimeStyles.AssumeUniversal
                                    | DateTimeStyles.AdjustToUniversal);
    }

    /// <summary>
    /// Convert a Date/Time to a string in UTC
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected override string ConvertBoundToString (DateTime bound)
    {
      return bound.ToUniversalTime ().ToString ("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Get the distance to a given date/time
    /// 
    /// The object must not be empty
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public TimeSpan GetDistance (DateTime dateTime)
    {
      if (this.IsEmpty ()) {
        Debug.Assert (!this.IsEmpty ());
        log.Fatal ($"GetDistance: empty range => fallback is 0s. StackTrace: {System.Environment.StackTrace}");
        return TimeSpan.FromTicks (0);
      }

      if (this.ContainsElement (dateTime)) {
        return TimeSpan.FromTicks (0);
      }

      UtcDateTimeRange fictiveRange = new UtcDateTimeRange (dateTime, dateTime, "[]");
      if (this.IsAdjacentTo (fictiveRange)) {
        return TimeSpan.FromTicks (0);
      }
      else if (this.IsStrictlyLeftOf (fictiveRange)) {
        Debug.Assert (this.Upper.HasValue);
        return dateTime.Subtract (this.Upper.Value);
      }
      else {  // this.IsStrictlyRightOf (fictiveRange))
        Debug.Assert (this.IsStrictlyRightOf (fictiveRange));
        Debug.Assert (this.Lower.HasValue);
        return this.Lower.Value.Subtract (dateTime);
      }
    }

    /// <summary>
    /// Convert a UTC range to a local range
    /// </summary>
    /// <returns></returns>
    public LocalDateTimeRange ToLocalTime ()
    {
      return new LocalDateTimeRange (this.Lower, this.Upper, this.LowerInclusive, this.UpperInclusive);
    }

    /// <summary>
    /// Return all the Utc ranges that are made from the intersection with a time period
    /// </summary>
    /// <param name="timePeriod"></param>
    /// <returns></returns>
    public IList<UtcDateTimeRange> Intersects (TimePeriodOfDay timePeriod)
    {
      if (!this.Lower.HasValue || !this.Upper.HasValue) {
        log.Error ("Intersects: an infinite range can't intersect a time period");
        throw new ArgumentException ("timePeriod");
      }

      LocalDateTimeRange localRange = this.ToLocalTime ();
      Debug.Assert (localRange.Lower.HasValue);
      Debug.Assert (localRange.Upper.HasValue);

      IList<UtcDateTimeRange> result = new List<UtcDateTimeRange> ();
      for (DateTime date = localRange.Lower.Value.Date;
           date <= localRange.Upper.Value.Date;
           date = date.AddDays (1).Date) {
        result.Add (localRange.Intersects (date, timePeriod).ToUniversalTime ());
      }
      return result;
    }

    /// <summary>
    /// Intersect this date/time range with the specified date and time period
    /// </summary>
    /// <param name="date">a date, not a day with a cut-off</param>
    /// <param name="timePeriod"></param>
    /// <returns></returns>
    public UtcDateTimeRange Intersects (DateTime date, TimePeriodOfDay timePeriod)
    {
      LocalDateTimeRange localRange = this.ToLocalTime ();
      return localRange.Intersects (date, timePeriod).ToUniversalTime ();
    }
  }
}
