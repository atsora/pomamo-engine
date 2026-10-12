// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Table a cnc value comes from (view cncvalueall)
  /// </summary>
  public enum CncValueSource
  {
    /// <summary>
    /// Table cncvalue
    /// </summary>
    CncValue = 0,
    /// <summary>
    /// Table cncdataperiod
    /// </summary>
    CncDataPeriod = 1,
    /// <summary>
    /// Table cncdataperiodarchive
    /// </summary>
    CncDataPeriodArchive = 2,
  }

  /// <summary>
  /// Read-only cnc value, from table cncvalue or from table cncdataperiod (and its archive),
  /// with the properties of a cnc value (view cncvalueall)
  /// </summary>
  public interface ICncValueData : IWithDateTimeRange
  {
    /// <summary>
    /// Id, unique only with <see cref="Source"/>
    /// </summary>
    long Id { get; }

    /// <summary>
    /// Table the value comes from
    /// </summary>
    CncValueSource Source { get; }

    /// <summary>
    /// Associated machine module
    /// </summary>
    IMachineModule MachineModule { get; }

    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }

    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    DateTime Begin { get; }

    /// <summary>
    /// End UTC date/time
    /// </summary>
    DateTime End { get; }

    /// <summary>
    /// Length
    /// </summary>
    TimeSpan Length { get; }

    /// <summary>
    /// String value
    /// </summary>
    string String { get; }

    /// <summary>
    /// Int value
    /// </summary>
    int? Int { get; }

    /// <summary>
    /// Double value: the average, the maximum, the sum or the last value according to the aggregation type of the field
    /// </summary>
    double? Double { get; }

    /// <summary>
    /// String, Int, Double or Boolean value according to the type of the field
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Deviation: standard deviation (Average) or max - min (Max)
    /// </summary>
    double? Deviation { get; }

    /// <summary>
    /// The value was interrupted
    /// </summary>
    bool Stopped { get; }

    /// <summary>
    /// Number of values (cncdataperiod only)
    /// </summary>
    int? Count { get; }

    /// <summary>
    /// Minimum value (cncdataperiod only)
    /// </summary>
    double? Min { get; }

    /// <summary>
    /// Maximum value (cncdataperiod only)
    /// </summary>
    double? Max { get; }

    /// <summary>
    /// Raw json value (cncdataperiod only)
    /// </summary>
    string JsonValue { get; }

    /// <summary>
    /// No value
    /// </summary>
    /// <returns></returns>
    bool IsEmpty ();
  }
}
