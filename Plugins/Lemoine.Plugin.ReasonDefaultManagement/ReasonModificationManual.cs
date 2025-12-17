// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Reasons with a manual modification
  /// </summary>
  public sealed class ReasonModificationManual
    : Lemoine.Extensions.NotConfigurableExtension
    , IReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (ReasonModificationManual).FullName);

    IMachine m_machine;
    IEnumerable<IReasonExtension> m_reasonExtensions = null;
    IEnumerable<IReasonSelectionExtension> m_reasonSelectionExtensions = null;
    bool m_batch = false;
    UtcDateTimeRange m_cacheRange;
    IEnumerable<IReasonProposal> m_cacheData = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonModificationManual ()
    {
    }

    #region Methods
    void ResetCache ()
    {
      m_cacheRange = new UtcDateTimeRange ();
      m_cacheData = null;
    }

    IEnumerable<IReasonProposal> GetManualReasonProposals (UtcDateTimeRange range)
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
        using (var transaction = session.BeginReadOnlyTransaction ("ReasonModificationManual.GetManualReasonProposals")) {
          IEnumerable<IReasonProposal> reasonProposals = ModelDAOHelper.DAOFactory.ReasonProposalDAO
            .FindManualOverlapsRange (m_machine, range);
          if (m_batch) { // Only use the cache in case of batch
            m_cacheRange = range;
            m_cacheData = reasonProposals.ToList ();
          }
          return reasonProposals;
        }
      }
    }

    /// <summary>
    /// Reason extensions
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonExtension> GetReasonExtensions ()
    {
      if (null == m_reasonExtensions) {
        var request = new Lemoine.Business.Extension
          .MachineExtensions<IReasonExtension> (m_machine,
          (ext, m) => ext.Initialize (m));
        m_reasonExtensions = Lemoine.Business.ServiceProvider
          .Get (request);
      }
      return m_reasonExtensions;
    }

    /// <summary>
    /// ReasonSelection extensions
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonSelectionExtension> GetReasonSelectionExtensions ()
    {
      if (null == m_reasonSelectionExtensions) {
        var request = new Lemoine.Business.Extension
          .MachineExtensions<IReasonSelectionExtension> (m_machine,
          (ext, m) => ext.Initialize (m));
        m_reasonSelectionExtensions = Lemoine.Business.ServiceProvider
          .Get (request);
      }
      return m_reasonSelectionExtensions;
    }
    #endregion // Methods

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
      log = LogManager.GetLogger (typeof (ReasonModificationManual).FullName + "." + machine.Id);

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
              .Where (x => x.Kind.IsManual ())
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
      var manualReasonProposals = GetManualReasonProposals (newReasonSlot.DateTimeRange);
      if (!manualReasonProposals.Any ()) {
        return -1.0; // None
      }
      else {
        return manualReasonProposals.Max (m => m.ReasonScore);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      // Approximative, but this is enough...
      var manualReasonProposals = GetManualReasonProposals (newReasonSlot.DateTimeRange);
      return manualReasonProposals.Any ();
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

    bool IsActiveForNewActivity (IReasonSlot newReasonSlot)
    {
      // New activity => consider only the manual reasons and only if there the current reason slot corresponds to a manual reason
      var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
        .FindById (m_machine.Id);
      if (null != machineStatus) {
        if (machineStatus.ReasonSource.HasFlag (ReasonSource.Manual)
          && (UpperBound.Compare<DateTime> (newReasonSlot.BeginDateTime.Value, machineStatus.ConsolidationLimit) < 0)) {
          return true;
        }
        else {
          return false;
        }
      }
      return true; // Default
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="oldReasonSlot">nullable</param>
    /// <param name="newReasonSlot"></param>
    /// <param name="modification"></param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      if ((null != modification) && (modification is IReasonMachineAssociation)) {
        return RequiredResetKind.None; // Everything is managed in MergeDataWithOldSlot
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
        if (reasonSlotChange.HasFlag (ReasonSlotChange.Period)) { // Period change means the period is longer because a new activity was detected
          if (newReasonSlot.ReasonSource.IsDefault ()) {
            return RequiredResetKind.None;
          }
          if (!newReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
            return RequiredResetKind.None;
          }
          else if (!newReasonSlot.ReasonSource.Equals (ReasonSource.Manual)) { // Extra manual
            return RequiredResetKind.ExtraManual;
          }
        }
        else { // No period change, meaning a new machine mode is detected
          if (!IsActiveForNewActivity (newReasonSlot)) {
            return RequiredResetKind.None;
          }
        }
      }

      var result = RequiredResetKind.None;

      if (reasonSlotChange.HasFlag (ReasonSlotChange.Period)) {
        result = GetRequiredResetKindForPeriodChange (oldReasonSlot, newReasonSlot, modification, reasonSlotChange);
      }
      if (result.Equals (RequiredResetKind.Full)) {
        return result;
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode)
        || reasonSlotChange.HasFlag (ReasonSlotChange.MachineObservationState)) {
        // Check the reason is still valid in case of extra manual reason
        if (newReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
          Debug.Assert (!reasonSlotChange.HasFlag (ReasonSlotChange.ResetManual)); // Managed elsewhere
          var manualReasonProposals = GetManualReasonProposals (newReasonSlot.DateTimeRange);
          foreach (var reasonMachineAssociation in manualReasonProposals.Where (m => null != m.Reason)) {
            // Is the reason still active for this machine mode / machine observation state ?
            var isCompatible = GetReasonExtensions ()
              .Any (ext => ext.IsCompatible (newReasonSlot.DateTimeRange, newReasonSlot.MachineMode, newReasonSlot.MachineObservationState, reasonMachineAssociation.Reason, reasonMachineAssociation.ReasonScore, ReasonSource.Manual));
            log.InfoFormat ("GetRequiredResetKind: old reason is not compatible any more in {0} => return Reset ExtraManual",
              newReasonSlot);
            if (!isCompatible) {
              result = result.Add (RequiredResetKind.ExtraManual);
              break;
            }
          }
        }
      }

      return result;
    }

    RequiredResetKind GetRequiredResetKindForPeriodChange (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      var newPeriod = newReasonSlot.DateTimeRange;
      var newManualReasonProposals = GetManualReasonProposals (newPeriod);
      if (newManualReasonProposals.Any ()) {
        if (1 == newManualReasonProposals.Count ()) {
          var newManualReasonProposal = newManualReasonProposals.First ();
          if (newPeriod.ContainsRange (newManualReasonProposal.DateTimeRange)
            && (newManualReasonProposal.Reason.Id == newReasonSlot.Reason.Id)
            && newReasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
            // If the current reason is already the manual one and if it matches that period,
            // nothing to do...
            return RequiredResetKind.None;
          }
        }
        return RequiredResetKind.Full;
      }
      else { // No ManualReasonProposals
        if (newReasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
          return RequiredResetKind.Full;
        }
        else if (newReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) { // Extra manual
          return RequiredResetKind.ExtraManual;
        }
      }

      return RequiredResetKind.None;
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
      return reasonSource.HasFlag (ReasonSource.Manual) || reasonSource.HasFlag (ReasonSource.UnsafeManualFlag);
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

      IEnumerable<IReasonProposal> manualReasonProposals =
        GetManualReasonProposals (reasonSlot.DateTimeRange)
        .OrderByDescending (m => m.ReasonScore);
      if (!manualReasonProposals.Any ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("TryResetReason: no manual reason proposal during that period => return");
        }
        return;
      }

      var range = reasonSlot.DateTimeRange;
      IReasonMachineAssociation manualReasonAssociation = null;
      foreach (var manualReasonProposal in manualReasonProposals) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryResetReason: check reason={manualReasonProposal.Reason.Id} at {manualReasonProposal.DateTimeRange}");
        }
        // Note: the returned manual reason association may have an analysisstatus=New since
        //       some of them are cloned from the original ones
        if (!manualReasonProposal.DateTimeRange.Overlaps (range)) {
          // The association does not match the new range, skip it
          continue;
        }
        var adjustedManualReasonRange = manualReasonProposal.DateTimeRange;
        if (!manualReasonProposal.DateTimeRange.Overlaps (range)) {
          continue;
        }
        if (!manualReasonProposal.DateTimeRange.ContainsRange (range)) {
          range = new UtcDateTimeRange (manualReasonProposal.DateTimeRange.Intersects (range));
          if (log.IsDebugEnabled) {
            log.Debug ($"TryTresetReason: restrict range to {range} because of reason proposal {manualReasonProposal.Id}");
          }
        }
        manualReasonAssociation = ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (m_machine, range);
        manualReasonAssociation.SetManualReason (manualReasonProposal.Reason, manualReasonProposal.ReasonScore, manualReasonProposal.ReasonDetails, manualReasonProposal.JsonData);
        if (log.IsDebugEnabled) {
          log.Debug ($"TryResetReason: consider manual reason {manualReasonProposal.Reason.Id} score {manualReasonProposal.ReasonScore} for range {range}");
        }
        break;
      }
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (null != manualReasonAssociation);

      if (manualReasonAssociation is null) {
        log.Fatal ($"TryResetReason: no manual reason association for range {reasonSlot.DateTimeRange} although one was expected");
      }
      else { // (null != manualReasonAssociation)
        bool isCurrentActivity = IsCurrentActivity (range);
        if (reasonSlot.DateTimeRange.Equals (range)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryResetReason: range matches, apply directly {manualReasonAssociation.Reason?.Id} score={manualReasonAssociation.ReasonScore}");
          }
          Debug.Assert (manualReasonAssociation.OptionalReasonScore.HasValue);
          Debug.Assert (null != manualReasonAssociation.Reason);
          var consolidationLimit = range.Upper;
          if (isCurrentActivity) {
            consolidationLimit = manualReasonAssociation.End;
          }
          var oldSlot = (reasonSlot.ReasonScore <= manualReasonAssociation.ReasonScore)
            ? (ISlot)reasonSlot.Clone ()
            : null;
          var manualReasonApplied = this.TryManualReason (reasonSlot, manualReasonAssociation,
            consolidationLimit);
          if (manualReasonApplied) { // Consolidate
            reasonSlot.Consolidate (oldSlot, manualReasonAssociation);
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"TryResetReason: apply manual reason {manualReasonAssociation.Reason?.Id} on a sub-range {range} of {reasonSlot.DateTimeRange}");
          }
          Debug.Assert (reasonSlot.DateTimeRange.ContainsRange (range));
          manualReasonAssociation.Option = AssociationOption.FinalProcess;
          if (isCurrentActivity && range.Upper.HasValue) { // Consider applying the machine reason association in the future as well
            if (log.IsDebugEnabled) {
              log.Debug ($"TryResetReason: current activity => apply in the future from {range.Lower}");
            }
            var association = manualReasonAssociation.Clone (new UtcDateTimeRange (range.Lower));
            association.Apply ();
          }
          else { // Restrict it to the range
            manualReasonAssociation.Apply ();
          }
          ModelDAOHelper.DAOFactory.Flush ();
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (reasonSlot.Machine, range);
          if (1 != reasonSlots.Count) {
            log.Fatal ($"TryResetReason: invalid number of reason slots in {range}: {reasonSlots.Count}, a unique slot was expected");
          }
          Debug.Assert (1 == reasonSlots.Count ());
          reasonSlot = reasonSlots.First ();
          if (reasonSlot.Reason.Id != manualReasonAssociation.Reason.Id) {
            log.Error ($"TryResetReason: updated reason slot at {range} has reason {reasonSlot.Reason?.Id} instead of {manualReasonAssociation.Reason?.Id}");
          }
        }
      }

      Debug.Assert (reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual));
      if (!reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
        log.Fatal ($"TryResetReason: reason slot at {reasonSlot?.DateTimeRange} has no manual flag although one should have been applied on range {range}");
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
          var manualReasonProposals = GetManualReasonProposals (range);
          Debug.Assert (manualReasonProposals.Count () <= 1);
          var manualReasonProposal = manualReasonProposals
            .FirstOrDefault ();
          if (null != manualReasonProposal) {
            Debug.Assert (null != manualReasonProposal.Reason);
            // Check if it could be still selected
            var isCompatible = GetReasonSelectionExtensions ()
              .Any (ext => IsMatch (manualReasonProposal, ext.GetReasonSelections (null, range, machineMode, machineObservationState, true)));
            if (isCompatible) {
              return new List<IPossibleReason> { manualReasonProposal };
            }
          }
        }
      }

      return new List<IPossibleReason> ();
    }

    bool IsMatch (IReasonProposal reasonProposal, IEnumerable<IReasonSelection> reasonSelections)
    {
      return reasonSelections.Any (s => IsMatch (reasonProposal, s));
    }

    bool IsMatch (IReasonProposal reasonProposal, IReasonSelection reasonSelection)
    {
      return (null != reasonSelection.Reason)
        && reasonProposal.Reason.Id == reasonSelection.Reason.Id
        && reasonProposal.ReasonScore == reasonSelection.ReasonScore;
    }

    /// <summary>
    /// Is a reason compatible with the range, machine mode and machine observation state ?
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
      if (!reasonSource.Equals (ReasonSource.Manual)) {
        return false;
      }
      var reasonSelectionExtensions = GetReasonSelectionExtensions ();
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var reasonSelections = reasonSelectionExtension.GetReasonSelections (null, range, machineMode, machineObservationState, true);
        if (reasonSelections.Any (s => (null != s.Reason) && s.Reason.Id == reason.Id && s.ReasonScore == reasonScore)) {
          return true;
        }
      }
      return false;
    }
    #endregion
  }
}
