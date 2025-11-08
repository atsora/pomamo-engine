// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineObservationState
{
  /// <summary>
  /// Request class to get the no production periods
  /// </summary>
  public sealed class ProductionPeriods
    : IRequest<IEnumerable<(UtcDateTimeRange, bool?)>>
  {
    readonly IMachine m_machine;
    readonly UtcDateTimeRange m_range;
    readonly UtcDateTimeRange m_preLoadRange = null;
    readonly Func<IEnumerable<IObservationStateSlot>> m_observationStateSlotsPreLoader = null;

    readonly ILog log = LogManager.GetLogger (typeof (ProductionPeriods).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <param name="preLoadRange">must include range if not null</param>
    /// <param name="observationStateSlotsPreLoader">Optional: pre-load of observation state slots if required</param>
    public ProductionPeriods (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null, Func<IEnumerable<IObservationStateSlot>> observationStateSlotsPreLoader = null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != range);

      log = LogManager.GetLogger ($"{typeof (ProductionPeriods).FullName}.{machine.Id}");

      if (null != preLoadRange) {
        if (!preLoadRange.ContainsRange (range)) {
          log.Fatal ($"ProductionPeriods: pre-load range {preLoadRange} does not contain {range}");
          Debug.Assert (false);
          throw new ArgumentException ("Pre-load range does not contain range", "range");
        }
      }

      m_machine = machine;
      m_range = range;
      m_preLoadRange = preLoadRange;
      m_observationStateSlotsPreLoader = observationStateSlotsPreLoader;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(UtcDateTimeRange, bool?)> Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      if (m_preLoadRange is null) {
        IEnumerable<IObservationStateSlot> slots;
        if (m_observationStateSlotsPreLoader is null) {
          using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
            slots = ModelDAO.ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (m_machine, m_range);
          }
        }
        else {
          slots = m_observationStateSlotsPreLoader ();
        }
        return slots
          .Select (s => (s.DateTimeRange, s.Production))
          .ToList ();
      }
      else {
        var preLoadProductionPeriodsRequest = new ProductionPeriods (m_machine, m_preLoadRange, null, m_observationStateSlotsPreLoader);
        var preLoadProductionPeriods = Lemoine.Business.ServiceProvider
          .Get (preLoadProductionPeriodsRequest);
        return preLoadProductionPeriods
          .Where (x => x.Item1.Overlaps (m_range))
          .Select (x => (new UtcDateTimeRange (x.Item1.Intersects (m_range)), x.Item2))
          .Where (x => !x.Item1.IsEmpty ())
          .ToList ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IEnumerable<(UtcDateTimeRange, bool?)>> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      if (m_preLoadRange is null) {
        IEnumerable<IObservationStateSlot> slots;
        if (m_observationStateSlotsPreLoader is null) {
          using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
            slots = await ModelDAO.ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRangeAsync (m_machine, m_range);
          }
        }
        else {
          slots = m_observationStateSlotsPreLoader ();
        }
        return slots
          .Select (s => (s.DateTimeRange, s.Production))
          .ToList ();
      }
      else {
        var preLoadProductionPeriodsRequest = new ProductionPeriods (m_machine, m_preLoadRange, null, m_observationStateSlotsPreLoader);
        var preLoadProductionPeriods = await Lemoine.Business.ServiceProvider
          .GetAsync (preLoadProductionPeriodsRequest);
        return preLoadProductionPeriods
          .Where (x => x.Item1.Overlaps (m_range))
          .Select (x => (new UtcDateTimeRange (x.Item1.Intersects (m_range)), x.Item2))
          .Where (x => !x.Item1.IsEmpty ())
          .ToList ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineObservationState.ProductionPeriods.{m_machine.Id}.{m_range}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<(UtcDateTimeRange, bool?)> data)
    {
      if (m_preLoadRange is null) {
        return CacheTimeOut.Config.GetTimeSpan ();
      }
      else { // Do not keep it if a pre-load range is used
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<(UtcDateTimeRange, bool?)>> data)
    {
      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }
}
