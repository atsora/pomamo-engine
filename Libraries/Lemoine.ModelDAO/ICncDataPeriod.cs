// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Modifiable cnc data period of table cncdataperiod (to replace ICncValue)
  ///
  /// The table is partitioned by machine module, then by week:
  /// <item>Begin and MachineModule can't be changed. Another begin requires a new period (a new id)</item>
  /// <item>A period never crosses a week bound: End can't be after <see cref="MaxEnd"/></item>
  /// </summary>
  public interface ICncDataPeriod
    : ICncDataPeriodValue
    , IVersionable<long>
    , IPartitionedByMachineModule
  {
    /// <summary>
    /// Id (both <see cref="ICncDataPeriodValue.Id"/> and the id of <see cref="IVersionable{ID}"/>)
    /// </summary>
    new long Id { get; }

    /// <summary>
    /// Associated field
    /// </summary>
    IField Field { get; }

    /// <summary>
    /// End UTC date/time, truncated to the millisecond
    ///
    /// An exception is raised if it is before Begin or after <see cref="MaxEnd"/>
    /// </summary>
    new DateTime End { get; set; }

    /// <summary>
    /// Week bound the period can't cross (not persistent)
    /// </summary>
    DateTime MaxEnd { get; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.Stopped"/>
    /// </summary>
    new bool Stopped { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.Count"/>
    /// </summary>
    new int? Count { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.Average"/>
    /// </summary>
    new double? Average { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.Min"/>
    /// </summary>
    new double? Min { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.Max"/>
    /// </summary>
    new double? Max { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.StdDev"/>
    /// </summary>
    new double? StdDev { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.LastValue"/>
    /// </summary>
    new double? LastValue { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.StringValue"/>
    /// </summary>
    new string StringValue { get; set; }

    /// <summary>
    /// <see cref="ICncDataPeriodValue.JsonValue"/>
    /// </summary>
    new string JsonValue { get; set; }

    /// <summary>
    /// Current statistics, null if there is no numerical value yet
    ///
    /// Setting it updates Count, Average, Min, Max, StdDev and LastValue
    /// </summary>
    CncDataStatistics? Statistics { get; set; }

    /// <summary>
    /// Extend the period up to end and combine the statistics of a new batch of values
    /// </summary>
    /// <param name="end">UTC date/time, after Begin and not after <see cref="MaxEnd"/></param>
    /// <param name="statistics">statistics of the values between the current End and end</param>
    void Extend (DateTime end, CncDataStatistics statistics);
  }
}
