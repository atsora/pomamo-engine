// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.CncValue
{
  /// <summary>
  /// Summary of the cnc data of a field on a period, for example an execution of a sequence (see <see cref="CncDataSummary.FromSequenceSlot"/>)
  /// </summary>
  public sealed class CncDataSummaryValue
  {
    /// <summary>
    /// Field
    /// </summary>
    public IField Field { get; set; }

    /// <summary>
    /// Summarized period
    /// </summary>
    public UtcDateTimeRange Range { get; set; }

    /// <summary>
    /// Duration with some data (above the effective threshold), in the range
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Average weighted by the duration (from the periods), null if no data
    /// </summary>
    public double? Average { get; set; }

    /// <summary>
    /// Minimum (from the samples if any, else from the periods)
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// Maximum (from the samples if any, else from the periods)
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// Integral of the value over the time in seconds (for example the work of the spindle load), from the periods
    /// </summary>
    public double? Work { get; set; }

    /// <summary>
    /// Number of values (prorated for the periods that cross a bound of the range)
    /// </summary>
    public double? Count { get; set; }

    /// <summary>
    /// Median, from the samples (null if no sample)
    /// </summary>
    public double? Median { get; set; }

    /// <summary>
    /// 95th percentile, from the samples (null if no sample)
    /// </summary>
    public double? P95 { get; set; }

    /// <summary>
    /// Time spent above the high threshold (from the samples if any, else from the periods), null if no threshold
    /// </summary>
    public TimeSpan? TimeAboveHigh { get; set; }

    /// <summary>
    /// Number of samples used for the median and the percentile
    /// </summary>
    public int SampleCount { get; set; }

    /// <summary>
    /// Counter, Integral: increase of the cumulated total (consumption, distance), else null
    /// </summary>
    public double? Increase { get; set; }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    public override string ToString () =>
      $"[CncDataSummary {this.Field?.Code} {this.Range} avg={this.Average} p95={this.P95} max={this.Max} increase={this.Increase}]";
  }

  /// <summary>
  /// Business request: summary of the cnc data of a field on a period, computed on demand from table cncdataperiod
  /// (and its archive) and from the samples of table cncdatasample (median, percentile), without any stored summary.
  ///
  /// With a cut of the cnc values at each change of sequence or tool (CutCncValueOnChange in the acquisition),
  /// no period crosses a bound of an execution, so that the summary of an execution is exact
  /// </summary>
  public sealed class CncDataSummary
    : IRequest<CncDataSummaryValue>
  {
    static readonly string CURRENT_DELAY_KEY = "Business.CncValue.CncDataSummary.CurrentDelay";
    static readonly TimeSpan CURRENT_DELAY_DEFAULT = TimeSpan.FromMinutes (2); // Data that may still change

    readonly IMachineModule m_machineModule;
    readonly IField m_field;
    readonly UtcDateTimeRange m_range;
    readonly double? m_effectiveThreshold;
    readonly double? m_highThreshold;

    readonly ILog log = LogManager.GetLogger (typeof (CncDataSummary).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range">with a lower bound (without any upper bound: up to now)</param>
    /// <param name="effectiveThreshold">default: cnc data parameter EffectiveThreshold of the field</param>
    /// <param name="highThreshold">default: cnc data parameter HighThreshold of the field</param>
    public CncDataSummary (IMachineModule machineModule, IField field, UtcDateTimeRange range,
      double? effectiveThreshold = null, double? highThreshold = null)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);
      Debug.Assert (range.Lower.HasValue);

      m_machineModule = machineModule;
      m_field = field;
      m_range = range;
      m_effectiveThreshold = effectiveThreshold ?? field.GetCncDataParameter<double?> (CncDataParameterNames.EffectiveThreshold);
      m_highThreshold = highThreshold ?? field.GetCncDataParameter<double?> (CncDataParameterNames.HighThreshold);
      log = LogManager.GetLogger ($"{typeof (CncDataSummary).FullName}.{machineModule.Id}.{field.Id}");
    }

    /// <summary>
    /// Summary of an execution of a sequence
    /// </summary>
    /// <param name="sequenceSlot">not null</param>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public static CncDataSummary FromSequenceSlot (ISequenceSlot sequenceSlot, IField field) =>
      new CncDataSummary (sequenceSlot.MachineModule, field, sequenceSlot.DateTimeRange);

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public CncDataSummaryValue Get ()
    {
      var range = GetBoundedRange ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Business.CncValue.CncDataSummary")) {
        var kind = m_field.GetPeriodAggregationKind ();
        if ((CncDataPeriodAggregationKind.Counter == kind) || (CncDataPeriodAggregationKind.Integral == kind)) {
          var increase = ModelDAOHelper.DAOFactory.CncDataPeriodDAO
            .GetIncrease (m_machineModule, m_field, range.Lower.Value, range.Upper.Value);
          return new CncDataSummaryValue {
            Field = m_field,
            Range = range,
            Duration = range.Duration ?? TimeSpan.Zero,
            Increase = increase,
          };
        }
        var periods = ModelDAOHelper.DAOFactory.CncDataPeriodDAO
          .FindValuesOverlapsRange (m_machineModule, m_field, range);
        IList<CncDataSampleValue> samples = new List<CncDataSampleValue> ();
        if ((FieldType.Double == m_field.Type) || (FieldType.Int32 == m_field.Type)) {
          samples = ModelDAOHelper.DAOFactory.CncDataSampleDAO
            .FindInRange (m_machineModule, m_field, range);
        }
        return Compute (m_field, range, periods, samples, m_effectiveThreshold, m_highThreshold);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public async Task<CncDataSummaryValue> GetAsync () => await Task.FromResult (Get ());

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey () =>
      $"Business.CncValue.CncDataSummary.{m_machineModule.Id}.{m_field.Id}.{m_range}.{m_effectiveThreshold}.{m_highThreshold}";

    /// <summary>
    /// <see cref="IRequest{T}"/>: short for a recent period whose data may still change, long for a past one
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (CncDataSummaryValue data)
    {
      var currentDelay = Lemoine.Info.ConfigSet.LoadAndGet (CURRENT_DELAY_KEY, CURRENT_DELAY_DEFAULT);
      if (!m_range.Upper.HasValue || (DateTime.UtcNow.Subtract (currentDelay) < m_range.Upper.Value)) {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else if (DateTime.UtcNow.Subtract (TimeSpan.FromDays (1)) < m_range.Upper.Value) {
        return CacheTimeOut.PastLong.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.OldLong.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<CncDataSummaryValue> data) => true;
    #endregion // IRequest implementation

    UtcDateTimeRange GetBoundedRange () =>
      m_range.Upper.HasValue ? m_range : new UtcDateTimeRange (m_range.Lower.Value, DateTime.UtcNow);

    /// <summary>
    /// Compute a summary from the periods and the samples of a field
    /// <item>periods: average weighted by the overlap duration (the average of a statistics period, else its last value),
    ///       work, duration, prorated count; minimum and maximum if there is no sample</item>
    /// <item>samples: median, 95th percentile (linear interpolation), minimum, maximum, time above the high threshold
    ///       (number of samples above it * median interval between two samples)</item>
    /// The values below the effective threshold are not considered
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="range">bounded</param>
    /// <param name="periods">not null</param>
    /// <param name="samples">not null</param>
    /// <param name="effectiveThreshold"></param>
    /// <param name="highThreshold"></param>
    /// <returns></returns>
    public static CncDataSummaryValue Compute (IField field, UtcDateTimeRange range,
      IEnumerable<ICncDataPeriodValue> periods, IEnumerable<CncDataSampleValue> samples,
      double? effectiveThreshold, double? highThreshold)
    {
      var result = new CncDataSummaryValue {
        Field = field,
        Range = range,
      };
      var begin = range.Lower.Value;
      var end = range.Upper.Value;

      // Periods
      double weighted = 0.0, seconds = 0.0, count = 0.0, high = 0.0;
      double? periodMin = null, periodMax = null;
      foreach (var period in periods) {
        var v = period.Average ?? period.LastValue;
        if (!v.HasValue || (effectiveThreshold.HasValue && (v.Value < effectiveThreshold.Value))) {
          continue;
        }
        var lower = (period.Begin < begin) ? begin : period.Begin;
        var upper = (end < period.End) ? end : period.End;
        if (upper < lower) {
          continue;
        }
        var d = upper.Subtract (lower).TotalSeconds;
        var length = period.End.Subtract (period.Begin).TotalSeconds;
        weighted += v.Value * d;
        seconds += d;
        count += (period.Count ?? 1) * ((0 < length) ? d / length : 1.0);
        if (highThreshold.HasValue && (highThreshold.Value < v.Value)) {
          high += d;
        }
        var min = period.Min ?? v.Value;
        var max = period.Max ?? v.Value;
        periodMin = periodMin.HasValue ? Math.Min (periodMin.Value, min) : min;
        periodMax = periodMax.HasValue ? Math.Max (periodMax.Value, max) : max;
      }
      result.Duration = TimeSpan.FromSeconds (seconds);
      if (0 < seconds) {
        result.Average = weighted / seconds;
        result.Work = weighted;
        result.Count = count;
      }
      result.Min = periodMin;
      result.Max = periodMax;
      if (highThreshold.HasValue) {
        result.TimeAboveHigh = TimeSpan.FromSeconds (high);
      }

      // Samples
      var allSamples = samples
        .Where (s => s.DoubleValue.HasValue && (begin <= s.DateTime) && (s.DateTime < end))
        .OrderBy (s => s.DateTime)
        .ToList ();
      var values = allSamples
        .Select (s => s.DoubleValue.Value)
        .Where (x => !effectiveThreshold.HasValue || (effectiveThreshold.Value <= x))
        .OrderBy (x => x)
        .ToList ();
      result.SampleCount = values.Count;
      if (0 < values.Count) {
        result.Median = Percentile (values, 0.5);
        result.P95 = Percentile (values, 0.95);
        result.Min = values.First ();
        result.Max = values.Last ();
        if (highThreshold.HasValue) {
          var intervals = allSamples
            .Zip (allSamples.Skip (1), (a, b) => b.DateTime.Subtract (a.DateTime).TotalSeconds)
            .OrderBy (x => x)
            .ToList ();
          var interval = (0 < intervals.Count) ? intervals[intervals.Count / 2] : 0.0;
          result.TimeAboveHigh = TimeSpan.FromSeconds (values.Count (x => highThreshold.Value < x) * interval);
        }
      }
      return result;
    }

    /// <summary>
    /// Percentile with a linear interpolation between the closest ranks
    /// </summary>
    /// <param name="sorted">sorted values, not empty</param>
    /// <param name="p">between 0 and 1</param>
    /// <returns></returns>
    static double Percentile (IList<double> sorted, double p)
    {
      var position = p * (sorted.Count - 1);
      var lower = (int)Math.Floor (position);
      var upper = (int)Math.Ceiling (position);
      return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
    }
  }
}
