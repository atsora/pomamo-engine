// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Globalization;

using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// UTC Date/time range
  /// </summary>
  [Serializable]
  public class LocalDateTimeRange : Range<DateTime>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (LocalDateTimeRange).FullName);

    /// <summary>
    /// Duration of the range
    /// 
    /// null if Upper or Lower is oo
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get { return this.ToUniversalTime ().Duration; }
    }

    /// <summary>
    /// Constructor for an empty range
    /// </summary>
    public LocalDateTimeRange ()
      : base ()
    { }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public LocalDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToLocalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToLocalTime () : default (DateTime?)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="range"></param>
    public LocalDateTimeRange (Range<DateTime> range)
      : base (range.IsEmpty (),
              new LowerBound<DateTime> ((!range.IsEmpty () && range.Lower.HasValue) ? range.Lower.Value.ToLocalTime () : default (DateTime?)),
              new UpperBound<DateTime> ((!range.IsEmpty () && range.Upper.HasValue) ? range.Upper.Value.ToLocalTime () : default (DateTime?)),
              range.LowerInclusive,
              range.UpperInclusive)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    public LocalDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end, bool lowerInclusive, bool upperInclusive)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToLocalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToLocalTime () : default (DateTime?)),
              lowerInclusive, upperInclusive)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="inclusivity"></param>
    public LocalDateTimeRange (LowerBound<DateTime> begin, UpperBound<DateTime> end, string inclusivity)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToLocalTime () : default (DateTime?)),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToLocalTime () : default (DateTime?)),
              inclusivity)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="inclusivity"></param>
    public LocalDateTimeRange (DateTime? begin, DateTime? end, string inclusivity)
      : base (new LowerBound<DateTime> (begin.HasValue ? begin.Value.ToLocalTime () : begin),
              new UpperBound<DateTime> (end.HasValue ? end.Value.ToLocalTime () : end),
              inclusivity)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="s"></param>
    public LocalDateTimeRange (string s)
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
                                    DateTimeStyles.AssumeLocal);
    }

    /// <summary>
    /// Convert a Date/Time to a string in UTC
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected override string ConvertBoundToString (DateTime bound)
    {
      return bound.ToLocalTime ().ToString ("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Convert a local range to a range in UTC
    /// </summary>
    /// <returns></returns>
    public UtcDateTimeRange ToUniversalTime ()
    {
      return new UtcDateTimeRange (this.Lower, this.Upper, this.LowerInclusive, this.UpperInclusive);
    }

    /// <summary>
    /// Intersect this date/time range with the specified date and time period
    /// </summary>
    /// <param name="date">a date, not a day with a cut-off</param>
    /// <param name="timePeriod"></param>
    /// <returns></returns>
    public LocalDateTimeRange Intersects (DateTime date, TimePeriodOfDay timePeriod)
    {
      LocalDateTimeRange dateRange = new LocalDateTimeRange (date, date.AddDays (1));
      LocalDateTimeRange dateIntersection = new LocalDateTimeRange (this.Intersects (dateRange));

      if (timePeriod.IsFullDay ()) {
        return dateIntersection;
      }

      if (dateIntersection.IsEmpty ()) {
        log.DebugFormat ("Intersects: " +
                         "the date range {0} does not intersect the local range {1} " +
                         "=> return an empty period",
                         dateRange, this);
        return dateIntersection; // empty
      }

      // There is an intersection of the range with date
      // A consequence is the bounds are finite
      Debug.Assert (dateIntersection.Lower.HasValue);
      Debug.Assert (dateIntersection.Upper.HasValue);

      // Check if there is an intersection with the times
      TimePeriodOfDay dateIntersectionPeriodOfDay = new TimePeriodOfDay (dateIntersection.Lower.Value.TimeOfDay,
                                                                         dateIntersection.Upper.Value.TimeOfDay);
      if (!dateIntersectionPeriodOfDay.Overlaps (timePeriod)) {
        log.DebugFormat ("Intersects: " +
                         "the two time periods {0} and {1} do not overlap with each other " +
                         "=> return an empty period",
                         timePeriod, dateIntersectionPeriodOfDay);
        return new LocalDateTimeRange ();
      }
      else {
        TimePeriodOfDay timePeriodIntersection = dateIntersectionPeriodOfDay.Intersects (timePeriod);
        DateTime begin = date.Add (timePeriodIntersection.Begin);
        DateTime end = date.Add (timePeriodIntersection.EndOffset);
        return new LocalDateTimeRange (begin, end);
      }
    }
  }
}
