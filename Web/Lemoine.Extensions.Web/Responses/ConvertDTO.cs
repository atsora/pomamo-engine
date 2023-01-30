// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Globalization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// Description of ConvertDTO.
  /// </summary>
  public class ConvertDTO
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ConvertDTO).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConvertDTO ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Convert a day to IsoString with T
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static string DayToIsoString (DateTime day)
    {
      return DayToIsoString (new LowerBound<DateTime> (day));
    }

    /// <summary>
    /// Convert a day to IsoString with T
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public static string DayToIsoString (IBound<DateTime> day)
    {
      if (day.HasValue) {
        Debug.Assert (day.Value.Kind != DateTimeKind.Utc);
        string dayString = day.Value.ToString ("yyyy'-'MM'-'dd");
        return dayString;
      }
      else { // -oo or +oo
        switch (day.BoundType) {
        case BoundType.Lower:
          return "-oo";
        case BoundType.Upper:
          return "+oo";
        default:
          throw new InvalidOperationException ("Unknown bound type");
        }
      }
    }

    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoString (DateTime utcDateTime)
    {
      return DateTimeUtcToIsoString (new LowerBound<DateTime> (utcDateTime));
    }

    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoString (IBound<DateTime> utcDateTime)
    {
      if (utcDateTime.HasValue) {
        Debug.Assert (utcDateTime.Value.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
        string dateString = utcDateTime.Value.ToString ("s") + "Z";
        return dateString;
      }
      else { // -oo or +oo
        switch (utcDateTime.BoundType) {
        case BoundType.Lower:
          return "-oo";
        case BoundType.Upper:
          return "+oo";
        default:
          throw new InvalidOperationException ("Unknown bound type");
        }
      }
    }

    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z and milli-seconds
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoStringMs (DateTime utcDateTime)
    {
      return DateTimeUtcToIsoStringMs (new LowerBound<DateTime> (utcDateTime));
    }

    /// <summary>
    /// Convert an UTC DateTime? to IsoString with T and Z and milli-seconds
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeUtcToIsoStringMs (IBound<DateTime> utcDateTime)
    {
      if (utcDateTime.HasValue) {
        Debug.Assert (utcDateTime.Value.Kind != DateTimeKind.Local); // Undefined peut venir de la BDD
        string dateString = utcDateTime.Value.ToString ("s")
          + "." + utcDateTime.Value.Millisecond.ToString ("D3") // ms
          + "Z";
        return dateString;
      }
      else { // -oo or +oo
        switch (utcDateTime.BoundType) {
        case BoundType.Lower:
          return "-oo";
        case BoundType.Upper:
          return "+oo";
        default:
          throw new InvalidOperationException ("Unknown bound type");
        }
      }
    }

    /// <summary>
    /// Convert a local DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static string DateTimeLocalToIsoString (DateTime utcDate)
    {
      return DateTimeLocalToIsoString (new LowerBound<DateTime> (utcDate));
    }

    /// <summary>
    /// Convert a local DateTime? to IsoString with T and Z
    /// </summary>
    /// <param name="utcDate"></param>
    /// <returns></returns>
    public static string DateTimeLocalToIsoString (IBound<DateTime> utcDate)
    {
      if (utcDate.HasValue) {
        Debug.Assert (utcDate.Value.Kind != DateTimeKind.Utc); // Undefined peut venir de la BDD
        if (DateTimeKind.Utc == utcDate.Value.Kind) {
          log.Warn ($"DateTimeLocalToIsoString: convert UTC time {utcDate.Value} into local");
          string dateString = utcDate.Value.ToLocalTime ().ToString ("s");
          return dateString;
        }
        else {
          string dateString = utcDate.Value.ToString ("s");
          return dateString;
        }
      }
      else { // -oo or +oo
        switch (utcDate.BoundType) {
        case BoundType.Lower:
          return "-oo";
        case BoundType.Upper:
          return "+oo";
        default:
          throw new InvalidOperationException ("Unknown bound type");
        }
      }
    }

    /// <summary>
    /// Convert a local DateTime? to IsoString with T and Z and milli-seconds
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeLocalToIsoStringMs (DateTime utcDateTime)
    {
      return DateTimeLocalToIsoStringMs (new LowerBound<DateTime> (utcDateTime));
    }

    /// <summary>
    /// Convert a local DateTime? to IsoString with T and Z and milli-seconds
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static string DateTimeLocalToIsoStringMs (IBound<DateTime> utcDateTime)
    {
      if (utcDateTime.HasValue) {
        Debug.Assert (utcDateTime.Value.Kind != DateTimeKind.Utc); // Undefined peut venir de la BDD
        if (DateTimeKind.Utc == utcDateTime.Value.Kind) {
          log.WarnFormat ("DateTimeLocalToIsoString: " +
                          "convert UTC time {0} into local",
                          utcDateTime.Value);
          DateTime localTime = utcDateTime.Value.ToLocalTime ();
          string dateString = localTime.ToString ("s")
            + "." + localTime.Millisecond.ToString ("D3") // ms
            ;
          return dateString;
        }
        else {
          string dateString = utcDateTime.Value.ToString ("s")
            + "." + utcDateTime.Value.Millisecond.ToString ("D3") // ms
            ;
          return dateString;
        }
      }
      else { // -oo or +oo
        switch (utcDateTime.BoundType) {
        case BoundType.Lower:
          return "-oo";
        case BoundType.Upper:
          return "+oo";
        default:
          throw new InvalidOperationException ("Unknown bound type");
        }
      }
    }

    /// <summary>
    /// Convert an ISO String to a date/time bound in UTC
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Bound<DateTime> IsoStringToDateTimeUtc (string s)
    {
      Debug.Assert (!string.IsNullOrEmpty (s));

      if (s.Equals ("-oo", StringComparison.InvariantCultureIgnoreCase)) {
        return new LowerBound<DateTime> (null);
      }
      else if (s.Equals ("+oo", StringComparison.InvariantCultureIgnoreCase)) {
        return new UpperBound<DateTime> (null);
      }
      else {
        IFormatProvider provider = CultureInfo.InvariantCulture;
        return System.DateTime.Parse (s, provider,
                                      DateTimeStyles.AssumeUniversal
                                      | DateTimeStyles.AdjustToUniversal);
      }
    }

    /// <summary>
    /// Convert an ISO String to a day
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Bound<DateTime> IsoStringToDay (string s)
    {
      Debug.Assert (!string.IsNullOrEmpty (s));

      if (s.Equals ("-oo", StringComparison.InvariantCultureIgnoreCase)) {
        return new LowerBound<DateTime> (null);
      }
      else if (s.Equals ("+oo", StringComparison.InvariantCultureIgnoreCase)) {
        return new UpperBound<DateTime> (null);
      }
      else {
        IFormatProvider provider = CultureInfo.InvariantCulture;
        var day = System.DateTime.Parse (s, provider);
        day = new DateTime (day.Year, day.Month, day.Day);
        return day;
      }
    }
    #endregion // Methods
  }
}
