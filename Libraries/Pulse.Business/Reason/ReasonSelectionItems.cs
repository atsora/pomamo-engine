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
  public sealed class ReasonSelectionItems
    : IRequest<IEnumerable<IReasonSelection>>
  {
    readonly IMonitoredMachine m_machine;
    readonly IRole m_role;
    readonly IMachineMode m_machineMode;
    readonly IMachineObservationState m_machineObservationState;
    readonly bool m_includeExtraAutoReasons;
    readonly UtcDateTimeRange m_range;

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSelectionItems).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public ReasonSelectionItems (IMonitoredMachine machine, IRole role, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      m_machine = machine;
      m_role = role;
      m_machineMode = machineMode;
      m_machineObservationState = machineObservationState;
      m_includeExtraAutoReasons = includeExtraAutoReasons;
      m_range = range;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Group or null if not found</returns>
    public IEnumerable<IReasonSelection> Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: machineId={m_machine.Id} roleId={m_role?.Id ?? 0} machineModeId={m_machineMode.Id} machineObservationStateId={m_machineObservationState.Id} includeExtraAutoReason={m_includeExtraAutoReasons}");
      }

      IEnumerable<IReasonSelection> reasonSelections = new List<IReasonSelection> ();
      var reasonSelectionExtensions = GetReasonSelectionExtensions (m_machine);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var extraReasonSelections = reasonSelectionExtension
          .GetReasonSelections (m_role, m_range, m_machineMode, m_machineObservationState, m_includeExtraAutoReasons);
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
    public async Task<IEnumerable<IReasonSelection>> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: machineId={m_machine.Id} role={m_role?.Id ?? 0} machineModeId={m_machineMode.Id} machineObservationStateId={m_machineObservationState.Id} includeExtraAutoReason={m_includeExtraAutoReasons}");
      }

      IEnumerable<IReasonSelection> reasonSelections = new List<IReasonSelection> ();
      var reasonSelectionExtensions = await GetReasonSelectionExtensionsAsync (m_machine);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var extraReasonSelections = reasonSelectionExtension
          .GetReasonSelections (m_role, m_range, m_machineMode, m_machineObservationState, m_includeExtraAutoReasons);
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
      => others.Any (b => !b.TimeDependent && !b.DynamicData && (b.ReasonScore >= a.ReasonScore) && (b.Reason.Id == a.Reason.Id));

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
    public string GetCacheKey () => $"Business.Reason.ReasonSelectionItems.{m_machine.Id}.{m_machineMode.Id}.{m_machineObservationState.Id}.{m_includeExtraAutoReasons}";

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<IReasonSelection>> data) => true;

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<IReasonSelection> reasonSelections)
    {
      if (reasonSelections.Any (x => x.TimeDependent || x.DynamicData)) {
        return TimeSpan.Zero;
      }

      var reasonSelectionExtensions = GetReasonSelectionExtensions (m_machine);
      if (reasonSelectionExtensions.All (x => !x.TimeDependent && !x.DynamicData)) {
        return CacheTimeOut.Config.GetTimeSpan ();
      }

      var possibleReasonSelectionsRequest = new PossibleReasonSelections (m_machine, m_machineMode, m_machineObservationState, m_includeExtraAutoReasons);
      var possibleReasonSelections = Lemoine.Business.ServiceProvider.Get (possibleReasonSelectionsRequest);
      if (possibleReasonSelections.All (x => !x.TimeDependent && !x.DynamicData)) {
        return CacheTimeOut.Config.GetTimeSpan ();
      }
      else {
        return TimeSpan.Zero;
      }
    }
    #endregion // IRequest implementation
  }
}
