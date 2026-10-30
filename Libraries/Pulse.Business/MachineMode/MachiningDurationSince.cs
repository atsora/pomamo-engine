// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get the machining duration of a machine since a date/time
  ///
  /// The answer only grows, so it is not recomputed from scratch: the value that is in the
  /// cache is extended with the machining duration of the range that was analysed since it
  /// was computed, and the result becomes the new value in the cache
  ///
  /// The cache is therefore kept for a long time on purpose: it is the accumulator, and
  /// losing it costs a full computation of the whole period. What decides whether the value
  /// is up to date is <see cref="IsCacheValid"/>, not the time out
  /// </summary>
  public sealed class MachiningDurationSince
    : IRequest<MachiningDurationSinceResponse>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachiningDurationSince).FullName);

    /// <summary>
    /// How long a value stays in the cache: this is the accumulator, not a freshness
    /// </summary>
    static readonly string CACHE_TIMEOUT_KEY = "Business.MachineMode.MachiningDurationSince.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = TimeSpan.FromDays (1);

    /// <summary>
    /// How far behind the current date/time a value may stop before it is extended
    /// </summary>
    static readonly string REFRESH_KEY = "Business.MachineMode.MachiningDurationSince.Refresh";
    static readonly TimeSpan REFRESH_DEFAULT = TimeSpan.FromMinutes (1);

    readonly IMachine m_machine;
    readonly DateTime m_since;
    readonly MachineModeFilter m_filter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="since">in UTC</param>
    /// <param name="filter">machine modes whose time is counted. Default: the running ones</param>
    public MachiningDurationSince (IMachine machine, DateTime since, MachineModeFilter filter = MachineModeFilter.Running)
    {
      if (machine is null) {
        log.Fatal ("MachiningDurationSince: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_since = since;
      m_filter = filter;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// MachiningDuration is read asynchronously, so the asynchronous implementation is the
    /// real one: this is the bridge the synchronous callers of IRequest go through
    /// </summary>
    /// <returns>not null</returns>
    public MachiningDurationSinceResponse Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<MachiningDurationSinceResponse> GetAsync ()
    {
      var previous = ServiceProvider.GetCacheData (this)?.Value;
      var slice = await GetMachiningDurationFromAsync (GetStart (previous, m_since));
      return Combine (previous, m_since, slice);
    }

    /// <summary>
    /// Extend a machining duration that was already computed, by reading the machining
    /// duration from where the previous response stops
    ///
    /// Nothing is read again on the period the previous response already covers. A previous
    /// response of another period is not usable: the whole period is read again
    /// </summary>
    /// <param name="previous">the response to extend, null if there is none</param>
    /// <param name="since">start of the counted period</param>
    /// <param name="getMachiningDurationFrom">
    /// machining duration from a date/time, with the date/time it was counted up to.
    /// Not null, called once
    /// </param>
    /// <returns>not null</returns>
    public static MachiningDurationSinceResponse Extend (MachiningDurationSinceResponse previous, DateTime since, Func<DateTime, (TimeSpan Duration, DateTime Until)> getMachiningDurationFrom)
    {
      return Combine (previous, since, getMachiningDurationFrom (GetStart (previous, since)));
    }

    /// <summary>
    /// Get the date/time the machining duration is to be read from: where the previous
    /// response stops when it may be extended, the start of the period otherwise
    /// </summary>
    /// <param name="previous">may be null</param>
    /// <param name="since"></param>
    /// <returns></returns>
    static DateTime GetStart (MachiningDurationSinceResponse previous, DateTime since)
    {
      return IsExtendable (previous, since) ? previous.Until : since;
    }

    /// <summary>
    /// Add a machining duration that was read to the previous response
    /// </summary>
    /// <param name="previous">may be null</param>
    /// <param name="since"></param>
    /// <param name="slice">machining duration that was read, and where it stops</param>
    /// <returns>not null</returns>
    static MachiningDurationSinceResponse Combine (MachiningDurationSinceResponse previous, DateTime since, (TimeSpan Duration, DateTime Until) slice)
    {
      if (!IsExtendable (previous, since)) {
        return new MachiningDurationSinceResponse (since, slice.Until, slice.Duration);
      }
      if (slice.Until <= previous.Until) {
        // Nothing new was analysed
        return previous;
      }
      return new MachiningDurationSinceResponse (since, slice.Until, previous.Duration.Add (slice.Duration));
    }

    /// <summary>
    /// May a previous response be extended, rather than read again from the start?
    /// </summary>
    /// <param name="previous">may be null</param>
    /// <param name="since"></param>
    /// <returns></returns>
    static bool IsExtendable (MachiningDurationSinceResponse previous, DateTime since)
    {
      return !(previous is null) && (previous.Since == since) && (since <= previous.Until);
    }

    /// <summary>
    /// Read the machining duration of the machine from a date/time, up to wherever the
    /// activity of the machine is analysed
    ///
    /// The range has no end on purpose: <see cref="MachiningDuration"/> then always reports
    /// the date/time it could count up to, which is the end of the last reason slot. No
    /// reason slot crosses it, so the next call may start there without counting anything
    /// twice
    /// </summary>
    /// <param name="from"></param>
    /// <returns>the machining duration, and the date/time it was counted up to</returns>
    async Task<(TimeSpan Duration, DateTime Until)> GetMachiningDurationFromAsync (DateTime from)
    {
      var response = await ServiceProvider
        .GetAsync (new MachiningDuration (m_machine, new UtcDateTimeRange (from), m_filter));
      return (response.Duration, GetUntil (response, from));
    }

    /// <summary>
    /// Get the date/time a machining duration was counted up to
    /// </summary>
    /// <param name="response">not null</param>
    /// <param name="from"></param>
    /// <returns></returns>
    DateTime GetUntil (MachiningDurationResponse response, DateTime from)
    {
      if (!response.MaxDateTime.HasValue) {
        // The range has no end, so it may not be counted up to its end
        log.Fatal ($"GetUntil: no max date/time for machine {m_machine.Id} from {from}");
        return from;
      }
      return response.MaxDateTime.Value;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      // The full precision of the date/time is kept: two periods that start in the same
      // second on a same machine must not share the accumulator
      return $"Business.MachineMode.MachiningDurationSince.{m_machine.Id}.{m_since.Ticks}{m_filter.GetCacheKeySuffix ()}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The value is kept long enough to be the accumulator of the next calls: dropping it
    /// only costs a full computation of the whole period, it is never wrong
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (MachiningDurationSinceResponse data)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet (CACHE_TIMEOUT_KEY, CACHE_TIMEOUT_DEFAULT);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// This is where the freshness is decided, and not in the time out: a value that stops
    /// too far behind the current date/time is extended, the cached one is kept otherwise.
    /// The extension reads what was analysed in between only, so asking too often costs
    /// little
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachiningDurationSinceResponse> data)
    {
      if (data.Value is null || data.Value.Since != m_since) {
        return false;
      }
      var refresh = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_KEY, REFRESH_DEFAULT);
      return DateTime.UtcNow.Subtract (data.Value.Until) < refresh;
    }
  }
}
