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

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get how long one cycle of an operation takes in a production period,
  /// stops included: the rate the machine is expected to make the next cycles at
  ///
  /// It is measured on the last production periods, the observation state slots that are
  /// production, that are not further in the past than a look back period: the cycles of
  /// the operation, read by <see cref="CycleCounter"/> (the cycle counter extensions, which
  /// use the summary tables when there are some), and the time the operation was active in
  /// these periods.
  ///
  /// When too few cycles were made, or when only the standard is asked for, the standard
  /// cycle duration of the operation is divided by a factor: the goal of type
  /// <see cref="GoalTypeId.QuantityVsProductionCycleDuration"/> of the machine, else a
  /// default factor
  /// </summary>
  public sealed class OperationProductionRate
    : IRequest<OperationProductionRateResponse>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationProductionRate).FullName);

    static readonly string LOOK_BACK_KEY = "Business.Operation.OperationProductionRate.LookBack";
    static readonly TimeSpan LOOK_BACK_DEFAULT = TimeSpan.FromDays (7);

    static readonly string PRODUCTION_PERIODS_KEY = "Business.Operation.OperationProductionRate.ProductionPeriods";
    static readonly int PRODUCTION_PERIODS_DEFAULT = 4;

    static readonly string MIN_CYCLES_KEY = "Business.Operation.OperationProductionRate.MinCycles";
    static readonly int MIN_CYCLES_DEFAULT = 4;

    static readonly string DEFAULT_FACTOR_KEY = "Business.Operation.OperationProductionRate.DefaultFactor";
    static readonly double DEFAULT_FACTOR_DEFAULT = 0.7;

    readonly IMonitoredMachine m_machine;
    readonly int m_operationId;
    readonly DateTime m_until;
    readonly bool m_standardOnly;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operationId">id of the operation</param>
    /// <param name="until">
    /// in UTC, the past production periods are the ones before it. It is truncated to the
    /// hour: the rate moves slowly, and the requests of a same hour share it
    /// </param>
    /// <param name="standardOnly">consider the standard cycle duration only, and not the past cycles</param>
    public OperationProductionRate (IMonitoredMachine machine, int operationId, DateTime until, bool standardOnly = false)
    {
      if (machine is null) {
        log.Fatal ("OperationProductionRate: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_operationId = operationId;
      m_until = new DateTime (until.Ticks - (until.Ticks % TimeSpan.TicksPerHour), DateTimeKind.Utc);
      m_standardOnly = standardOnly;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>null when the operation does not exist, or when it has no standard cycle duration and too few cycles were made</returns>
    public OperationProductionRateResponse Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>null when the operation does not exist, or when it has no standard cycle duration and too few cycles were made</returns>
    public async Task<OperationProductionRateResponse> GetAsync ()
    {
      if (!m_standardOnly) {
        var history = await GetFromHistoryAsync ();
        if (!(history is null)) {
          return history;
        }
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationProductionRate")) {
          var operation = await ModelDAOHelper.DAOFactory.OperationDAO.FindByIdAsync (m_operationId);
          if (operation is null) {
            log.Error ($"GetAsync: operation {m_operationId} does not exist");
            return null;
          }
          var standardCycleDuration = operation.GetStandardCycleDuration (m_machine);
          if (!standardCycleDuration.HasValue || (standardCycleDuration.Value <= TimeSpan.Zero)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetAsync: operation {m_operationId} has no standard cycle duration");
            }
            return null;
          }
          var goal = ModelDAOHelper.DAOFactory.GoalDAO
            .FindMatch (GoalTypeId.QuantityVsProductionCycleDuration, null, m_machine);
          // Note: the value of the goal is in % (between 1 and 100)
          var factor = (goal is null)
            ? Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_FACTOR_KEY, DEFAULT_FACTOR_DEFAULT)
            : goal.Value / 100.0;
          var source = (goal is null) ? OperationProductionRateSource.Default : OperationProductionRateSource.Goal;
          return FromStandard (standardCycleDuration.Value, factor, source);
        }
      }
    }

    /// <summary>
    /// Production rate from the standard cycle duration and a factor
    /// </summary>
    /// <param name="standardCycleDuration">strictly positive</param>
    /// <param name="factor">share of the standard rate the machine reaches, between 0 and 1</param>
    /// <param name="source"></param>
    /// <returns>null when the factor is not strictly positive</returns>
    public static OperationProductionRateResponse FromStandard (TimeSpan standardCycleDuration, double factor, OperationProductionRateSource source)
    {
      if (factor <= 0) {
        log.Error ($"FromStandard: factor {factor} is not strictly positive");
        return null;
      }
      return new OperationProductionRateResponse (TimeSpan.FromTicks ((long)(standardCycleDuration.Ticks / factor)), source);
    }

    /// <summary>
    /// Production rate from the cycles that were made in the past production periods
    /// </summary>
    /// <returns>null when too few cycles were made</returns>
    async Task<OperationProductionRateResponse> GetFromHistoryAsync ()
    {
      var lookBack = Lemoine.Info.ConfigSet.LoadAndGet (LOOK_BACK_KEY, LOOK_BACK_DEFAULT);
      var number = Lemoine.Info.ConfigSet.LoadAndGet (PRODUCTION_PERIODS_KEY, PRODUCTION_PERIODS_DEFAULT);
      var minCycles = Lemoine.Info.ConfigSet.LoadAndGet (MIN_CYCLES_KEY, MIN_CYCLES_DEFAULT);

      var range = new UtcDateTimeRange (m_until.Subtract (lookBack), m_until);
      var slots = await ServiceProvider.GetAsync (new MachineObservationState.ProductionPeriods (m_machine, range));
      // The slots are returned whole: they are cut to the look back period
      var periods = GetLastProductionPeriods (slots
        .Select (x => (new UtcDateTimeRange (x.Item1.Intersects (range)), x.Item2)), number);
      var cycles = 0;
      var duration = TimeSpan.Zero;
      foreach (var period in periods) {
        var counters = await ServiceProvider.GetAsync (new CycleCounter (m_machine, period));
        foreach (var counter in counters.Where (x => !(x.Operation is null) && (x.Operation.Id == m_operationId))) {
          cycles += counter.TotalCycles;
          duration = duration.Add (counter.Duration);
        }
      }
      var result = FromHistory (cycles, duration, minCycles);
      if (log.IsDebugEnabled) {
        log.Debug ($"GetFromHistoryAsync: machine {m_machine.Id} operation {m_operationId}: {cycles} cycles in {duration} over {periods.Count} production periods => {result}");
      }
      return result;
    }

    /// <summary>
    /// Production rate from the cycles that were made in production periods
    /// </summary>
    /// <param name="cycles">cycles that were made</param>
    /// <param name="duration">time the operation was active in the production periods</param>
    /// <param name="minCycles">fewer cycles are not representative</param>
    /// <returns>null when too few cycles were made</returns>
    public static OperationProductionRateResponse FromHistory (int cycles, TimeSpan duration, int minCycles)
    {
      if ((cycles <= 0) || (cycles < minCycles) || (duration <= TimeSpan.Zero)) {
        return null;
      }
      return new OperationProductionRateResponse (TimeSpan.FromTicks (duration.Ticks / cycles), OperationProductionRateSource.History);
    }

    /// <summary>
    /// The last production periods: the observation state slots that are production, the
    /// ones that follow each other being merged
    /// </summary>
    /// <param name="slots">observation state slots with their production flag, not null</param>
    /// <param name="number">number of periods to keep</param>
    /// <returns>the last production periods, the oldest first</returns>
    public static IList<UtcDateTimeRange> GetLastProductionPeriods (IEnumerable<(UtcDateTimeRange Range, bool? Production)> slots, int number)
    {
      var periods = new List<UtcDateTimeRange> ();
      foreach (var slot in slots.Where (x => (x.Production ?? false) && !x.Range.IsEmpty ()).OrderBy (x => x.Range.Lower)) {
        var last = periods.LastOrDefault ();
        if (!(last is null) && last.Upper.HasValue && slot.Range.Lower.HasValue && (last.Upper.Value == slot.Range.Lower.Value)) {
          periods[periods.Count - 1] = new UtcDateTimeRange (last.Lower, slot.Range.Upper);
        }
        else {
          periods.Add (slot.Range);
        }
      }
      return periods
        .Skip (Math.Max (0, periods.Count - number))
        .ToList ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Operation.OperationProductionRate.{m_machine.Id}.{m_operationId}.{m_until.Ticks}.{m_standardOnly}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (OperationProductionRateResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<OperationProductionRateResponse> data) => true;
  }
}
