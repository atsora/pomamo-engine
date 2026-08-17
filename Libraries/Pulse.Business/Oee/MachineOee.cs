// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.Oee
{
  /// <summary>
  /// Response class for the <see cref="MachineOee"/> business request
  /// </summary>
  public sealed class OeeResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="oeeType"></param>
    /// <param name="range"></param>
    public OeeResponse (OeeType oeeType, UtcDateTimeRange range)
    {
      this.OeeType = oeeType;
      this.Range = range;
      this.Duration = TimeSpan.FromTicks (0);
      this.ProductionDuration = TimeSpan.FromTicks (0);
    }

    /// <summary>
    /// Kind of rate that was computed
    /// </summary>
    public OeeType OeeType { get; }

    /// <summary>
    /// Requested range
    /// </summary>
    public UtcDateTimeRange Range { get; }

    /// <summary>
    /// Reference time: total duration of the considered machine observation states
    /// for which a production rate is known
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Effective production time: sum of the durations weighted by the production rate
    /// </summary>
    public TimeSpan ProductionDuration { get; set; }

    /// <summary>
    /// Duration of the considered machine observation states for which no production rate is known.
    ///
    /// It is excluded from <see cref="Duration"/>.
    ///
    /// null when it is not available, which is the case when the data comes from the summary table
    /// </summary>
    public TimeSpan? NoDataDuration { get; set; }

    /// <summary>
    /// Was the production rate summary table used ?
    /// </summary>
    public bool FromSummary { get; set; }

    /// <summary>
    /// Computed rate, between 0 and 1
    ///
    /// null if no data is available in the requested range
    /// </summary>
    public double? Rate
    {
      get {
        if (0 == this.Duration.Ticks) {
          return null;
        }
        return this.ProductionDuration.TotalSeconds / this.Duration.TotalSeconds;
      }
    }
  }

  /// <summary>
  /// Request class to get the OEE (TRS), OOE (TRG) or TEEP (TRE) of a machine in a specified range,
  /// optionally restricted to a specific shift.
  ///
  /// The machine observation states that are considered depend on their capacity level,
  /// see <see cref="OeeType"/>. The production rate of the reason slots is used to weight
  /// the periods: 1 for a full production, 0 for no production at all.
  ///
  /// The production rate summary table is used when it is available and when the requested range
  /// matches a whole number of days.
  /// </summary>
  public sealed class MachineOee
    : IRequest<OeeResponse>
  {
    static readonly string USE_SUMMARY_KEY = "Business.Oee.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly string PRODUCTION_RATE_SUMMARY_ACTIVE_KEY = "Summary.ProductionRate.Active";

    readonly IMachine m_machine;
    readonly UtcDateTimeRange m_range;
    readonly OeeType m_oeeType;
    readonly int? m_shiftId;

    readonly ILog log = LogManager.GetLogger (typeof (MachineOee).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <param name="oeeType"></param>
    /// <param name="shift">optional: restrict the computation to this shift</param>
    public MachineOee (IMachine machine, UtcDateTimeRange range, OeeType oeeType, IShift shift = null)
    {
      if (machine is null) {
        log.Fatal ("MachineOee: machine is null");
        throw new ArgumentNullException ("machine");
      }
      if (range is null) {
        log.Fatal ("MachineOee: range is null");
        throw new ArgumentNullException ("range");
      }

      m_machine = machine;
      m_range = range;
      m_oeeType = oeeType;
      m_shiftId = shift?.Id;

      log = LogManager.GetLogger ($"{typeof (MachineOee).FullName}.{machine.Id}");
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public OeeResponse Get ()
    {
      var response = new OeeResponse (m_oeeType, m_range);
      if (m_range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: empty range {m_range} => return an empty response");
        }
        return response;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dayRange = GetSummaryDayRange ();
        if (dayRange is null) {
          FillFromReasonSlots (response, ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (m_machine, m_range));
        }
        else {
          FillFromSummary (response, ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
            .FindInDayRange (m_machine, dayRange));
        }
      }

      return response;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<OeeResponse> GetAsync ()
    {
      var response = new OeeResponse (m_oeeType, m_range);
      if (m_range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetAsync: empty range {m_range} => return an empty response");
        }
        return response;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dayRange = await GetSummaryDayRangeAsync ();
        if (dayRange is null) {
          FillFromReasonSlots (response, await ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeAsync (m_machine, m_range));
        }
        else {
          FillFromSummary (response, await ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
            .FindInDayRangeAsync (m_machine, dayRange));
        }
      }

      return response;
    }

    /// <summary>
    /// Day range to consider to use the production rate summary table,
    /// null if the summary table can't be used here
    /// </summary>
    /// <returns></returns>
    DayRange GetSummaryDayRange ()
    {
      if (!IsSummaryActive ()) {
        return null;
      }

      var dayRange = ServiceProvider
        .Get (new Lemoine.Business.Time.DayRangeFromRange (m_range));
      var rangeFromDayRange = ServiceProvider
        .Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      return CheckSummaryDayRange (dayRange, rangeFromDayRange);
    }

    /// <summary>
    /// Asynchronous version of GetSummaryDayRange
    /// </summary>
    /// <returns></returns>
    async Task<DayRange> GetSummaryDayRangeAsync ()
    {
      if (!IsSummaryActive ()) {
        return null;
      }

      var dayRange = await ServiceProvider
        .GetAsync (new Lemoine.Business.Time.DayRangeFromRange (m_range));
      var rangeFromDayRange = await ServiceProvider
        .GetAsync (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      return CheckSummaryDayRange (dayRange, rangeFromDayRange);
    }

    bool IsSummaryActive ()
    {
      return ConfigSet.LoadAndGet (USE_SUMMARY_KEY, USE_SUMMARY_DEFAULT)
        && ConfigSet.LoadAndGet (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, false);
    }

    DayRange CheckSummaryDayRange (DayRange dayRange, UtcDateTimeRange rangeFromDayRange)
    {
      if ((dayRange is null) || dayRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckSummaryDayRange: no day range for {m_range} => do not use the summary");
        }
        return null;
      }

      if (!m_range.Equals (rangeFromDayRange)) {
        // The requested range does not match a whole number of days:
        // the summary, that is stored by day, is not valid here
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckSummaryDayRange: {m_range} does not match the day range {dayRange} => do not use the summary");
        }
        return null;
      }

      return dayRange;
    }

    void FillFromSummary (OeeResponse response, IEnumerable<IProductionRateSummary> summaries)
    {
      var duration = TimeSpan.FromTicks (0);
      var productionDuration = TimeSpan.FromTicks (0);

      foreach (var summary in summaries.Where (s => IsSelected (s))) {
        duration = duration.Add (summary.Duration);
        productionDuration = productionDuration
          .Add (TimeSpan.FromSeconds (summary.Duration.TotalSeconds * summary.ProductionRate));
      }

      response.Duration = duration;
      response.ProductionDuration = productionDuration;
      response.NoDataDuration = null; // Not tracked by the summary table
      response.FromSummary = true;
    }

    void FillFromReasonSlots (OeeResponse response, IEnumerable<IReasonSlot> reasonSlots)
    {
      var duration = TimeSpan.FromTicks (0);
      var productionDuration = TimeSpan.FromTicks (0);
      var noDataDuration = TimeSpan.FromTicks (0);

      foreach (var reasonSlot in reasonSlots.Where (s => IsSelected (s))) {
        var intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (m_range));
        if (intersection.IsEmpty ()) {
          continue;
        }
        if (!intersection.Duration.HasValue) {
          log.Error ($"FillFromReasonSlots: no duration for {intersection} => skip it");
          continue;
        }
        var slotDuration = intersection.Duration.Value;
        if (reasonSlot.ProductionRate.HasValue) {
          duration = duration.Add (slotDuration);
          productionDuration = productionDuration
            .Add (TimeSpan.FromSeconds (slotDuration.TotalSeconds * reasonSlot.ProductionRate.Value));
        }
        else {
          noDataDuration = noDataDuration.Add (slotDuration);
        }
      }

      response.Duration = duration;
      response.ProductionDuration = productionDuration;
      response.NoDataDuration = noDataDuration;
      response.FromSummary = false;
    }

    bool IsSelected (IProductionRateSummary summary)
    {
      return IsSelected (summary.MachineObservationState, summary.Shift);
    }

    bool IsSelected (IReasonSlot reasonSlot)
    {
      return IsSelected (reasonSlot.MachineObservationState, reasonSlot.Shift);
    }

    bool IsSelected (IMachineObservationState machineObservationState, IShift shift)
    {
      if (m_shiftId.HasValue && ((shift is null) || (shift.Id != m_shiftId.Value))) {
        return false;
      }
      return m_oeeType.IsIncluded (machineObservationState);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Oee.MachineOee.{m_machine.Id}.{m_range}.{m_oeeType}.{m_shiftId}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (OeeResponse data)
    {
      if (Bound.Compare<DateTime> (m_range.Upper, DateTime.UtcNow) < 0) { // Completed range
        return CacheTimeOut.PastLong.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<OeeResponse> data)
    {
      return true;
    }
  }
}
