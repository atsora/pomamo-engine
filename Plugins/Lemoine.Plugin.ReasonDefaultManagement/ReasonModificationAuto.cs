// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;
using Lemoine.Business;
using Lemoine.Business.Reason;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Reasons with an auto modification
  /// </summary>
  public sealed class ReasonModificationAuto
    : Lemoine.Extensions.NotConfigurableExtension
    , IReasonExtension
    , IReasonSelectionExtension
    , IReasonLegendExtension
  {
    ILog log = LogManager.GetLogger (typeof (ReasonModificationAuto).FullName);

    IMachine m_machine;
    IEnumerable<IAutoReasonExtension> m_autoReasonExtensions = null;
    bool m_batch = false;
    UtcDateTimeRange m_cacheRange;
    IEnumerable<IReasonProposal> m_cacheData = null;
    bool? m_timeDependent = null; // To keep it in cache

    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonModificationAuto ()
    {
    }

    void ResetCache ()
    {
      m_cacheRange = new UtcDateTimeRange ();
      m_cacheData = null;
    }

    IEnumerable<IReasonProposal> GetAutoReasonProposals (UtcDateTimeRange range)
    {
      if (m_batch) {
        if (null != m_cacheData) {
          if (UtcDateTimeRange.Equals (m_cacheRange, range)) {
            return m_cacheData;
          }
          else if (m_cacheRange.ContainsRange (range)) {
            return m_cacheData
              .Where (x => range.Overlaps (x.DateTimeRange));
          }
        }
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ReasonModificationAuto.GetAutoReasonProposals")) {
          IEnumerable<IReasonProposal> reasonProposals = ModelDAOHelper.DAOFactory.ReasonProposalDAO
            .FindOverlapsRange (m_machine, range);
          reasonProposals = reasonProposals.Where (x => x.Kind.IsAuto ());
          if (m_batch) { // Only use the cache in case of batch
            m_cacheRange = range;
            m_cacheData = reasonProposals.ToList ();
          }
          return reasonProposals;
        }
      }
    }

    /// <summary>
    /// Auto-Reason extensions
    /// </summary>
    /// <returns></returns>
    IEnumerable<IAutoReasonExtension> GetAutoReasonExtensions ()
    {
      Debug.Assert (null != m_machine);

      if (null == m_autoReasonExtensions) {
        m_autoReasonExtensions = GetAutoReasonExtensions (m_machine)
          .ToList ();
      }
      return m_autoReasonExtensions;
    }

    IEnumerable<IAutoReasonExtension> GetAutoReasonExtensions (IMachine machine)
    {
      var monitoredMachineRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (machine.Id);
      var monitoredMachine = Lemoine.Business.ServiceProvider
        .Get (monitoredMachineRequest);
      if (null != monitoredMachine) {
        return GetAutoReasonExtensions (monitoredMachine);
      }
      else {
        return new List<IAutoReasonExtension> ();
      }
    }

    IEnumerable<IAutoReasonExtension> GetAutoReasonExtensions (IMonitoredMachine monitoredMachine)
    {
      Debug.Assert (null != monitoredMachine);

      var request = new Lemoine.Business.Extension
        .MonitoredMachineExtensions<IAutoReasonExtension> (monitoredMachine,
        (ext, m) => ext.Initialize (m, null));
      return Lemoine.Business.ServiceProvider
        .Get (request);
    }

    #region IPossibleReasonExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (typeof (ReasonModificationAuto).FullName + "." + machine.Id);

      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    public void StartBatch ()
    {
      m_batch = true;
      ResetCache ();
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="range"></param>
    public void PreLoad (UtcDateTimeRange range)
    {
      Debug.Assert (m_batch);

      if (m_batch) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("ReasonModificationAuto.PreLoad")) {
            m_cacheData = ModelDAOHelper.DAOFactory.ReasonProposalDAO
              .FindOverlapsRange (m_machine, range)
              .Where (x => x.Kind.IsAuto ())
              .ToList ();
            m_cacheRange = range;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    public void EndBatch ()
    {
      m_batch = false;
      ResetCache ();
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public double? GetMaximumScore (IReasonSlot newReasonSlot)
    {
      // Approximative, but this is enough...
      var reasonProposals = GetAutoReasonProposals (newReasonSlot.DateTimeRange);
      if (!reasonProposals.Any ()) {
        return -1.0; // None
      }
      else {
        return reasonProposals.Max (m => m.ReasonScore);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      var autoReasonProposals = GetAutoReasonProposals (newReasonSlot.DateTimeRange);
      return autoReasonProposals.Any ();
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="oldReasonSlot">nullable</param>
    /// <param name="newReasonSlot">not null</param>
    /// <param name="modification"></param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      var result = RequiredResetKind.None;

      if (reasonSlotChange.HasFlag (ReasonSlotChange.Period)
          || reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
        result = result
          .Add (GetRequiredResetKindForPeriodChange (oldReasonSlot, newReasonSlot, modification, reasonSlotChange));
      }
      if (result.HasFlag (RequiredResetKind.Main) && result.HasFlag (RequiredResetKind.ExtraAuto)) {
        return result;
      }

      if ((null != modification) && (modification is IReasonMachineAssociation)) {
        return result; // Everything else is managed in MergeDataWithOldSlot
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode)
        || reasonSlotChange.HasFlag (ReasonSlotChange.MachineObservationState)) {
        var reasonProposals = GetAutoReasonProposals (newReasonSlot.DateTimeRange);
        if (!reasonProposals.Any ()) {
          return RequiredResetKind.None;
        }
        if ((null == oldReasonSlot) && (0 < newReasonSlot.AutoReasonNumber)) {
          result = result.Add (RequiredResetKind.ExtraAuto);
        }
        foreach (var autoReasonExtension in GetAutoReasonExtensions ()) {
          // Is there a potential other auto-reason with a higher score ?
          if (autoReasonExtension.CanOverride (newReasonSlot)) {
            return RequiredResetKind.Full;
          }
          if (null == oldReasonSlot) {
            if (newReasonSlot.ReasonSource.IsAuto ()) { // May be an auto-reason to refresh
              return RequiredResetKind.Full; // Approximative
            }
            if (autoReasonExtension.IsValidMatch (newReasonSlot.MachineMode, newReasonSlot.MachineObservationState, newReasonSlot.Reason, newReasonSlot.ReasonScore)) {
              return RequiredResetKind.Full;
            }
          }
          else { // null != oldReasonSlot
            if (autoReasonExtension.IsValidMatch (oldReasonSlot.MachineMode, oldReasonSlot.MachineObservationState, oldReasonSlot.Reason, oldReasonSlot.ReasonScore)) {
              return RequiredResetKind.Full;
            }
            else {
              // Can this auto-reason be one of the extra auto-reason of the old slot
              if (autoReasonExtension.IsValidExtraAutoReason (oldReasonSlot)) {
                result = result.Add (RequiredResetKind.ExtraAuto);
              }
            }
          }
          // Can this auto-reason affect the auto reason number
          if (autoReasonExtension.IsValidExtraAutoReason (newReasonSlot)) {
            result = result.Add (RequiredResetKind.ExtraAuto);
          }
        }
      }

      return result;
    }

    RequiredResetKind GetRequiredResetKindForPeriodChange (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      var newPeriod = newReasonSlot.DateTimeRange;
      var newAutoReasonProposals = GetAutoReasonProposals (newPeriod);
      if (newAutoReasonProposals.Any ()) {
        if (reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
          return RequiredResetKind.Main | RequiredResetKind.ExtraAuto;
        }
        if (newAutoReasonProposals.Any (p => !p.DateTimeRange.ContainsRange (newPeriod))) {
          return RequiredResetKind.Main | RequiredResetKind.ExtraAuto;
        }
        var result = RequiredResetKind.None;
        var autoReasonNumber = newReasonSlot.AutoReasonNumber;
        if (newReasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
          --autoReasonNumber;
        }
        var count = newAutoReasonProposals.Count ();
        if (count != autoReasonNumber) {
          result = result.Add (RequiredResetKind.ExtraAuto);
        }
        if (newReasonSlot.ReasonSource.IsAuto ()
          && !newReasonSlot.ReasonSource.IsDefault ()) {
          var topAutoReasonProposal = newAutoReasonProposals
            .OrderByDescending (p => p.ReasonScore)
            .First ();
          if (topAutoReasonProposal.Reason.Id != newReasonSlot.Reason.Id) {
            result = result.Add (RequiredResetKind.Main);
          }
        }
        return result;
      }

      // Else no AutoReasonProposal
      if (null == oldReasonSlot) {
        if (newReasonSlot.ReasonSource.IsDefault ()) { // No auto-reason to remove
          return RequiredResetKind.None;
        }
        if (0 == newReasonSlot.AutoReasonNumber) { // No auto-reason to remove
          return RequiredResetKind.None;
        }
        if ((1 == newReasonSlot.AutoReasonNumber)
          && (newReasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto))) {
          return RequiredResetKind.None;
        }
        if (newReasonSlot.ReasonSource.IsAuto ()) { // May be an auto-reason to refresh
          return RequiredResetKind.Full; // Approximative
        }
        else if (0 < newReasonSlot.AutoReasonNumber) { // Extra auto-reasons
          return RequiredResetKind.ExtraAuto;
        }
        else {
          return RequiredResetKind.None; // No auto-reason to remove
        }
      }
      else { // null != oldReasonSlot
        if (oldReasonSlot.ReasonSource.IsDefault ()) { // No auto-reason to remove
          return RequiredResetKind.None;
        }
        if (0 == oldReasonSlot.AutoReasonNumber) { // No auto-reason to remove
          return RequiredResetKind.None;
        }
        if ((1 == oldReasonSlot.AutoReasonNumber)
          && (oldReasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto))) {
          return RequiredResetKind.None;
        }
        if (oldReasonSlot.ReasonSource.IsAuto ()) {
          var oldPeriod = oldReasonSlot.DateTimeRange;
          var oldAutoReasonProposals = GetAutoReasonProposals (oldPeriod);
          if (oldAutoReasonProposals.Any ()) {
            // A litle bit approximative but probably sufficient today
            return RequiredResetKind.Full;
          }
          else { // No reason-proposal during the old and new period
            return RequiredResetKind.None;
          }
        }
        else if (0 < oldReasonSlot.AutoReasonNumber) { // Extra auto-reasons
          return RequiredResetKind.ExtraAuto;
        }
        else {
          return RequiredResetKind.None; // No auto-reason to remove
        }
      }

      // Remove the following code since a warning is raised:
      // CS0162: Impossible d atteindre le code detecte
      /*
      log.FatalFormat ("GetRequiredResetKindForPeriodChange: this code should not be reached");
      Debug.Assert (false);
      return RequiredResetKind.None;
      */
    }

    /// <summary>
    /// <see cref="IReasonExtension"/>
    /// </summary>
    /// <param name="reasonSource"></param>
    /// <param name="reasonScore"></param>
    /// <param name="autoReasonNumber"></param>
    /// <returns></returns>
    public bool IsResetApplicable (ReasonSource reasonSource, double reasonScore, int autoReasonNumber)
    {
      if (reasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber)) {
        return true;
      }
      if (1 < autoReasonNumber) {
        return true;
      }
      else if (1 == autoReasonNumber) {
        return !reasonSource.HasFlag (ReasonSource.DefaultIsAuto);
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Update the default reason following a change in the reason
    ///
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="reasonSlot">reason slot (not null)</param>
    /// <returns></returns>
    public void TryResetReason (ref Lemoine.Model.IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);

      var autoReasonProposals = GetAutoReasonProposals (reasonSlot.DateTimeRange)
        .OrderByDescending (m => m.ReasonScore);
      if (!autoReasonProposals.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("TryResetReason: no auto-reason proposal during that period => return");
        }
        return;
      }

      var range = reasonSlot.DateTimeRange;
      // Make it in two steps: determine first the applicable range
      foreach (var autoReasonProposal in autoReasonProposals) {
        if (!autoReasonProposal.DateTimeRange.Overlaps (range)) {
          // The association does not match the new range, skip it
          continue;
        }
        Debug.Assert (autoReasonProposal.DateTimeRange.Overlaps (range));
        if (!autoReasonProposal.DateTimeRange.ContainsRange (range)) {
          range = new UtcDateTimeRange (autoReasonProposal.DateTimeRange.Intersects (range));
          if (log.IsDebugEnabled) {
            log.Debug ($"TryResetReason: restrict range to {range} because ot autoReasonProposal {autoReasonProposal.Id}");
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryResetReason: do not restrict range {range} for autoReasonProposal {autoReasonProposal.Id}");
          }
        }
      }
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (reasonSlot.DateTimeRange.ContainsRange (range));

      bool isCurrentActivity = IsCurrentActivity (range);
      if (reasonSlot.DateTimeRange.Equals (range)) {
        bool autoReasonApplied = false;
        var oldSlot = (ISlot)reasonSlot.Clone ();
        foreach (var autoReasonProposal in autoReasonProposals) {
          if (!autoReasonProposal.DateTimeRange.ContainsRange (range)) {
            log.Fatal ($"TryResetReason: unexpected, reason proposal {autoReasonProposal.Id} {autoReasonProposal.DateTimeRange} does not contain range {range}");
            Debug.Assert (false);
            continue;
          }
          var consolidationLimit = range.Upper;
          if (isCurrentActivity) {
            consolidationLimit = autoReasonProposal.DateTimeRange.Upper;
          }
          autoReasonApplied |= this.TryAutoReason (reasonSlot, autoReasonProposal,
            consolidationLimit,
            false);
        }
        if (autoReasonApplied) { // Consolidate
          reasonSlot.Consolidate (oldSlot, null);
        }
      }
      else { // !reasonSlot.DateTimeRange.Equals (range)
        foreach (var autoReasonProposal in autoReasonProposals
          .Where (p => p.DateTimeRange.Overlaps (range))) {
          if (!autoReasonProposal.DateTimeRange.ContainsRange (range)) {
            log.FatalFormat ("TryResetReason: unexpected, reason proposal {0} {1} does not contain range {2}",
              autoReasonProposal.Id, autoReasonProposal.DateTimeRange, range);
            Debug.Assert (false);
            continue;
          }
          var associationRange = range;
          if (isCurrentActivity) {
            var adjustedUpperBound = autoReasonProposal.DateTimeRange.Upper;
            associationRange = new UtcDateTimeRange (range.Lower, adjustedUpperBound);
          }
          var association = ModelDAOHelper.ModelFactory
            .CreateReasonMachineAssociation (m_machine, associationRange);
          association.SetAutoReason (autoReasonProposal.Reason, autoReasonProposal.ReasonScore,
            autoReasonProposal.Kind.IsOverwriteRequired (), autoReasonProposal.ReasonDetails, autoReasonProposal.JsonData);
          association.Option = AssociationOption.FinalProcess | AssociationOption.NoCompatibilityCheck;
          association.Apply ();
          ModelDAOHelper.DAOFactory.Flush ();
        } // Loop on autoReasonProposals
        ModelDAOHelper.DAOFactory.Flush ();
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRange (reasonSlot.Machine, range);
        Debug.Assert (1 == reasonSlots.Count ());
        reasonSlot = reasonSlots.First ();
      }

      // At least one auto-reason should have been applied in range
      Debug.Assert (0 < reasonSlot.AutoReasonNumber);
      if (reasonSlot.AutoReasonNumber <= 0) {
        log.FatalFormat ("TryResetReason: reason slot {0} does not have any auto-reason number, although it was expected, considered range was {1}", reasonSlot, range);
      }
    }

    bool IsCurrentActivity (UtcDateTimeRange range)
    {
      if (!range.Upper.HasValue) {
        log.FatalFormat ("IsCurrentActivity: unexpected range upper value in {0}", range);
        throw new ArgumentOutOfRangeException ("range", "invalid upper value (+oo)");
      }

      var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
        .FindById (m_machine.Id);
      if (null == machineStatus) {
        return false;
      }
      else { // null != machineStatus
        return range.Upper.Value.Equals (machineStatus.ReasonSlotEnd);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="at"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="autoManualOnly"></param>
    /// <returns></returns>
    public IEnumerable<IPossibleReason> TryGetActiveAt (DateTime at, IMachineMode machineMode, IMachineObservationState machineObservationState, bool autoManualOnly)
    {
      Debug.Assert (null != m_machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ReasonWithModification.TryGetActiveAt")) {
          var range = new UtcDateTimeRange (at, at, "[]");
          var autoReasonMachineAssociations = GetAutoReasonProposals (range);
          return autoReasonMachineAssociations.Cast<IPossibleReason> ();
        }
      }
    }

    /// <summary>
    /// Is a reason compatible with the machine mode and the machine observation state ?
    /// 
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="reasonSource"></param>
    /// <returns></returns>
    public bool IsCompatible (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, ReasonSource reasonSource)
    {
      Debug.Assert (null != reason);

      if (!reasonSource.IsAuto ()) { // Not applicable
        return false;
      }
      else { // Auto
        var autoReasonExtensions = GetAutoReasonExtensions ();
        var compatibleFromExtensions = autoReasonExtensions.Any (ext => ext.IsValidMatch (machineMode, machineObservationState, reason, reasonScore));
        if (compatibleFromExtensions) {
          return true;
        }
        else { // Check if there is a matching reason proposal
          var reasonProposals = GetAutoReasonProposals (range);
          return reasonProposals.Any (p => p.DateTimeRange.ContainsRange (range)
            && (p.Reason.Id == reason.Id) && (p.ReasonScore == reasonScore));
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    public bool TimeDependent
    {
      get {
        if (!m_timeDependent.HasValue) {
          m_timeDependent = GetAutoReasonExtensions ().Any ();
        }
        return m_timeDependent.Value;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    public bool DynamicData => false;

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="includeExtraAutoReasons"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSelection> GetReasonSelections (IMachine machine, IRole role, UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
    {
      if (!includeExtraAutoReasons) {
        return new List<IReasonSelection> ();
      }

      var autoReasonProposal = GetAutoReasonProposals (range)
        .Where (a => a.DateTimeRange.ContainsRange (range));
      var extraReasonSelections = autoReasonProposal
        .Select (a => new ExtraReasonSelection (machineMode, machineObservationState, a.Reason, GetManualScore (machineMode, machineObservationState, a) ?? -1, null, a.Data, timeDependent: true));
      return extraReasonSelections
        .Where (s => 0 < s.ReasonScore)
        .Cast<IReasonSelection> ();
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    public IEnumerable<IReasonSelection> GetPossibleReasonSelections (IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
    {
      if (!includeExtraAutoReasons) {
        return new List<IReasonSelection> ();
      }

      var autoReasonExtensions = GetAutoReasonExtensions ();
      var result = new List<IReasonSelection> ();
      foreach (var ext in autoReasonExtensions.Where (x => x.ManualScore.HasValue)) {
        if (ext.IsValidMatch (machineMode, machineObservationState, ext.Reason, ext.ReasonScore)) {
          // AlternativeText and Data is not included here, but setting DependentTime to true should be enough for the moment
          // TODO: check for alternative text
          result.Add (new ExtraReasonSelection (machineMode, machineObservationState, ext.Reason, ext.ManualScore.Value, timeDependent: true));
        }
      }
      return result;
    }

    double? GetManualScore (IMachineMode machineMode, IMachineObservationState machineObservationState, IReasonProposal reasonProposal)
    {
      var autoReasonExtensionsWithManualScore = GetAutoReasonExtensions ()
        .Where (ext => ext.ManualScore.HasValue);
      var validReasonExtensions = autoReasonExtensionsWithManualScore
        .Where (ext => ext.IsValidMatch (machineMode, machineObservationState, reasonProposal.Reason, reasonProposal.ReasonScore));
      if (validReasonExtensions.Any ()) {
        return validReasonExtensions
          .Max (ext => ext.ManualScore.Value);
      }
      else {
        return null;
      }
    }
    #endregion

    #region IReasonLegendExtension implementation
    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public bool Initialize ()
    {
      return true;
    }

    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReason> GetUsedReasons ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        HashSet<int> reasonIds = new HashSet<int> ();

        var autoReasonExtensionsRequest = new Lemoine.Business.Extension
          .GlobalExtensions<IAutoReasonExtension> ();
        var autoReasonExtensions = Lemoine.Business.ServiceProvider
          .Get (autoReasonExtensionsRequest);

        // Try to optimize it a little
        var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
        foreach (var autoReasonExtension in autoReasonExtensions) {
          var reason = GetReasonFromExtension (autoReasonExtension, monitoredMachines);
          if (null != reason) {
            reasonIds.Add (reason.Id);
          }
        }

        // Note: because some of the returned reasons may not be attached to this session,
        // this is required to early fetch the reason groups
        HashSet<IReason> reasons = new HashSet<IReason> ();
        foreach (var reasonId in reasonIds) {
          var reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (reasonId);
          ModelDAOHelper.DAOFactory.Initialize (reason.ReasonGroup);
          reasons.Add (reason);
        }
        return reasons;
      }
    }

    IReason GetReasonFromExtension (IAutoReasonExtension autoReasonExtension, IList<IMonitoredMachine> monitoredMachines)
    {
      if (null != autoReasonExtension.Reason) {
        return autoReasonExtension.Reason;
      }

      foreach (var monitoredMachine in monitoredMachines) {
        if (autoReasonExtension.Initialize (monitoredMachine, null)) {
          if (null != autoReasonExtension.Reason) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetReasonFromExtension: add reason {autoReasonExtension.Reason.Id} for extension {autoReasonExtension}");
            }
            return autoReasonExtension.Reason;
          }
        }
      }

      return null;
    }
    #endregion // IReasonLegendExtension implementation
  }
}
