// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Lemoine.Conversion.Json;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.TimeConfigs
{
  /// <summary>
  /// Get the values from a fixed configuration.
  /// <item>manually set</item>
  /// <item>from config keys Stamping.Time.Factor and Stamping.Time.MilestoneFrequency</item>
  /// </summary>
  public class TimeConfig: ITimeConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (TimeConfig).FullName);

    /// <summary>
    /// <see cref="ITimeConfig"/>
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public double? TimeFactor { get; set; } = null;

    /// <summary>
    /// <see cref="ITimeConfig"/>
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter (typeof (NullableTimeSpanJsonConverter))]
    public TimeSpan? MilestoneTriggerFrequency { get; set; } = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public TimeConfig ()
    {
    }

    /// <summary>
    /// <see cref="ITimeConfig"/>
    /// </summary>
    /// <returns></returns>
    public double GetTimeFactor () => TimeFactor ?? Lemoine.Info.ConfigSet.LoadAndGet ("Stamping.Time.Factor", 1.0);

    /// <summary>
    /// <see cref="ITimeConfig"/>
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetMilestoneTriggerFrequency () => MilestoneTriggerFrequency ?? Lemoine.Info.ConfigSet.LoadAndGet ("Stamping.Time.MilestoneFrequency", TimeSpan.FromMinutes (2));
  }
}
