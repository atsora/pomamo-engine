// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Time-stamped cnc data sample (table cncdatasample, high frequency)
  ///
  /// Either a double value or a raw json value
  /// </summary>
  public readonly struct CncDataSampleValue
  {
    /// <summary>
    /// UTC date/time, millisecond precision
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    /// Numerical value, null in case of a json value
    /// </summary>
    public double? DoubleValue { get; }

    /// <summary>
    /// Raw json value, null in case of a numerical value
    ///
    /// Note: the .NET type to deserialize it could be stored later in the field table
    /// (today the reason data use an extension for that)
    /// </summary>
    public string JsonValue { get; }

    /// <summary>
    /// Constructor for a numerical value
    /// </summary>
    /// <param name="dateTime">UTC date/time, truncated to the millisecond</param>
    /// <param name="value"></param>
    public CncDataSampleValue (DateTime dateTime, double value)
    {
      this.DateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      this.DoubleValue = value;
      this.JsonValue = null;
    }

    /// <summary>
    /// Constructor for a json value
    /// </summary>
    /// <param name="dateTime">UTC date/time, truncated to the millisecond</param>
    /// <param name="jsonValue">raw json, not null</param>
    public CncDataSampleValue (DateTime dateTime, string jsonValue)
    {
      this.DateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      this.DoubleValue = null;
      this.JsonValue = jsonValue ?? throw new ArgumentNullException (nameof (jsonValue));
    }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"{this.DateTime:o}={(object)this.DoubleValue ?? this.JsonValue}";
  }

  /// <summary>
  /// Values of a field in a batch of samples
  /// </summary>
  public sealed class CncDataSampleSeries
  {
    /// <summary>
    /// Field
    /// </summary>
    public IField Field { get; }

    /// <summary>
    /// Values, ordered by date/time
    /// </summary>
    public IReadOnlyList<CncDataSampleValue> Values { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="values">not null, ordered by date/time</param>
    public CncDataSampleSeries (IField field, IReadOnlyList<CncDataSampleValue> values)
    {
      this.Field = field ?? throw new ArgumentNullException (nameof (field));
      this.Values = values ?? throw new ArgumentNullException (nameof (values));
    }
  }
}
