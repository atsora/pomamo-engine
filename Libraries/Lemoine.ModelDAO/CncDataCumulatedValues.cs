// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.ModelDAO;

namespace Lemoine.Model
{
  /// <summary>
  /// Read the cumulated totals of the fields with an aggregation of kind
  /// <see cref="CncDataPeriodAggregationKind.Counter"/> or <see cref="CncDataPeriodAggregationKind.Integral"/>
  /// (periods with Min = total at the begin and Max = total at the end), from cncdataperiod and its archive
  /// </summary>
  public static class CncDataCumulatedValues
  {
    static readonly TimeSpan DEFAULT_LOOK_BACK = TimeSpan.FromDays (7);

    /// <summary>
    /// Cumulated total of a period at a date/time, interpolated between the total at its begin and the one at its end
    /// </summary>
    /// <param name="period">not null</param>
    /// <param name="at">UTC, in the period</param>
    /// <returns>null if the period has no total</returns>
    public static double? Interpolate (ICncDataPeriodValue period, DateTime at)
    {
      var end = period.Max ?? period.LastValue;
      if (!end.HasValue) {
        return null;
      }
      var begin = period.Min ?? end.Value;
      if ((period.End <= period.Begin) || (period.End <= at)) {
        return end.Value;
      }
      if (at <= period.Begin) {
        return begin;
      }
      return begin + (end.Value - begin) * at.Subtract (period.Begin).Ticks / period.End.Subtract (period.Begin).Ticks;
    }

    /// <summary>
    /// Cumulated total of a field at a date/time:
    /// interpolated in the period that contains it, else the total at the end of the last previous period
    /// (no increase is known during a gap or a stop)
    /// </summary>
    /// <param name="dao">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="at">UTC</param>
    /// <param name="lookBack">maximum duration before at to look for a previous period, default 7 days</param>
    /// <returns>null if no period is found</returns>
    public static double? GetCumulatedValue (this ICncDataPeriodDAO dao, IMachineModule machineModule, IField field, DateTime at, TimeSpan? lookBack = null)
    {
      var range = new UtcDateTimeRange (at.Subtract (lookBack ?? DEFAULT_LOOK_BACK), at, "[]");
      var periods = dao.FindValuesOverlapsRange (machineModule, field, range);
      var containing = periods
        .Where (p => (p.Begin <= at) && (at <= p.End))
        .OrderByDescending (p => p.Begin)
        .FirstOrDefault ();
      if (null != containing) {
        return Interpolate (containing, at);
      }
      var previous = periods
        .Where (p => p.End <= at)
        .OrderByDescending (p => p.End)
        .FirstOrDefault ();
      return (null == previous) ? (double?)null : Interpolate (previous, previous.End);
    }

    /// <summary>
    /// Increase of the cumulated total of a field between two date/times (consumption, distance...)
    /// </summary>
    /// <param name="dao">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="from">UTC</param>
    /// <param name="to">UTC</param>
    /// <param name="lookBack">see <see cref="GetCumulatedValue"/></param>
    /// <returns>null if a total is unknown</returns>
    public static double? GetIncrease (this ICncDataPeriodDAO dao, IMachineModule machineModule, IField field, DateTime from, DateTime to, TimeSpan? lookBack = null)
    {
      var end = dao.GetCumulatedValue (machineModule, field, to, lookBack);
      var begin = dao.GetCumulatedValue (machineModule, field, from, lookBack);
      return (end.HasValue && begin.HasValue) ? end.Value - begin.Value : (double?)null;
    }
  }
}
