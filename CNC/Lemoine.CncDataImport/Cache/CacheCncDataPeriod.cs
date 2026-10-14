// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.CncDataImport.Cache
{
  /// <summary>
  /// State of the import of a field in table cncdataperiod
  /// </summary>
  internal sealed class CncDataPeriodState
  {
    /// <summary>
    /// Last period of the field (it may be stopped), null if none
    /// </summary>
    public ICncDataPeriod Period { get; set; }

    /// <summary>
    /// Statistics: statistics of [Period.Begin, SegmentBegin], already validated (null if none)
    /// </summary>
    public CncDataStatistics? BaseStatistics { get; set; }

    /// <summary>
    /// Statistics: begin of the current segment, that is not validated yet (MinTime not reached)
    /// </summary>
    public DateTime SegmentBegin { get; set; }

    /// <summary>
    /// Statistics: statistics of the current segment (null if none)
    /// </summary>
    public CncDataStatistics? SegmentStatistics { get; set; }

    /// <summary>
    /// Date/time of a cut of the current period, not processed yet (null if none):
    /// the next value starts a new period at this date/time (except after a gap)
    ///
    /// Note: it is kept only in memory. After a restart, the next value continues the period that ends at the cut
    /// </summary>
    public DateTime? CutDateTime { get; set; }

    /// <summary>
    /// Counter, Integral: cumulated total at the last value (null if none)
    /// </summary>
    public double? CumulatedTotal { get; set; }

    /// <summary>
    /// Counter, Integral: date/time of the last value (null if none)
    /// </summary>
    public DateTime? CumulatedDateTime { get; set; }

    /// <summary>
    /// Counter: last raw value of the counter (null if none)
    /// </summary>
    public double? CounterRaw { get; set; }

    /// <summary>
    /// Counter: offset to add to the raw value after the resets and the rollovers
    /// </summary>
    public double CounterOffset { get; set; }

    /// <summary>
    /// Integral: last instantaneous value, multiplied by the factor (null if none)
    /// </summary>
    public double? IntegralRate { get; set; }

    /// <summary>
    /// Counter, Integral: restore the state from the last period (total, and the json value written by the import)
    /// </summary>
    /// <param name="last">not null</param>
    public void RestoreCumulated (ICncDataPeriod last)
    {
      this.CumulatedTotal = last.LastValue;
      this.CumulatedDateTime = last.End;
      if (string.IsNullOrEmpty (last.JsonValue)) {
        return;
      }
      try {
        using (var document = System.Text.Json.JsonDocument.Parse (last.JsonValue)) {
          var root = document.RootElement;
          if (root.TryGetProperty ("raw", out var raw) && (raw.ValueKind == System.Text.Json.JsonValueKind.Number)) {
            this.CounterRaw = raw.GetDouble ();
          }
          if (root.TryGetProperty ("offset", out var offset) && (offset.ValueKind == System.Text.Json.JsonValueKind.Number)) {
            this.CounterOffset = offset.GetDouble ();
          }
          if (root.TryGetProperty ("rate", out var rate) && (rate.ValueKind == System.Text.Json.JsonValueKind.Number)) {
            this.IntegralRate = rate.GetDouble ();
          }
        }
      }
      catch (System.Text.Json.JsonException) {
      }
    }

    /// <summary>
    /// Set a new period that starts the statistics
    /// </summary>
    /// <param name="period"></param>
    /// <param name="segmentBegin"></param>
    /// <param name="segmentStatistics"></param>
    public void Start (ICncDataPeriod period, DateTime segmentBegin, CncDataStatistics? segmentStatistics)
    {
      this.Period = period;
      this.BaseStatistics = null;
      this.SegmentBegin = segmentBegin;
      this.SegmentStatistics = segmentStatistics;
      this.CutDateTime = null;
    }
  }

  /// <summary>
  /// Cache of the import states of the fields of a machine module in table cncdataperiod
  ///
  /// The state of a field is initialized from the last period in the database,
  /// so that the import continues after a restart without any overlap
  /// </summary>
  internal sealed class CacheCncDataPeriod
  {
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly IDictionary<int, CncDataPeriodState> m_states = new Dictionary<int, CncDataPeriodState> ();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    public CacheCncDataPeriod (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      m_machineModule = machineModule;
      log = LogManager.GetLogger ($"{typeof (CacheCncDataPeriod).FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");
    }

    /// <summary>
    /// Get the state of a field, load it from the database if needed
    ///
    /// To run in a session
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns>not null</returns>
    public CncDataPeriodState Get (IField field)
    {
      if (!m_states.TryGetValue (field.Id, out var state)) {
        var last = ModelDAOHelper.DAOFactory.CncDataPeriodDAO.FindLast (m_machineModule, field);
        state = new CncDataPeriodState ();
        if (null != last) {
          state.Period = last;
          state.BaseStatistics = last.Statistics;
          state.SegmentBegin = last.End;
          var kind = field.GetPeriodAggregationKind ();
          if ((CncDataPeriodAggregationKind.Counter == kind) || (CncDataPeriodAggregationKind.Integral == kind)) {
            state.RestoreCumulated (last);
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: initial state of field {field.Id} from the last period {last}");
        }
        m_states[field.Id] = state;
      }
      return state;
    }

    /// <summary>
    /// Forget the state of a field, for example after a rollback
    /// </summary>
    /// <param name="fieldId"></param>
    public void Reset (int fieldId)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Reset: field {fieldId}");
      }
      m_states.Remove (fieldId);
    }
  }
}
