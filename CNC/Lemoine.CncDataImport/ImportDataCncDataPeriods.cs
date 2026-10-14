// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Lemoine.Cnc.Data;
using Lemoine.CncDataImport.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Value of a period of kind <see cref="CncDataPeriodAggregationKind.Change"/>:
  /// one of a number, a string or a raw json value
  /// </summary>
  internal readonly struct CncDataChangeValue : IEquatable<CncDataChangeValue>
  {
    public double? Number { get; }
    public string String { get; }
    public string Json { get; }

    public CncDataChangeValue (double? number, string s, string json)
    {
      this.Number = number;
      this.String = s;
      this.Json = json;
    }

    public bool Equals (CncDataChangeValue other) =>
      Nullable.Equals (this.Number, other.Number)
      && string.Equals (this.String, other.String, StringComparison.Ordinal)
      && string.Equals (this.Json, other.Json, StringComparison.Ordinal);

    public override bool Equals (object obj) => obj is CncDataChangeValue other && Equals (other);

    public override int GetHashCode () => (this.Number, this.String, this.Json).GetHashCode ();

    public override string ToString () => this.Number?.ToString (CultureInfo.InvariantCulture) ?? this.String ?? this.Json;
  }

  /// <summary>
  /// Read-only copy of a cnc data period, given to the extensions after the commit
  /// </summary>
  internal sealed class CncDataPeriodSnapshot : ICncDataPeriodValue
  {
    public long Id { get; }
    public DateTime Begin { get; }
    public DateTime End { get; }
    public bool Stopped { get; }
    public int? Count { get; }
    public double? Average { get; }
    public double? Min { get; }
    public double? Max { get; }
    public double? StdDev { get; }
    public double? LastValue { get; }
    public string StringValue { get; }
    public string JsonValue { get; }
    public bool Archived => false;
    public UtcDateTimeRange DateTimeRange => new UtcDateTimeRange (this.Begin, this.End);

    public CncDataPeriodSnapshot (ICncDataPeriodValue period)
    {
      this.Id = period.Id;
      this.Begin = period.Begin;
      this.End = period.End;
      this.Stopped = period.Stopped;
      this.Count = period.Count;
      this.Average = period.Average;
      this.Min = period.Min;
      this.Max = period.Max;
      this.StdDev = period.StdDev;
      this.LastValue = period.LastValue;
      this.StringValue = period.StringValue;
      this.JsonValue = period.JsonValue;
    }

    public override string ToString () => $"[CncDataPeriodSnapshot {this.Id} {this.Begin:o}-{this.End:o}]";
  }

  /// <summary>
  /// Conversion of the values of the cnc data queue for table cncdataperiod
  /// </summary>
  internal static class CncDataPeriodValues
  {
    /// <summary>
    /// Statistics of a value:
    /// <item>a number (or a boolean, or a numeric string): a single value</item>
    /// <item>a dictionary with the keys Count and Average, and optionally Min, Max, StandardDeviation (or StdDev), Last:
    ///       the statistics of a batch, for example from the high frequency module HighFrequencyStatistics (GetStatistics)</item>
    /// </summary>
    /// <param name="v"></param>
    /// <param name="statistics"></param>
    /// <returns></returns>
    public static bool TryGetStatistics (object v, out CncDataStatistics statistics)
    {
      if (v is IDictionary<string, object> dictionary) {
        if (TryGetDouble (dictionary, "Count", out var count) && (0 < count)
          && TryGetDouble (dictionary, "Average", out var average)) {
          var min = TryGetDouble (dictionary, "Min", out var m) ? m : average;
          var max = TryGetDouble (dictionary, "Max", out var M) ? M : average;
          var stdDev = TryGetDouble (dictionary, "StandardDeviation", out var s) ? s
            : (TryGetDouble (dictionary, "StdDev", out var s2) ? s2 : 0.0);
          var last = TryGetDouble (dictionary, "Last", out var l) ? l : average;
          try {
            statistics = new CncDataStatistics ((int)count, average, min, max, stdDev, last);
            return true;
          }
          catch (ArgumentException) {
          }
        }
        statistics = default;
        return false;
      }
      if (TryGetDouble (v, out var x)) {
        statistics = CncDataStatistics.FromValue (x);
        return true;
      }
      statistics = default;
      return false;
    }

    static bool TryGetDouble (IDictionary<string, object> dictionary, string key, out double x)
    {
      if (dictionary.TryGetValue (key, out var v)) {
        return TryGetDouble (v, out x);
      }
      x = double.NaN;
      return false;
    }

    /// <summary>
    /// Convert a number, a boolean or a numeric string into a double (not NaN)
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static bool TryGetDouble (object v, out double x)
    {
      switch (v) {
      case null:
        x = double.NaN;
        return false;
      case bool b:
        x = b ? 1.0 : 0.0;
        return true;
      case System.Text.Json.JsonElement e: // For example after a json serialization in the queue
        switch (e.ValueKind) {
        case System.Text.Json.JsonValueKind.Number:
          x = e.GetDouble ();
          return true;
        case System.Text.Json.JsonValueKind.True:
          x = 1.0;
          return true;
        case System.Text.Json.JsonValueKind.False:
          x = 0.0;
          return true;
        case System.Text.Json.JsonValueKind.String:
          return TryGetDouble (e.GetString (), out x);
        default:
          x = double.NaN;
          return false;
        }
      case string s:
        return double.TryParse (s, NumberStyles.Float, CultureInfo.InvariantCulture, out x) && !double.IsNaN (x);
      case IConvertible c when (c.GetTypeCode () != TypeCode.Object) && (c.GetTypeCode () != TypeCode.DateTime) && (c.GetTypeCode () != TypeCode.Char):
        try {
          x = c.ToDouble (CultureInfo.InvariantCulture);
          return !double.IsNaN (x);
        }
        catch (Exception) {
          x = double.NaN;
          return false;
        }
      default:
        x = double.NaN;
        return false;
      }
    }

    /// <summary>
    /// Value of a period of kind Change, according to the type of the field:
    /// <item>String: the string, or the json serialization of a structured value</item>
    /// <item>Int32, Double, Boolean: a number</item>
    /// <item>a structured value (dictionary, list...): its json serialization</item>
    /// </summary>
    /// <param name="field"></param>
    /// <param name="v"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetChangeValue (IField field, object v, out CncDataChangeValue value)
    {
      if (null == v) {
        value = default;
        return false;
      }
      if (IsStructured (v)) {
        value = new CncDataChangeValue (null, null, System.Text.Json.JsonSerializer.Serialize (v, v.GetType ()));
        return true;
      }
      switch (field.Type) {
      case FieldType.String:
        value = new CncDataChangeValue (null, Convert.ToString (v, CultureInfo.InvariantCulture), null);
        return true;
      case FieldType.Int32:
      case FieldType.Double:
      case FieldType.Boolean:
        if (TryGetDouble (v, out var x)) {
          value = new CncDataChangeValue (x, null, null);
          return true;
        }
        value = default;
        return false;
      default:
        value = new CncDataChangeValue (null, null, System.Text.Json.JsonSerializer.Serialize (v, v.GetType ()));
        return true;
      }
    }

    static bool IsStructured (object v) => !(v is string) && (v is IEnumerable);
  }

  /// <summary>
  /// Import the cnc values in table cncdataperiod (fields with the storage CncDataPeriod)
  ///
  /// According to <see cref="IFieldExtensions.GetPeriodAggregationKind(IField)"/>:
  /// <item>Change: a new period each time the value changes</item>
  /// <item>Statistics: combinable statistics. The values are accumulated during MinTime (a segment),
  ///       then the segment is combined with the current period if its average differs of at most LimitDeviation,
  ///       else (or if LimitDeviation is null, or if the period would last more than PeriodMaxDuration) a new period starts</item>
  /// <item>Custom: processed by an <see cref="Lemoine.Extensions.Cnc.IImportCncDataPeriodExtension"/></item>
  ///
  /// A new period also starts after a stop, after a gap (same rule as the cnc values), and at a week bound.
  /// The values before the end of the last period (already imported) are skipped
  /// </summary>
  internal sealed class ImportDataCncDataPeriods : IImportData
  {
    static readonly TimeSpan DEFAULT_SKIP_OLD_SECONDARY_CNC_VALUES_AFTER = TimeSpan.FromHours (2);

    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly CacheCncDataPeriod m_cache;
    readonly CacheCncValue m_cncValueCache;
    readonly Lemoine.Threading.IChecked m_caller;
    readonly IEnumerable<Lemoine.Extensions.Cnc.IImportCncDataPeriodExtension> m_extensions;
    readonly ISet<int> m_unsupportedFields = new HashSet<int> ();
    readonly IEnumerable<Lemoine.Extensions.Cnc.IImportCncDataPeriodsExtension> m_afterCommitExtensions;
    DateTime? m_writtenLower = null; // Range of the periods written by the current import
    DateTime? m_writtenUpper = null;
    readonly IList<ICncDataPeriod> m_writtenPeriods = new List<ICncDataPeriod> (); // Periods written by the current import
    readonly IList<CncDataIncrement> m_increments = new List<CncDataIncrement> (); // New numeric data of the current import

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Duration after which a secondary cnc value is considered old and thus skipped (same config as the cnc values)
    /// </summary>
    TimeSpan? SkipOldSecondaryCncValuesAfter { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="cache">not null, shared with <see cref="ImportDataStopCncDataPeriods"/></param>
    /// <param name="cncValueCache">not null, for the rule of the end of a period after a gap</param>
    /// <param name="caller">nullable</param>
    public ImportDataCncDataPeriods (IMachineModule machineModule, CacheCncDataPeriod cache, CacheCncValue cncValueCache, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      m_cache = cache;
      m_cncValueCache = cncValueCache;
      m_caller = caller;
      log = LogManager.GetLogger ($"{typeof (ImportDataCncDataPeriods).FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");

      SkipOldSecondaryCncValuesAfter = Info.ConfigSet.LoadAndGet<TimeSpan> (
        "SkipOldSecondaryCncValuesAfter", DEFAULT_SKIP_OLD_SECONDARY_CNC_VALUES_AFTER);
      if (SkipOldSecondaryCncValuesAfter.HasValue && SkipOldSecondaryCncValuesAfter.Value.TotalSeconds < 1) {
        SkipOldSecondaryCncValuesAfter = null;
      }

      m_extensions = ExtensionManager
        .GetExtensions<Lemoine.Extensions.Cnc.IImportCncDataPeriodExtension> ()
        .Where (extension => extension.Initialize (m_machineModule))
        .ToList ();
      m_afterCommitExtensions = ExtensionManager
        .GetExtensions<Lemoine.Extensions.Cnc.IImportCncDataPeriodsExtension> ()
        .Where (extension => extension.Initialize (m_machineModule))
        .ToList ();

      // Partitions of the current week (for example for a new machine module), in their own short transaction,
      // so that they are not created in the transaction of an import (see EnsurePartition)
      try {
        ModelDAOHelper.DAOFactory.CncDataPartitionDAO.CreateWeekPartitions (m_machineModule.Id, DateTime.UtcNow, 7);
      }
      catch (Exception ex) {
        log.Warn ($"ImportDataCncDataPeriods: the partitions of the current week could not be created, they will be created when needed", ex);
      }
    }

    /// <summary>
    /// Extend the range of the written periods
    /// </summary>
    void AddWritten (DateTime begin, DateTime end)
    {
      if (!m_writtenLower.HasValue || (begin < m_writtenLower.Value)) {
        m_writtenLower = begin;
      }
      if (!m_writtenUpper.HasValue || (m_writtenUpper.Value < end)) {
        m_writtenUpper = end;
      }
    }

    /// <summary>
    /// Call the extensions after the commit, outside the session.
    /// Their exceptions are logged: the data are already committed
    /// </summary>
    void NotifyAfterCommit (IField field, UtcDateTimeRange range, IList<ICncDataPeriodValue> writtenPeriods, IList<CncDataIncrement> increments)
    {
      foreach (var extension in m_afterCommitExtensions) {
        try {
          SetActive ();
          extension.AfterCommit (m_machineModule, field, range, writtenPeriods, increments);
        }
        catch (Exception ex) {
          log.Error ($"NotifyAfterCommit: exception in extension {extension} for field {field.Code} and range {range}, but the data was committed => skip it", ex);
        }
      }
    }

    void SetActive () => m_caller?.SetActive ();

    #region IImportData implementation
    /// <summary>
    /// <see cref="IImportData"/>: same key, ordered date/times
    /// </summary>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      return otherData.Key.Equals (data.Key) && (otherData.DateTime <= data.DateTime);
    }

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      Debug.Assert (0 < datas.Count);
      var key = datas[0].Key;

      IField field = null;
      UtcDateTimeRange committedRange = null;
      IList<ICncDataPeriodValue> committedPeriods = null;
      IList<CncDataIncrement> committedIncrements = null;
      m_writtenLower = null;
      m_writtenUpper = null;
      m_writtenPeriods.Clear ();
      m_increments.Clear ();
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode (key);
          if (null == field) {
            log.Error ($"ImportDatas: field code={key} is unknown => skip the record");
            return;
          }
          if (!field.Active || (field.GetCncDataStorage () != CncDataStorage.CncDataPeriod)) {
            return;
          }
          var kind = field.GetPeriodAggregationKind ();
          if (CncDataPeriodAggregationKind.None == kind) {
            return;
          }
          if (SkipOldSecondaryCncValuesAfter.HasValue
            && !field.Equals (m_machineModule.MonitoredMachine.PerformanceField)
            && SkipOldSecondaryCncValuesAfter.Value < DateTime.UtcNow.Subtract (datas[datas.Count - 1].DateTime)) {
            log.Info ($"ImportDatas: do not import {datas[datas.Count - 1]} because it is older than {SkipOldSecondaryCncValuesAfter.Value}");
            return;
          }

          using (var transaction = session.BeginTransaction ("CncData.ImportCncDataPeriod", TransactionLevel.ReadCommitted)) {
            switch (kind) {
            case CncDataPeriodAggregationKind.Change: {
                var state = m_cache.Get (field);
                foreach (var data in datas) {
                  SetActive ();
                  if (CncDataPeriodValues.TryGetChangeValue (field, data.Value, out var value)) {
                    ProcessChange (field, state, data.PreciseDateTime, value);
                  }
                  else {
                    log.Error ($"ImportDatas: value {data.Value} of {data} can't be converted for field {field.Code} of type {field.Type} => skip it");
                  }
                }
              }
              break;
            case CncDataPeriodAggregationKind.Statistics: {
                var state = m_cache.Get (field);
                foreach (var data in datas) {
                  SetActive ();
                  if (CncDataPeriodValues.TryGetStatistics (data.Value, out var statistics)) {
                    ProcessStatistics (field, state, data.PreciseDateTime, statistics);
                  }
                  else {
                    log.Error ($"ImportDatas: value {data.Value} of {data} is not numeric for field {field.Code} => skip it");
                  }
                }
              }
              break;
            case CncDataPeriodAggregationKind.Counter: {
                var state = m_cache.Get (field);
                foreach (var data in datas) {
                  SetActive ();
                  if (CncDataPeriodValues.TryGetDouble (data.Value, out var raw)) {
                    ProcessCounter (field, state, data.PreciseDateTime, raw);
                  }
                  else {
                    log.Error ($"ImportDatas: value {data.Value} of {data} is not numeric for counter field {field.Code} => skip it");
                  }
                }
              }
              break;
            case CncDataPeriodAggregationKind.Integral: {
                var state = m_cache.Get (field);
                foreach (var data in datas) {
                  SetActive ();
                  if (CncDataPeriodValues.TryGetStatistics (data.Value, out var statistics)) {
                    ProcessIntegral (field, state, data.PreciseDateTime, statistics);
                  }
                  else {
                    log.Error ($"ImportDatas: value {data.Value} of {data} is not numeric for integral field {field.Code} => skip it");
                  }
                }
              }
              break;
            case CncDataPeriodAggregationKind.Custom: {
                var extension = GetExtension (field);
                if (null != extension) {
                  extension.Import (m_machineModule, field,
                    datas.Select (d => new KeyValuePair<DateTime, object> (d.PreciseDateTime, d.Value)).ToList ());
                  // The extension writes the periods itself: range of the imported values
                  AddWritten (datas[0].PreciseDateTime, datas[datas.Count - 1].PreciseDateTime);
                }
              }
              break;
            }
            SetActive ();
            transaction.Commit ();
          }
          if (m_writtenLower.HasValue) {
            committedRange = new UtcDateTimeRange (m_writtenLower.Value, m_writtenUpper.Value, "[]");
            // Read-only copies in their state after the commit (with the ids of the new periods)
            committedPeriods = m_writtenPeriods
              .Select (p => (ICncDataPeriodValue)new CncDataPeriodSnapshot (p))
              .OrderBy (p => p.Begin)
              .ToList ();
            committedIncrements = m_increments.ToList ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ImportDatas: exception => reset the state of the field", ex);
        if (null != field) {
          Reset (field.Id);
        }
        throw;
      }

      // After the commit and outside the session
      if (null != committedRange) {
        NotifyAfterCommit (field, committedRange, committedPeriods, committedIncrements);
      }
    }
    #endregion // IImportData implementation

    /// <summary>
    /// Extension for a custom aggregation, null if none
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    internal Lemoine.Extensions.Cnc.IImportCncDataPeriodExtension GetExtension (IField field)
    {
      var extension = m_extensions.FirstOrDefault (e => e.IsSupported (field));
      if (null == extension && m_unsupportedFields.Add (field.Id)) {
        log.Error ($"GetExtension: no extension for the custom aggregation type {field.CncDataAggregationType} of field {field.Code} => its data are skipped");
      }
      return extension;
    }

    /// <summary>
    /// Forget the state of a field after a rollback
    /// </summary>
    /// <param name="fieldId"></param>
    internal void Reset (int fieldId)
    {
      m_cache.Reset (fieldId);
      foreach (var extension in m_extensions) {
        try {
          extension.Reset (m_machineModule.Id, fieldId);
        }
        catch (Exception ex) {
          log.Error ($"Reset: exception in extension {extension}", ex);
        }
      }
    }

    void Persist (ICncDataPeriod period)
    {
      ModelDAOHelper.DAOFactory.CncDataPeriodDAO.MakePersistent (period);
      AddWritten (period.Begin, period.End);
      if (!m_writtenPeriods.Any (p => object.ReferenceEquals (p, period))) {
        m_writtenPeriods.Add (period);
      }
    }

    /// <summary>
    /// Add an increment, combined with the previous one when they are contiguous and their values close enough:
    /// same rule as the periods, the value differs of at most LimitDeviation from the combined value
    /// (no combination if LimitDeviation is null) and the combined increment does not last more than PeriodMaxDuration
    /// </summary>
    void AddIncrement (IField field, DateTime begin, DateTime end, double? value, CncDataStatistics? received)
    {
      if (!value.HasValue && !received.HasValue) {
        return;
      }
      if (0 < m_increments.Count) {
        var last = m_increments[m_increments.Count - 1];
        if (field.LimitDeviation.HasValue
          && (last.End == begin)
          && last.Value.HasValue && value.HasValue
          && (Math.Abs (value.Value - last.Value.Value) <= field.LimitDeviation.Value)
          && (!field.PeriodMaxDuration.HasValue || (end.Subtract (last.Begin) <= field.PeriodMaxDuration.Value))) {
          var lastSeconds = last.Duration.TotalSeconds;
          var seconds = end.Subtract (begin).TotalSeconds;
          var combinedValue = (0 < lastSeconds + seconds)
            ? (last.Value.Value * lastSeconds + value.Value * seconds) / (lastSeconds + seconds) // Time-weighted
            : last.Value.Value;
          m_increments[m_increments.Count - 1] = new CncDataIncrement (last.Begin, end, combinedValue, Combine (last.Received, received));
          return;
        }
      }
      m_increments.Add (new CncDataIncrement (begin, end, value, received));
    }

    ICncDataPeriod Create (IField field, DateTime begin)
    {
      return ModelDAOHelper.ModelFactory.CreateCncDataPeriod (m_machineModule, field, begin);
    }

    /// <summary>
    /// Is the date/time already imported?
    /// </summary>
    bool IsAlreadyImported (CncDataPeriodState state, DateTime dateTime)
    {
      if ((null != state.Period) && (dateTime < state.Period.End)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsAlreadyImported: {dateTime} before the end of {state.Period} => skip it");
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Must the next value start a new period (no period, stop or gap)?
    /// </summary>
    bool IsNewPeriodRequired (IField field, CncDataPeriodState state, DateTime dateTime)
    {
      return (null == state.Period)
        || state.Period.Stopped
        || m_cncValueCache.IsEndCncValueRequired (field, state.Period.End, dateTime, false);
    }

    /// <summary>
    /// Extend a period up to dateTime, except after a gap (same rule as the values) or beyond the week bound
    /// </summary>
    /// <returns>the period ends at dateTime</returns>
    bool ExtendTo (IField field, ICncDataPeriod period, DateTime dateTime)
    {
      if (dateTime <= period.End) {
        return dateTime == period.End;
      }
      if (period.MaxEnd < dateTime) {
        return false;
      }
      if (m_cncValueCache.IsEndCncValueRequired (field, period.End, dateTime, false)) { // Gap
        return false;
      }
      period.End = dateTime;
      return true;
    }

    /// <summary>
    /// Stop of a field with an aggregation of kind Change or Statistics:
    /// the current period gets the Stopped property (its end remains the date/time of the last value, as in cncvalue)
    /// and the next value starts a new period
    /// </summary>
    internal void ProcessStop (IField field, CncDataPeriodState state, DateTime dateTime)
    {
      var period = state.Period;
      if ((null == period) || period.Stopped) {
        return;
      }
      period.Stopped = true;
      Persist (period);
      state.BaseStatistics = period.Statistics;
      state.SegmentBegin = period.End;
      state.SegmentStatistics = null;
      state.CutDateTime = null;
    }

    /// <summary>
    /// Cut of a field with an aggregation of kind Change or Statistics (see <see cref="ExchangeData.BuildCutCncValueExchangeData"/>):
    /// the current period ends at the cut (without the Stopped property) and the next value starts a new period at the cut,
    /// without any interruption:
    /// <item>Change: the previous value lasts from the cut up to the next value</item>
    /// <item>Statistics: the new period starts at the cut</item>
    ///
    /// Nothing is done after a gap, beyond the week bound (the next value starts a new period anyway)
    /// or if the field is stopped
    /// </summary>
    internal void ProcessCut (IField field, CncDataPeriodState state, DateTime dateTime)
    {
      dateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      var period = state.Period;
      if ((null == period) || period.Stopped) {
        return;
      }
      if (!ExtendTo (field, period, dateTime)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessCut: cut of {period} at {dateTime} after a gap, beyond the week bound or before its end => skip it");
        }
        return;
      }
      Persist (period);
      state.BaseStatistics = period.Statistics;
      state.SegmentBegin = period.End;
      state.SegmentStatistics = null;
      state.CutDateTime = dateTime;
    }

    static void SetChangeValue (ICncDataPeriod period, CncDataChangeValue value)
    {
      period.LastValue = value.Number;
      period.StringValue = value.String;
      period.JsonValue = value.Json;
    }

    static CncDataChangeValue GetChangeValue (ICncDataPeriod period) =>
      new CncDataChangeValue (period.LastValue, period.StringValue, NormalizeJson (period.JsonValue));

    /// <summary>
    /// jsonb is normalized by PostgreSQL: compare the json values after the same normalization
    /// </summary>
    static string NormalizeJson (string json)
    {
      if (null == json) {
        return null;
      }
      try {
        using (var document = System.Text.Json.JsonDocument.Parse (json)) {
          return System.Text.Json.JsonSerializer.Serialize (document.RootElement);
        }
      }
      catch (Exception) {
        return json;
      }
    }

    /// <summary>
    /// Process a value of a field with an aggregation of kind Change
    /// </summary>
    internal void ProcessChange (IField field, CncDataPeriodState state, DateTime dateTime, CncDataChangeValue value)
    {
      dateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      value = new CncDataChangeValue (value.Number, value.String, NormalizeJson (value.Json));
      if (IsAlreadyImported (state, dateTime)) {
        return;
      }

      var forceNewPeriod = false;
      if (state.CutDateTime.HasValue) { // Cut not processed yet
        var cut = state.CutDateTime.Value;
        state.CutDateTime = null;
        if ((cut < dateTime) && !IsNewPeriodRequired (field, state, dateTime)) {
          // The previous value lasts from the cut up to this value, in a new period
          var next = Create (field, cut);
          SetChangeValue (next, GetChangeValue (state.Period));
          Persist (next);
          state.Start (next, cut, null);
        }
        else { // Value at the date/time of the cut, or after a gap
          forceNewPeriod = true;
        }
      }

      if (!forceNewPeriod && !IsNewPeriodRequired (field, state, dateTime)) {
        var period = state.Period;
        // The previous value lasts up to the new value
        AddIncrement (field, period.End, dateTime, period.LastValue, null);
        // The previous value lasts up to the week bound, then continues in the next week
        while (period.MaxEnd < dateTime) {
          var weekEnd = period.MaxEnd;
          period.End = weekEnd;
          Persist (period);
          var next = Create (field, weekEnd);
          SetChangeValue (next, GetChangeValue (period));
          Persist (next);
          state.Start (next, weekEnd, null);
          period = next;
        }
        var maxDurationReached = field.PeriodMaxDuration.HasValue
          && (field.PeriodMaxDuration.Value < dateTime.Subtract (period.Begin));
        period.End = dateTime;
        Persist (period);
        if (GetChangeValue (period).Equals (value) && !maxDurationReached) {
          return;
        }
      }

      // New period
      AddIncrement (field, dateTime, dateTime, value.Number, value.Number.HasValue ? CncDataStatistics.FromValue (value.Number.Value) : (CncDataStatistics?)null);
      var newPeriod = Create (field, dateTime);
      SetChangeValue (newPeriod, value);
      Persist (newPeriod);
      state.Start (newPeriod, dateTime, null);
    }

    /// <summary>
    /// Process a value (or the statistics of a batch) of a field with an aggregation of kind Statistics
    /// </summary>
    internal void ProcessStatistics (IField field, CncDataPeriodState state, DateTime dateTime, CncDataStatistics statistics)
    {
      dateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      if (IsAlreadyImported (state, dateTime)) {
        return;
      }

      if (state.CutDateTime.HasValue) { // Cut not processed yet: the new period starts at the cut, except after a gap
        var cut = state.CutDateTime.Value;
        state.CutDateTime = null;
        var begin = dateTime;
        if ((cut < dateTime) && !IsNewPeriodRequired (field, state, dateTime)) {
          var weekStart = CncDataWeek.GetWeekStart (dateTime, (DayOfWeek)ModelDAOHelper.DAOFactory.CncDataPartitionDAO.GetWeekCutOffDay ());
          begin = (cut < weekStart) ? weekStart : cut;
        }
        AddIncrement (field, begin, dateTime, statistics.Average, statistics);
        StartStatistics (field, state, begin, dateTime, statistics);
        return;
      }

      if (IsNewPeriodRequired (field, state, dateTime)) {
        AddIncrement (field, dateTime, dateTime, statistics.Average, statistics);
        StartStatistics (field, state, dateTime, dateTime, statistics);
        return;
      }

      var period = state.Period;
      // The new values cover the time since the previous data, whatever the periods become (extended, split, week bound)
      AddIncrement (field, period.End, dateTime, statistics.Average, statistics);
      if (period.MaxEnd < dateTime) {
        // Close the period at the week bound, the value starts a period in the next week
        var weekEnd = period.MaxEnd;
        period.End = weekEnd;
        period.Statistics = Combine (state.BaseStatistics, state.SegmentStatistics);
        Persist (period);
        StartStatistics (field, state, CncDataWeek.GetWeekStart (dateTime, (DayOfWeek)ModelDAOHelper.DAOFactory.CncDataPartitionDAO.GetWeekCutOffDay ()), dateTime, statistics);
        return;
      }

      var segment = Combine (state.SegmentStatistics, statistics).Value;
      period.End = dateTime;
      var minTime = field.MinTime ?? TimeSpan.Zero;
      if (minTime <= dateTime.Subtract (state.SegmentBegin)) { // Decision
        if (!state.BaseStatistics.HasValue) { // First segment of the period
          state.BaseStatistics = segment;
          period.Statistics = segment;
        }
        else {
          var baseStatistics = state.BaseStatistics.Value;
          var split = !field.LimitDeviation.HasValue
            || (field.LimitDeviation.Value < Math.Abs (segment.Average - baseStatistics.Average))
            || (field.PeriodMaxDuration.HasValue && (field.PeriodMaxDuration.Value < dateTime.Subtract (period.Begin)));
          if (split && (period.Begin < state.SegmentBegin)) {
            // The period ends at the begin of the segment, that starts a new period
            period.End = state.SegmentBegin;
            period.Statistics = baseStatistics;
            Persist (period);
            var next = Create (field, state.SegmentBegin);
            next.End = dateTime;
            next.Statistics = segment;
            Persist (next);
            state.Period = next;
            state.BaseStatistics = segment;
            state.SegmentBegin = dateTime;
            state.SegmentStatistics = null;
            return;
          }
          else {
            state.BaseStatistics = CncDataStatistics.Combine (baseStatistics, segment);
            period.Statistics = state.BaseStatistics;
          }
        }
        state.SegmentBegin = dateTime;
        state.SegmentStatistics = null;
      }
      else {
        state.SegmentStatistics = segment;
        period.Statistics = Combine (state.BaseStatistics, segment);
      }
      Persist (period);
    }

    static readonly TimeSpan DEFAULT_CUMULATED_INTERVAL = TimeSpan.FromSeconds (30);

    /// <summary>
    /// Process a value of a field with an aggregation of kind Counter (a value that is already cumulated)
    /// </summary>
    internal void ProcessCounter (IField field, CncDataPeriodState state, DateTime dateTime, double raw)
    {
      dateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      if (IsAlreadyImported (state, dateTime)) {
        return;
      }

      // A decrease is a reset (the counter restarts from 0) or a rollover
      if (state.CounterRaw.HasValue && (raw < state.CounterRaw.Value)) {
        var rollover = field.GetCncDataParameter<double?> (CncDataParameterNames.Rollover);
        var added = rollover ?? state.CounterRaw.Value;
        log.Warn ($"ProcessCounter: counter {field.Code} decreased from {state.CounterRaw.Value} to {raw} at {dateTime} => {(rollover.HasValue ? "rollover" : "reset")}, offset + {added}");
        state.CounterOffset += added;
      }
      state.CounterRaw = raw;
      var factor = field.GetCncDataParameter (CncDataParameterNames.Factor, 1.0);
      var total = (raw + state.CounterOffset) * factor;
      ProcessCumulated (field, state, dateTime, total, 1, null);
    }

    /// <summary>
    /// Factor of an integrated value: Factor, or for a current Voltage * PowerFactor (* √3 if ThreePhase) * Factor / 1000 (A => kW)
    /// </summary>
    static double GetIntegralFactor (IField field)
    {
      var factor = field.GetCncDataParameter (CncDataParameterNames.Factor, 1.0);
      var voltage = field.GetCncDataParameter<double?> (CncDataParameterNames.Voltage);
      if (voltage.HasValue) {
        factor *= voltage.Value * field.GetCncDataParameter (CncDataParameterNames.PowerFactor, 1.0) / 1000.0;
        if (field.GetCncDataParameter (CncDataParameterNames.ThreePhase, false)) {
          factor *= Math.Sqrt (3.0);
        }
      }
      return factor;
    }

    /// <summary>
    /// Process a value (or the statistics of a batch) of a field with an aggregation of kind Integral:
    /// the value is integrated since the previous value (trapezoidal rule, or the average of a batch over its duration),
    /// except after a gap or a stop
    /// </summary>
    internal void ProcessIntegral (IField field, CncDataPeriodState state, DateTime dateTime, CncDataStatistics statistics)
    {
      dateTime = CncDataWeek.TruncateToMilliseconds (dateTime);
      if (IsAlreadyImported (state, dateTime)) {
        return;
      }

      var rate = statistics.Average * GetIntegralFactor (field);
      var total = state.CumulatedTotal ?? 0.0;
      if (state.CumulatedTotal.HasValue && state.CumulatedDateTime.HasValue && (state.CumulatedDateTime.Value < dateTime)
        && !IsNewPeriodRequired (field, state, dateTime)) {
        var timeUnit = field.GetCncDataParameter (CncDataParameterNames.TimeUnit, TimeSpan.FromHours (1));
        var duration = dateTime.Subtract (state.CumulatedDateTime.Value).TotalSeconds / timeUnit.TotalSeconds;
        var averageRate = ((1 < statistics.Count) || !state.IntegralRate.HasValue)
          ? rate // Average of a batch over its duration
          : (state.IntegralRate.Value + rate) / 2.0; // Trapezoidal rule
        total += averageRate * duration;
      }
      state.IntegralRate = rate;
      ProcessCumulated (field, state, dateTime, total, statistics.Count, rate);
    }

    /// <summary>
    /// Common part of Counter and Integral: record the cumulated total at dateTime
    /// <item>after a gap or a stop (or without any period): a new period starts at dateTime</item>
    /// <item>after a cut: the total at the cut is interpolated, the current period ends there and a new one starts there</item>
    /// <item>at each bound of the clock grid (multiples of MinTime since 00:00 UTC) and at the week bound:
    ///       the total is interpolated, the current period ends there and a new one starts there</item>
    /// </summary>
    /// <param name="field"></param>
    /// <param name="state"></param>
    /// <param name="dateTime">truncated to the millisecond</param>
    /// <param name="total">cumulated total at dateTime</param>
    /// <param name="count">number of received values</param>
    /// <param name="rate">Integral: instantaneous rate (for the json value), else null</param>
    void ProcessCumulated (IField field, CncDataPeriodState state, DateTime dateTime, double total, int count, double? rate)
    {
      var interval = ((field.MinTime ?? TimeSpan.Zero) <= TimeSpan.Zero) ? DEFAULT_CUMULATED_INTERVAL : field.MinTime.Value;
      var interrupted = IsNewPeriodRequired (field, state, dateTime);
      var period = state.Period;
      var previousDateTime = state.CumulatedDateTime;
      var previousTotal = state.CumulatedTotal;

      if (state.CutDateTime.HasValue) {
        var cut = state.CutDateTime.Value;
        state.CutDateTime = null;
        if (!interrupted && (cut <= dateTime)) {
          var totalAtCut = Interpolate (previousDateTime, previousTotal, dateTime, total, cut);
          CloseCumulated (field, period, cut, totalAtCut);
          period = StartCumulated (field, state, cut, totalAtCut);
        }
        else {
          interrupted = true;
        }
      }

      if (interrupted) {
        period = StartCumulated (field, state, dateTime, total);
      }
      else {
        // Bounds of the clock grid and week bound
        while (true) {
          var gridBound = GetGridBound (dateTime, interval);
          DateTime? bound = null;
          if (period.MaxEnd < dateTime) {
            bound = period.MaxEnd;
          }
          if ((period.Begin < gridBound) && (!bound.HasValue || (gridBound < bound.Value))) {
            bound = gridBound;
          }
          if (!bound.HasValue) {
            break;
          }
          var totalAtBound = Interpolate (previousDateTime, previousTotal, dateTime, total, bound.Value);
          CloseCumulated (field, period, bound.Value, totalAtBound);
          period = StartCumulated (field, state, bound.Value, totalAtBound);
        }
      }

      period.End = dateTime;
      period.Max = total;
      period.LastValue = total;
      period.Count = (period.Count ?? 0) + count;
      SetCumulatedAverage (field, period);
      period.JsonValue = GetCumulatedJson (state, rate);
      Persist (period);
      state.CumulatedTotal = total;
      state.CumulatedDateTime = dateTime;
    }

    /// <summary>
    /// Last bound of the clock grid before or at dateTime (multiples of interval since 00:00 UTC)
    /// </summary>
    static DateTime GetGridBound (DateTime dateTime, TimeSpan interval)
    {
      var day = dateTime.Date;
      var sinceDay = dateTime.Subtract (day).Ticks;
      return new DateTime (day.Ticks + sinceDay - sinceDay % interval.Ticks, dateTime.Kind);
    }

    /// <summary>
    /// Value at a date/time, interpolated between (previousDateTime, previousValue) and (dateTime, value)
    /// </summary>
    static double Interpolate (DateTime? previousDateTime, double? previousValue, DateTime dateTime, double value, DateTime at)
    {
      if (!previousDateTime.HasValue || !previousValue.HasValue || (dateTime <= previousDateTime.Value) || (dateTime <= at)) {
        return value;
      }
      if (at <= previousDateTime.Value) {
        return previousValue.Value;
      }
      return previousValue.Value + (value - previousValue.Value) * at.Subtract (previousDateTime.Value).Ticks / dateTime.Subtract (previousDateTime.Value).Ticks;
    }

    void CloseCumulated (IField field, ICncDataPeriod period, DateTime end, double total)
    {
      period.End = end;
      period.Max = total;
      period.LastValue = total;
      SetCumulatedAverage (field, period);
      Persist (period);
    }

    ICncDataPeriod StartCumulated (IField field, CncDataPeriodState state, DateTime begin, double total)
    {
      var period = Create (field, begin);
      period.Min = total;
      period.Max = total;
      period.LastValue = total;
      period.Count = 0;
      state.Start (period, begin, null);
      return period;
    }

    /// <summary>
    /// Integral: average rate of the period, in units of the value multiplied by the factor
    /// </summary>
    static void SetCumulatedAverage (IField field, ICncDataPeriod period)
    {
      if ((CncDataPeriodAggregationKind.Integral != field.GetPeriodAggregationKind ())
        || !period.Min.HasValue || !period.Max.HasValue || (period.End <= period.Begin)) {
        return;
      }
      var timeUnit = field.GetCncDataParameter (CncDataParameterNames.TimeUnit, TimeSpan.FromHours (1));
      period.Average = (period.Max.Value - period.Min.Value) / (period.End.Subtract (period.Begin).TotalSeconds / timeUnit.TotalSeconds);
    }

    /// <summary>
    /// State to continue after a restart (see <see cref="CncDataPeriodState.RestoreCumulated"/>)
    /// </summary>
    static string GetCumulatedJson (CncDataPeriodState state, double? rate)
    {
      if (rate.HasValue) {
        return string.Format (CultureInfo.InvariantCulture, @"{{""rate"": {0:R}}}", rate.Value);
      }
      if (state.CounterRaw.HasValue) {
        return string.Format (CultureInfo.InvariantCulture, @"{{""raw"": {0:R}, ""offset"": {1:R}}}", state.CounterRaw.Value, state.CounterOffset);
      }
      return null;
    }

    void StartStatistics (IField field, CncDataPeriodState state, DateTime begin, DateTime dateTime, CncDataStatistics statistics)
    {
      var period = Create (field, begin);
      period.End = dateTime;
      period.Statistics = statistics;
      Persist (period);
      state.Start (period, begin, statistics);
    }

    static CncDataStatistics? Combine (CncDataStatistics? a, CncDataStatistics? b)
    {
      if (!a.HasValue) {
        return b;
      }
      if (!b.HasValue) {
        return a;
      }
      return CncDataStatistics.Combine (a.Value, b.Value);
    }
  }
}
