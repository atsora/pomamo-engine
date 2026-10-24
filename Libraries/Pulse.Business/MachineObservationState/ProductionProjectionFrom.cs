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

namespace Lemoine.Business.MachineObservationState
{
  /// <summary>
  /// Request class to get the planned production periods of a machine from a date/time, see
  /// <see cref="ProductionProjection"/>
  ///
  /// The observation state slots are planned a few days ahead from the machine state
  /// templates. A slot is production when it says so, else when its machine observation
  /// state is production
  /// </summary>
  public sealed class ProductionProjectionFrom
    : IRequest<ProductionProjection>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionProjectionFrom).FullName);

    readonly IMachine m_machine;
    readonly DateTime m_from;
    readonly TimeSpan m_period;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="from">in UTC</param>
    /// <param name="period">how far after from the observation state slots are read</param>
    public ProductionProjectionFrom (IMachine machine, DateTime from, TimeSpan period)
    {
      if (machine is null) {
        log.Fatal ("ProductionProjectionFrom: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_from = from;
      m_period = period;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public ProductionProjection Get ()
    {
      return GetAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null</returns>
    public async Task<ProductionProjection> GetAsync ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.ProductionProjectionFrom")) {
          var range = new UtcDateTimeRange (m_from, m_from.Add (m_period));
          var slots = await ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRangeAsync (m_machine, range);
          var periods = slots
            .Select (s => (s.DateTimeRange, s.Production ?? s.MachineObservationState?.IsProduction ?? false))
            .ToList ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetAsync: machine {m_machine.Id} from {m_from}: {periods.Count} planned periods, {periods.Count (x => x.Item2)} of production");
          }
          return new ProductionProjection (periods);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineObservationState.ProductionProjectionFrom.{m_machine.Id}.{m_from.Ticks}.{m_period.Ticks}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// The planned observation state slots may be changed at any time
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (ProductionProjection data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<ProductionProjection> data) => true;
  }
}
