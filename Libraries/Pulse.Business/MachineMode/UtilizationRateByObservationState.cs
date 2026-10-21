// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get the utilization rate of a machine by machine observation state,
  /// over the full days of a past period
  ///
  /// A machine does not machine the same way in every machine observation state: it is
  /// expected to machine during the attended production, much less during the unattended
  /// one or during a set-up, not at all when it is off. A single rate over all the states
  /// hides that. This one is read from the machine activity summaries, so it only covers
  /// the days that are fully processed: the current day is left out
  /// </summary>
  public sealed class UtilizationRateByObservationState
    : IRequest<UtilizationRateByObservationStateResponse>
  {
    static readonly string PERIOD_KEY = "Business.MachineMode.UtilizationRateByObservationState.Period";
    static readonly TimeSpan PERIOD_DEFAULT = TimeSpan.FromDays (90);

    static readonly string MIN_TIME_KEY = "Business.MachineMode.UtilizationRateByObservationState.MinTime";
    static readonly TimeSpan MIN_TIME_DEFAULT = TimeSpan.FromHours (1);

    static readonly string DEFAULT_PRODUCTION_KEY = "Business.MachineMode.UtilizationRateByObservationState.Default.Production";
    static readonly double DEFAULT_PRODUCTION_DEFAULT = 0.7;

    static readonly string DEFAULT_NOT_PRODUCTION_KEY = "Business.MachineMode.UtilizationRateByObservationState.Default.NotProduction";
    static readonly double DEFAULT_NOT_PRODUCTION_DEFAULT = 0.1;

    static readonly string CACHE_TIMEOUT_KEY = "Business.MachineMode.UtilizationRateByObservationState.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = TimeSpan.FromHours (3);

    static readonly ILog log = LogManager.GetLogger (typeof (UtilizationRateByObservationState).FullName);

    readonly IMachine m_machine;
    readonly int m_numberOfDays;

    /// <summary>
    /// Past period the rates are computed over, by default
    /// </summary>
    public static TimeSpan DefaultPeriod => Lemoine.Info.ConfigSet.LoadAndGet (PERIOD_KEY, PERIOD_DEFAULT);

    /// <summary>
    /// Minimum time a machine observation state must have been observed in the period for its
    /// rate to be significant
    /// </summary>
    static TimeSpan MinTime => Lemoine.Info.ConfigSet.LoadAndGet (MIN_TIME_KEY, MIN_TIME_DEFAULT);

    /// <summary>
    /// Constructor, over the default past period (configuration key
    /// Business.MachineMode.UtilizationRateByObservationState.Period)
    /// </summary>
    /// <param name="machine">not null</param>
    public UtilizationRateByObservationState (IMachine machine)
      : this (machine, DefaultPeriod)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="period">past period, rounded up to full days, at least one day</param>
    public UtilizationRateByObservationState (IMachine machine, TimeSpan period)
    {
      if (machine is null) {
        log.Fatal ("UtilizationRateByObservationState: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_numberOfDays = Math.Max (1, (int)Math.Ceiling (period.TotalDays));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The asynchronous implementation is the real one: this is the bridge the synchronous
    /// callers of IRequest go through
    /// </summary>
    /// <returns>not null</returns>
    public UtilizationRateByObservationStateResponse Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<UtilizationRateByObservationStateResponse> GetAsync ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Read-write because of the day processing
        using (var transaction = session.BeginTransaction ("Business.MachineMode.UtilizationRateByObservationState")) {
          var lastDay = await GetLastFullDayAsync ();
          var firstDay = lastDay.AddDays (1 - m_numberOfDays);
          var summaries = await ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
            .FindInDayRangeWithMachineModeAsync (m_machine, new DayRange (firstDay, lastDay));
          var response = Compute (firstDay, lastDay, summaries);
          transaction.Commit ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine {m_machine.Id} => {response}");
          }
          return response;
        }
      }
    }

    /// <summary>
    /// Get the last day that is fully processed: the day before the current one
    /// </summary>
    /// <returns></returns>
    async Task<DateTime> GetLastFullDayAsync ()
    {
      var now = DateTime.UtcNow;
      var daySlot = await ModelDAOHelper.DAOFactory.DaySlotDAO
        .FindProcessedAtAsync (now);
      if (daySlot?.Day is null) {
        log.Error ($"GetLastFullDayAsync: no processed day at {now} => fallback, use the UTC date");
        return now.Date.AddDays (-1);
      }
      return daySlot.Day.Value.AddDays (-1);
    }

    /// <summary>
    /// Compute the rates from the machine activity summaries
    /// </summary>
    /// <param name="firstDay"></param>
    /// <param name="lastDay"></param>
    /// <param name="summaries">not null</param>
    /// <returns>not null</returns>
    UtilizationRateByObservationStateResponse Compute (DateTime firstDay, DateTime lastDay, IEnumerable<IMachineActivitySummary> summaries)
    {
      var minSeconds = MinTime.TotalSeconds;
      var byState = summaries
        .Where (s => null != s.MachineObservationState)
        .GroupBy (s => s.MachineObservationState.Id)
        .Select (g => new {
          MachineObservationStateId = g.Key,
          TotalSeconds = g.Sum (s => s.Time.TotalSeconds),
          RunningSeconds = g.Where (IsRunning).Sum (s => s.Time.TotalSeconds),
        })
        .ToList ();

      var rates = byState
        .Where (x => (0 < x.TotalSeconds) && (minSeconds <= x.TotalSeconds))
        .ToDictionary (x => x.MachineObservationStateId, x => x.RunningSeconds / x.TotalSeconds);

      var totalSeconds = byState.Sum (x => x.TotalSeconds);
      double? overallRate = (0 < totalSeconds)
        ? byState.Sum (x => x.RunningSeconds) / totalSeconds
        : (double?)null;
      if (!overallRate.HasValue) {
        log.Warn ($"Compute: no activity data for machine {m_machine.Id} between {firstDay:yyyy-MM-dd} and {lastDay:yyyy-MM-dd} => the default rates apply");
      }

      return new UtilizationRateByObservationStateResponse (firstDay, lastDay, rates, overallRate,
        Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_PRODUCTION_KEY, DEFAULT_PRODUCTION_DEFAULT),
        Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_NOT_PRODUCTION_KEY, DEFAULT_NOT_PRODUCTION_DEFAULT));
    }

    static bool IsRunning (IMachineActivitySummary summary) =>
      summary.MachineMode?.Running ?? false;

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineMode.UtilizationRateByObservationState.{m_machine.Id}.{m_numberOfDays}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<UtilizationRateByObservationStateResponse> data) => true;

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The rates only move with a new processed day, and slowly: they are kept a few hours
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (UtilizationRateByObservationStateResponse data)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet (CACHE_TIMEOUT_KEY, CACHE_TIMEOUT_DEFAULT);
    }
  }
}
