// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using Pulse.Extensions.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Request class to get all the reason selection items for a specific period
  /// with an optimized cache management
  /// </summary>
  public sealed class PossibleReasonSelections
    : IRequest<IList<IReasonSelection>>
  {
    readonly IMonitoredMachine m_machine;
    readonly IMachineMode m_machineMode;
    readonly IMachineObservationState m_machineObservationState;
    readonly bool m_includeExtraAutoReasons;

    static readonly ILog log = LogManager.GetLogger (typeof (PossibleReasonSelections).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public PossibleReasonSelections (IMonitoredMachine machine, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      m_machine = machine;
      m_machineMode = machineMode;
      m_machineObservationState = machineObservationState;
      m_includeExtraAutoReasons = includeExtraAutoReasons;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public IList<IReasonSelection> Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: machineId={m_machine.Id} machineModeId={m_machineMode.Id} machineObservationStateId={m_machineObservationState.Id} includeExtraAutoReason={m_includeExtraAutoReasons}");
      }

      IEnumerable<IReasonSelection> reasonSelections = new List<IReasonSelection> ();
      var reasonSelectionExtensions = GetReasonSelectionExtensions (m_machine);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var extraReasonSelections = reasonSelectionExtension
          .GetPossibleReasonSelections (m_machineMode, m_machineObservationState, m_includeExtraAutoReasons);
        reasonSelections = reasonSelections
          .Concat (extraReasonSelections);
      }
      reasonSelections = reasonSelections.ToList ();
      return reasonSelections
        .Where (x => !x.TimeDependent || x.DynamicData || !ExistsTimeIndependentHigherScore (x, reasonSelections))
        .Distinct (new ReasonSelectionReasonEqualityComparer ())
        .ToList ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IList<IReasonSelection>> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: machineId={m_machine.Id} machineModeId={m_machineMode.Id} machineObservationStateId={m_machineObservationState.Id} includeExtraAutoReason={m_includeExtraAutoReasons}");
      }

      IEnumerable<IReasonSelection> reasonSelections = new List<IReasonSelection> ();
      var reasonSelectionExtensions = await GetReasonSelectionExtensionsAsync (m_machine);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var extraReasonSelections = reasonSelectionExtension
          .GetPossibleReasonSelections (m_machineMode, m_machineObservationState, m_includeExtraAutoReasons);
        reasonSelections = reasonSelections
          .Concat (extraReasonSelections);
      }
      reasonSelections = reasonSelections.ToList ();
      return reasonSelections
        .Where (x => !x.TimeDependent || x.DynamicData || !ExistsTimeIndependentHigherScore (x, reasonSelections))
        .Distinct (new ReasonSelectionReasonEqualityComparer ())
        .ToList ();
    }

    bool ExistsTimeIndependentHigherScore (IReasonSelection a, IEnumerable<IReasonSelection> others)
      => others.Any (b => !b.TimeDependent && !b.DynamicData && (b.ReasonScore >= a.ReasonScore) && (b.Reason.Id == a.Reason.Id) );

    IEnumerable<IReasonSelectionExtension> GetReasonSelectionExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      return Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.MonitoredMachineExtensions<IReasonSelectionExtension> (machine, (x, m) => x.Initialize (m)));
    }

    async Task<IEnumerable<IReasonSelectionExtension>> GetReasonSelectionExtensionsAsync (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      return await Lemoine.Business.ServiceProvider
        .GetAsync (new Lemoine.Business.Extension.MonitoredMachineExtensions<IReasonSelectionExtension> (machine, (x, m) => x.Initialize (m)));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey () => $"Business.Reason.PossibleReasonSelections.{m_machine.Id}.{m_machineMode.Id}.{m_machineObservationState.Id}.{m_includeExtraAutoReasons}";

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IList<IReasonSelection>> data) => true;

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IList<IReasonSelection> data) => CacheTimeOut.Config.GetTimeSpan ();
    #endregion // IRequest implementation
  }
}
