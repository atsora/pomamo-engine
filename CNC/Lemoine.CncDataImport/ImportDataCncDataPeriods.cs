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
                    ProcessChange (field, state, data.DateTime, value);
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
                    ProcessStatistics (field, state, data.DateTime, statistics);
                  }
                  else {
                    log.Error ($"ImportDatas: value {data.Value} of {data} is not numeric for field {field.Code} => skip it");
                  }
                }
              }
              break;
            case CncDataPeriodAggregationKind.Custom: {
                var extension = GetExtension (field);
                if (null != extension) {
                  extension.Import (m_machineModule, field,
                    datas.Select (d => new KeyValuePair<DateTime, object> (d.DateTime, d.Value)).ToList ());
                }
              }
              break;
            }
            SetActive ();
            transaction.Commit ();
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
        || m_cncValueCache.IsEndCncValueRequired (field.Code, state.Period.End, dateTime, false);
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

      if (!IsNewPeriodRequired (field, state, dateTime)) {
        var period = state.Period;
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

      if (IsNewPeriodRequired (field, state, dateTime)) {
        StartStatistics (field, state, dateTime, dateTime, statistics);
        return;
      }

      var period = state.Period;
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
