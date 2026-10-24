// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the parts, or the cycles, a machine made in a range, and when the
  /// thresholds were reached, see <see cref="CountThresholds"/>
  ///
  /// The full cycles whose end is in the range are read from the operation cycles, and
  /// counted by <see cref="CountThresholds.Walk"/>: the count and the date/times come from the
  /// same walk, so they cannot disagree. The reached date/times are exact: they tell since
  /// when something is due, or since how long it is late
  ///
  /// The range is the one of a single operation: the caller bounds it by the operation
  /// period of the machine, so the parts per cycle are the ones of that operation
  /// </summary>
  public sealed class OperationCountReached
    : IRequest<CountThresholdsResult>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCountReached).FullName);

    readonly IMachine m_machine;
    readonly UtcDateTimeRange m_range;
    readonly bool m_countsParts;
    readonly int m_partsPerCycle;
    readonly int m_due;
    readonly int? m_late;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">counted range, not null: a cycle counts when its end is in it</param>
    /// <param name="countsParts">are the parts counted? Else the cycles are</param>
    /// <param name="partsPerCycle">parts one cycle of the operation makes, when the cycle is not adjusted</param>
    /// <param name="due">strictly positive count of the due threshold</param>
    /// <param name="late">count of the late threshold, null if there is none</param>
    public OperationCountReached (IMachine machine, UtcDateTimeRange range, bool countsParts, int partsPerCycle, int due, int? late)
    {
      if (machine is null) {
        log.Fatal ("OperationCountReached: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }
      if (range is null) {
        log.Fatal ("OperationCountReached: range is null");
        throw new ArgumentNullException (nameof (range));
      }

      m_machine = machine;
      m_range = range;
      m_countsParts = countsParts;
      m_partsPerCycle = partsPerCycle;
      m_due = due;
      m_late = late;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The DAO is read asynchronously, so the asynchronous implementation is the real one:
    /// this is the bridge the synchronous callers of IRequest go through
    /// </summary>
    /// <returns>not null</returns>
    public CountThresholdsResult Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<CountThresholdsResult> GetAsync ()
    {
      if (m_range.IsEmpty ()) {
        return CountThresholds.Walk (Enumerable.Empty<CountedCycle> (), m_countsParts, m_partsPerCycle, m_due, m_late);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationCountReached")) {
          // FindOverlapsRangeAsync also returns the partial cycles that begin in the range:
          // only the full cycles that end in it count
          var cycles = (await ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindOverlapsRangeAsync (m_machine, m_range))
            .Where (x => x.Full && x.End.HasValue && m_range.ContainsElement (x.End.Value))
            .Select (x => new CountedCycle (x.End.Value, x.Quantity))
            .ToList ();
          var result = CountThresholds.Walk (cycles, m_countsParts, m_partsPerCycle, m_due, m_late);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine {m_machine.Id} range {m_range} => {result}");
          }
          return result;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var parts = m_countsParts ? $"Parts.{m_partsPerCycle}" : "Cycles";
      return $"Business.Operation.OperationCountReached.{m_machine.Id}.{m_range.ToString (x => x.Ticks.ToString ())}.{parts}.{m_due}.{m_late}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The count grows as new cycles end: it is asked again regularly
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (CountThresholdsResult data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<CountThresholdsResult> data) => true;
  }
}
