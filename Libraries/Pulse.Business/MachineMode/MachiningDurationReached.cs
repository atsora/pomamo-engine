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

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get when the machining time of a machine, counted from a date/time,
  /// reached a number of seconds
  ///
  /// This is the exact date/time, walked from the reason slots: it tells for example since
  /// when a maintenance is due, and an estimation would not do
  /// </summary>
  public sealed class MachiningDurationReached
    : IRequest<DateTime?>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachiningDurationReached).FullName);

    readonly IMachine m_machine;
    readonly DateTime m_since;
    readonly int m_seconds;
    readonly MachineModeFilter m_filter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="since">in UTC</param>
    /// <param name="seconds">number of seconds of machining time to reach</param>
    /// <param name="filter">machine modes whose time is counted. Default: the running ones</param>
    public MachiningDurationReached (IMachine machine, DateTime since, int seconds, MachineModeFilter filter = MachineModeFilter.Running)
    {
      if (machine is null) {
        log.Fatal ("MachiningDurationReached: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_since = since;
      m_seconds = seconds;
      m_filter = filter;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The DAO is read asynchronously, so the asynchronous implementation is the real one:
    /// this is the bridge the synchronous callers of IRequest go through
    /// </summary>
    /// <returns>null if the machining time did not reach the number of seconds yet</returns>
    public DateTime? Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>null if the machining time did not reach the number of seconds yet</returns>
    public async Task<DateTime?> GetAsync ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachiningDurationReached")) {
          var countedRange = new UtcDateTimeRange (m_since);
          var reasonSlots = (await ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAllInUtcRangeWithMachineModeAsync (m_machine, countedRange))
            .Where (x => (m_filter == MachineModeFilter.Running) ? x.Running : m_filter.Matches (x.MachineMode))
            .OrderBy (x => x.DateTimeRange.Lower.HasValue ? x.DateTimeRange.Lower.Value : DateTime.MinValue);

          var remaining = TimeSpan.FromSeconds (m_seconds);
          foreach (var reasonSlot in reasonSlots) {
            // The first slot may start before the counted period
            var range = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (countedRange));
            if (range.IsEmpty () || !range.Lower.HasValue || !range.Duration.HasValue) {
              continue;
            }
            if (remaining <= range.Duration.Value) {
              return range.Lower.Value.Add (remaining);
            }
            remaining = remaining.Subtract (range.Duration.Value);
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine {m_machine.Id} did not reach {m_seconds}s of machining time since {m_since}, {remaining} left");
          }
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineMode.MachiningDurationReached.{m_machine.Id}.{m_since.Ticks}.{m_seconds}{m_filter.GetCacheKeySuffix ()}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// Once a date/time is reached, it does not move any more: it is kept for a long time.
    /// While it is not reached, it is asked again regularly
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (DateTime? data)
    {
      return data.HasValue
        ? CacheTimeOut.PastLong.GetTimeSpan ()
        : CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<DateTime?> data) => true;
  }
}
