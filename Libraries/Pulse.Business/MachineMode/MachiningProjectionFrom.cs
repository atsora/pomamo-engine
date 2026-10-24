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
  /// Request class to get the projection of the machining time of a machine from a date/time
  ///
  /// Each observation state slot that follows the date/time gets the utilization rate its
  /// machine observation state had in the past period. The observation state slots are
  /// only planned a few days ahead from the machine state templates: after them, and in a
  /// slot whose state is not known yet or was hardly observed, the overall utilization
  /// rate of the machine applies. Without any data in the past period, a default rate of a
  /// production period, or of a period that is not production, applies
  /// </summary>
  public sealed class MachiningProjectionFrom
    : IRequest<MachiningProjection>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachiningProjectionFrom).FullName);

    readonly IMachine m_machine;
    readonly DateTime m_from;
    readonly TimeSpan m_period;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="from">in UTC</param>
    /// <param name="period">
    /// how far after from the observation state slots are read. The default rate applies
    /// after the slots anyway, so a longer period only matters when slots are planned there
    /// </param>
    public MachiningProjectionFrom (IMachine machine, DateTime from, TimeSpan period)
    {
      if (machine is null) {
        log.Fatal ("MachiningProjectionFrom: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_from = from;
      m_period = period;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The DAO is read asynchronously, so the asynchronous implementation is the real one:
    /// this is the bridge the synchronous callers of IRequest go through
    /// </summary>
    /// <returns>not null</returns>
    public MachiningProjection Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<MachiningProjection> GetAsync ()
    {
      var rates = await ServiceProvider
        .GetAsync (new UtilizationRateByObservationState (m_machine));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachiningProjectionFrom")) {
          var range = new UtcDateTimeRange (m_from, m_from.Add (m_period));
          var slots = await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRangeAsync (m_machine, range);
          var segments = slots
            .Select (s => new MachiningRateSegment (
              s.DateTimeRange.Lower.HasValue ? s.DateTimeRange.Lower.Value : m_from,
              s.DateTimeRange.Upper.HasValue ? s.DateTimeRange.Upper.Value : (DateTime?)null,
              rates.GetExpectedRate (s.MachineObservationState?.Id, s.Production ?? s.MachineObservationState?.IsProduction)))
            .ToList ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine {m_machine.Id} from {m_from}: {segments.Count} slots, {rates}");
          }
          // After the planned slots, neither the state nor the production is known
          return new MachiningProjection (segments, rates.GetExpectedRate (null, null));
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineMode.MachiningProjectionFrom.{m_machine.Id}.{m_from.Ticks}.{m_period.Ticks}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The planned observation state slots may be changed at any time
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (MachiningProjection data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<MachiningProjection> data) => true;
  }
}
