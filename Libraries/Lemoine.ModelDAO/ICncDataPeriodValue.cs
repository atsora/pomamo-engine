// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Read-only cnc data period, common to the recent data (cncdataperiod)
  /// and to the archived data (cncdataperiodarchive), see the view cncdataperiodall
  /// </summary>
  public interface ICncDataPeriodValue : IWithDateTimeRange
  {
    /// <summary>
    /// Id (kept in the archive)
    /// </summary>
    long Id { get; }

    /// <summary>
    /// Begin UTC date/time (millisecond precision)
    /// </summary>
    DateTime Begin { get; }

    /// <summary>
    /// End UTC date/time (millisecond precision)
    /// </summary>
    DateTime End { get; }

    /// <summary>
    /// The period was ended by a stop: it must not be aggregated with the next one
    /// </summary>
    bool Stopped { get; }

    /// <summary>
    /// Number of values (numerical field)
    /// </summary>
    int? Count { get; }

    /// <summary>
    /// Average (numerical field)
    /// </summary>
    double? Average { get; }

    /// <summary>
    /// Minimum value (numerical field)
    /// </summary>
    double? Min { get; }

    /// <summary>
    /// Maximum value (numerical field)
    /// </summary>
    double? Max { get; }

    /// <summary>
    /// Population standard deviation (numerical field)
    /// </summary>
    double? StdDev { get; }

    /// <summary>
    /// Last value of the period (numerical field)
    /// </summary>
    double? LastValue { get; }

    /// <summary>
    /// String value (string field)
    /// </summary>
    string StringValue { get; }

    /// <summary>
    /// Raw json value (jsonb)
    ///
    /// Note: the .NET type to deserialize it could be stored later in the field table
    /// (today the reason data use an extension for that)
    /// </summary>
    string JsonValue { get; }

    /// <summary>
    /// The period comes from the archive (read-only)
    /// </summary>
    bool Archived { get; }
  }
}
