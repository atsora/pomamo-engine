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
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.DefaultReasonMinimalConfig
{
  /// <summary>
  /// Default implementation of IReasonExtension that is using default settings
  /// </summary>
  public sealed class DefaultReasonMinimalConfig
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , IReasonExtension
    , IReasonSelectionExtension
    , IReasonLegendExtension
  {
    /// <summary>
    /// Default reason selection score
    /// </summary>
    static readonly string REASON_SELECTION_SCORE_KEY = "Reason.SelectionScore.DefaultReasonMinimalConfig";
    static readonly double REASON_SELECTION_SCORE_DEFAULT = 100.0;

    static readonly string CURRENT_MACHINE_MODE_CLOSE_FUTURE_KEY = "DefaultReasonMinimalConfig.CurrentMachineMode.CloseFuture";
    static readonly TimeSpan CURRENT_MACHINE_MODE_CLOSE_FUTURE_DEFAULT = TimeSpan.FromSeconds (4);

    static readonly string LOG_ACTIVE_KEY = "DefaultReasonMinimalConfig.LogActive";
    static readonly bool LOG_ACTIVE_DEFAULT = false;

    static readonly string INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY = "DefaultReasonMinimalConfig.InterruptExtendIfRightReason";
    static readonly bool INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT = true;

    IMachine m_machine;

    bool m_batch = false;

    UtcDateTimeRange m_cacheRange = new UtcDateTimeRange ();
    UpperBound<DateTime> m_cacheConsolidationLimit = new UpperBound<DateTime> ();
    bool? m_cacheIsShort = null;

    bool m_logActive;
    IMachineMode m_activeMachineMode;
    IMachineMode m_unknownMachineMode;
    IMachineMode m_inactiveMachineMode;

    Configuration m_configuration;
    IReason m_activeReason;
    IReason m_inactiveShortReason;
    TimeSpan? m_maximumShortDuration;
    IReason m_inactiveLongReason;
    IReason m_unknownReason;
    double m_score;
    bool m_overwriteRequired;
    bool m_unknownIsInactive;

    ILog log = LogManager.GetLogger (typeof (DefaultReasonMinimalConfig).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultReasonMinimalConfig ()
    {
      m_logActive = Lemoine.Info.ConfigSet.LoadAndGet (LOG_ACTIVE_KEY, LOG_ACTIVE_DEFAULT);
    }

    void ResetCache ()
    {
      m_cacheRange = new UtcDateTimeRange ();
      m_cacheConsolidationLimit = new UpperBound<DateTime> ();
      m_cacheIsShort = null;
    }

    bool IsCacheApplicable (UtcDateTimeRange range)
    {
      return m_batch && m_cacheIsShort.HasValue
        && m_cacheRange.ContainsRange (range);
    }

    #region IReasonExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      var configurations = LoadConfigurations ();
      return Initialize (machine, configurations);
    }

    /// <summary>
    /// Initialize the plugin with a specific configuration
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="configurations"></param>
    /// <returns></returns>
    public bool Initialize (IMachine machine, IEnumerable<Configuration> configurations)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (typeof (DefaultReasonMinimalConfig).FullName + "." + machine.Id);

      if (!configurations.Any ()) {
        log.Error ("Initialize: LoadConfiguration returned no configuration");
        return false;
      }
      if (configurations.Any (x => x.TurnOff)) {
        log.Info ($"Initialize: one configuration is set with the TurnOff parameter => omit the plugin");
        return false;
      }
      m_configuration = configurations
        .OrderByDescending (x => x.ReasonScore)
        .First ();

      if (0 == m_configuration.ActiveReasonId) {
        log.Error ($"Initialize: no active reason set (id=0) => return false");
        return false;
      }
      if (0 == m_configuration.InactiveShortReasonId && m_configuration.MaximumShortDuration.HasValue) {
        log.Error ($"Initialize: no short reason set (id=0) although the maximum short duration {m_configuration.MaximumShortDuration} is set => return false");
        return false;
      }
      if (0 == m_configuration.InactiveLongReasonId && string.IsNullOrEmpty (m_configuration.InactiveLongDefaultReasonTranslationKey)) {
        log.Error ($"Initialize: no inactive reason set (id=0 and no translation key) => return false");
        return false;
      }
      if (0 == m_configuration.UnknownReasonId) {
        log.Error ($"Initialize: no unknown reason set (id=0) => return false");
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_activeReason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (m_configuration.ActiveReasonId);
        if (m_activeReason is null) {
          log.Error ($"Initialize: no active reason with id={m_configuration.ActiveReasonId} => return false");
          return false;
        }
        if (m_configuration.MaximumShortDuration.HasValue) {
          m_inactiveShortReason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (m_configuration.InactiveShortReasonId);
          if (m_inactiveShortReason is null) {
            log.Error ($"Initialize: no short reason with id={m_configuration.InactiveShortReasonId} => return false");
            return false;
          }
        }
        if (0 == m_configuration.InactiveLongReasonId) {
          m_inactiveLongReason = Lemoine.Extensions.AutoReason.ConfigRequests
            .AddReason (m_configuration.InactiveLongDefaultReasonTranslationKey, m_configuration.InactiveLongDefaultReasonTranslationValue);
        }
        else {
          m_inactiveLongReason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (m_configuration.InactiveLongReasonId);
          if (m_inactiveShortReason is null) {
            log.Error ($"Initialize: no long reason with id={m_configuration.InactiveLongReasonId} => return false");
            return false;
          }
        }
        m_unknownReason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (m_configuration.UnknownReasonId);
        if (m_unknownReason is null) {
          log.Error ($"Initialize: no unknown reason with id={m_configuration.UnknownReasonId} => return false");
          return false;
        }
        m_activeMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById ((int)MachineModeId.Active);
        if (m_activeMachineMode is null) {
          log.Error ($"Initialize: no active machine mode with id={MachineModeId.Active} => return false");
          return false;
        }
        m_unknownMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById ((int)MachineModeId.Unknown);
        if (m_unknownMachineMode is null) {
          log.Error ($"Initialize: no unknown machine mode with id={MachineModeId.Unknown} => return false");
          return false;
        }
        m_inactiveMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
          .FindById ((int)MachineModeId.Inactive);
        if (m_inactiveMachineMode is null) {
          log.Error ($"Initialize: no inactive machine mode with id={MachineModeId.Inactive} => return false");
          return false;
        }
      }
      m_maximumShortDuration = m_configuration.MaximumShortDuration;
      m_score = m_configuration.ReasonScore;
      m_overwriteRequired = m_configuration.OverwriteRequired;
      m_unknownIsInactive = m_configuration.UnknownIsInactive;

      return true;
    }

    bool? IsRunning (IReasonSlot reasonSlot)
    {
      return IsRunning (reasonSlot.MachineMode);
    }

    bool? IsRunning (IMachineMode machineMode)
    {
      if (machineMode.Running.HasValue) {
        return machineMode.Running.Value;
      }
      else if (m_unknownIsInactive) {
        return false;
      }
      else {
        return null;
      }
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
      // Not necessary here
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
    /// <param name="newReasonSlot">not null</param>
    public double? GetMaximumScore (IReasonSlot newReasonSlot)
    {
      Debug.Assert (null != newReasonSlot);

      return m_score;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      return IsRunning (newReasonSlot) ?? true; // Auto if Active or Unknown
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="oldReasonSlot">nullable</param>
    /// <param name="newReasonSlot">not null</param>
    /// <param name="modification"></param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      if ((null != modification) && (modification is IReasonMachineAssociation)) {
        return RequiredResetKind.None; // Everything is managed in MergeDataWithOldSlot
      }

      Debug.Assert (null != newReasonSlot.Reason);
      Debug.Assert ((int)ReasonId.Processing != newReasonSlot.Reason.Id);

      var reasonSlot = oldReasonSlot ?? newReasonSlot;

      if (reasonSlot.ReasonScore < m_score) {
        return RequiredResetKind.Full;
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode)) {
        if ((oldReasonSlot is null) || !object.Equals (IsRunning (oldReasonSlot), IsRunning (newReasonSlot))) {
          if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
            return RequiredResetKind.Main;
          }
          else if (reasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
            // Exclude the case when you switch from Unknown to Active
            if (oldReasonSlot is null || IsRunning (oldReasonSlot).HasValue || !(IsRunning (newReasonSlot) ?? false)) {
              return RequiredResetKind.ExtraAuto;
            }
          }
        }
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
        if (oldReasonSlot is null) {
          return RequiredResetKind.Full;
        }
        else { // At least for Fast process
          if (newReasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
            if (m_maximumShortDuration.HasValue && !(IsRunning (newReasonSlot) ?? true)
              && (m_inactiveLongReason.Id != newReasonSlot.Reason.Id)) { // Inactive short to long ?
              return RequiredResetKind.Main; // May be managed by the Period change also
            }
          }
        }
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.Period)) {
        if (!m_maximumShortDuration.HasValue) {
          return RequiredResetKind.None;
        }
        else if (IsRunning (newReasonSlot) ?? true) { // Running or unknown
          return RequiredResetKind.None;
        }
        else if (!newReasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
          return RequiredResetKind.None;
        }
        else if (null != oldReasonSlot) { // Inactive => check duration for short stop
          if (newReasonSlot.DateTimeRange.ContainsRange (oldReasonSlot.DateTimeRange)) { // New range is larger
            if (m_inactiveLongReason.Id == newReasonSlot.Reason.Id) {
              return RequiredResetKind.None;
            }
            var oldDuration = oldReasonSlot.DateTimeRange.Duration.Value;
            var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
            if ((oldDuration < m_maximumShortDuration.Value)
              && (m_maximumShortDuration.Value <= newDuration)) {
              return RequiredResetKind.Main;
            }
            else {
              return RequiredResetKind.None;
            }
          }
          else if (oldReasonSlot.DateTimeRange.ContainsRange (newReasonSlot.DateTimeRange)) { // New range is shorter
            if (m_inactiveShortReason.Id == newReasonSlot.Reason.Id) {
              return RequiredResetKind.None;
            }
            var oldDuration = oldReasonSlot.DateTimeRange.Duration.Value;
            var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
            if ((m_maximumShortDuration.Value <= oldDuration)
               && (newDuration < m_maximumShortDuration.Value)) {
              return RequiredResetKind.Main;
            }
            else {
              return RequiredResetKind.None;
            }
          }
          else { // Distinct ranges: approximative
            return RequiredResetKind.Main;
          }
        }
        else { // Inactive and oldReasonSlot is null
          var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
          if (newDuration < m_maximumShortDuration.Value) { // Shorter
            if (m_inactiveShortReason.Id == newReasonSlot.Reason.Id) {
              return RequiredResetKind.None;
            }
            else {
              return RequiredResetKind.Main;
            }
          }
          else if (m_inactiveLongReason.Id != newReasonSlot.Reason.Id) { // Longer and other than inactiveLong
            return RequiredResetKind.Main;
          }
          else {
            return RequiredResetKind.None;
          }
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
      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="reasonSlot">reason slot (not null)</param>
    /// <returns></returns>
    public void TryResetReason (ref Lemoine.Model.IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (reasonSlot.Duration.HasValue);

      if (IsRunning (reasonSlot) ?? false) { // Active
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"TryResetReason: apply the reason {m_activeReason.Id} because active");
        }
        // Note: there is no need to check the neighbors because there is no limited duration
        var oldSlot = (ISlot)reasonSlot.Clone ();
        this.TryDefaultReason (reasonSlot, m_activeReason,
          m_score,
          false,
          true,
          new UpperBound<DateTime> ());
        reasonSlot.Consolidate (oldSlot, null);
        return;
      }
      else if (!IsRunning (reasonSlot).HasValue) { // Unknown
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"TryResetReason: apply the reason {m_unknownReason.Id} because unknown");
        }
        // Note: there is no need to check the neighbors because there is no limited duration
        var oldSlot = (ISlot)reasonSlot.Clone ();
        this.TryDefaultReason (reasonSlot, m_unknownReason,
          m_score,
          false,
          true,
          new UpperBound<DateTime> ());
        reasonSlot.Consolidate (oldSlot, null);
        return;
      }
      else { // Inactive
        if (!m_maximumShortDuration.HasValue) {
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"TryResetReason: apply the reason {m_inactiveLongReason.Id} because inactive and no short");
          }
          // Note: there is no need to check the neighbors because there is no limited duration
          var oldSlot = (ISlot)reasonSlot.Clone ();
          this.TryDefaultReason (reasonSlot, m_inactiveLongReason,
            m_score,
            m_overwriteRequired,
            false,
            m_cacheConsolidationLimit);
          reasonSlot.Consolidate (oldSlot, null);
          return;
        }
        else { // Short/long
          UtcDateTimeRange range = reasonSlot.DateTimeRange;

          if (IsCacheApplicable (reasonSlot.DateTimeRange) && m_cacheIsShort.HasValue) {
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"TryResetReason: fast process (batch) for range {range} isShort={m_cacheIsShort}");
            }
            var isShort = m_cacheIsShort.Value;
            var oldSlot = (ISlot)reasonSlot;
            var reason = isShort ? m_inactiveShortReason : m_inactiveLongReason;
            this.TryDefaultReason (reasonSlot, reason,
              m_score,
              isShort ? false : m_overwriteRequired,
              false,
              new UpperBound<DateTime> ());
            reasonSlot.Consolidate (oldSlot, null);
          }
          else { // Cache not applicable
            var isShort = TestShort (m_machine, range, out var extendedPeriodReasonSlots, out var extendedPeriodStart, out var extendedPeriodEnd);
            // Apply to this slot
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"TryResetReason: isShort={isShort}");
            }
            var consolidationLimit = new UpperBound<DateTime> ();
            if (isShort) {
              consolidationLimit = extendedPeriodStart.Add (m_maximumShortDuration.Value);
            }
            var oldSlot = (ISlot)reasonSlot;
            var reason = isShort ? m_inactiveShortReason : m_inactiveLongReason;
            this.TryDefaultReason (reasonSlot, reason,
              m_score,
              isShort ? false : m_overwriteRequired,
              false,
              consolidationLimit);
            reasonSlot.Consolidate (oldSlot, null);

            // TODO: postpone the update on the neighbor slots ?
            var reasonSlotId = reasonSlot.Id;
            var applicableNeighbors = extendedPeriodReasonSlots
              .Where (s => !s.Id.Equals (reasonSlotId))
              .Where (s => s.Reason.Id != (int)ReasonId.Processing);
            foreach (var neighbor in applicableNeighbors) {
              this.TryDefaultReason (neighbor, reason,
              m_score,
              isShort ? false : m_overwriteRequired,
              false,
              consolidationLimit);
            }
            if (m_batch) {
              m_cacheRange = new UtcDateTimeRange (extendedPeriodStart, extendedPeriodEnd);
              m_cacheConsolidationLimit = consolidationLimit;
              m_cacheIsShort = isShort;
            }
          }
        }
      }
    }
    #endregion

    bool TestShort (IMachine machine, UtcDateTimeRange range, out IList<IReasonSlot> extendedPeriodReasonSlots, out DateTime extendedPeriodStart, out DateTime extendedPeriodEnd)
    {
      Debug.Assert (range.Duration.HasValue);
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      var effectiveRange = range;
      extendedPeriodStart = range.Lower.Value;
      extendedPeriodEnd = range.Upper.Value;
      extendedPeriodReasonSlots = new List<IReasonSlot> ();

      if (!m_maximumShortDuration.HasValue) {
        return false;
      }

      // Test the duration
      if (m_maximumShortDuration.Value < effectiveRange.Duration.Value) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"TestShort: duration {effectiveRange.Duration.Value} is greater than maximum duration {m_maximumShortDuration} => return long");
        }
        return false;
      }

      // Test again the duration with an extended period of time
      // if needed
      bool noDurationInterruption = ExtendToLeft (machine, ref effectiveRange, extendedPeriodReasonSlots);
      if (noDurationInterruption) {
        noDurationInterruption = ExtendToRight (machine, ref effectiveRange, extendedPeriodReasonSlots);
      }

      if (extendedPeriodReasonSlots.Any ()) {
        var first = extendedPeriodReasonSlots.First ();
        Debug.Assert (first.DateTimeRange.Lower.HasValue);
        if (first.DateTimeRange.Lower.Value < extendedPeriodStart) {
          extendedPeriodStart = first.DateTimeRange.Lower.Value;
        }
        var last = extendedPeriodReasonSlots.Last ();
        Debug.Assert (last.DateTimeRange.Upper.HasValue);
        if (extendedPeriodEnd < last.DateTimeRange.Upper.Value) {
          extendedPeriodEnd = last.DateTimeRange.Upper.Value;
        }
      }

      if (!noDurationInterruption) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ("TestShort: one of the Extend method returned false");
        }
        return false;
      }

      Debug.Assert (effectiveRange.Duration.HasValue);
      TimeSpan extendedPeriodDuration = effectiveRange.Duration.Value;
      if (m_maximumShortDuration.Value < extendedPeriodDuration) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"TestShort: extended duration {extendedPeriodDuration} is greater than maximum duration {m_maximumShortDuration} => return false");
        }
        return false;
      }

      if (log.IsDebugEnabled && m_logActive) {
        log.Debug ($"TestShort: extended duration {extendedPeriodDuration} is less than maximum duration {m_maximumShortDuration} => return true");
      }
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reasonSlots"></param>
    /// <param name="maxRecursionNumber"></param>
    /// <returns>false if interrupted because range duration is longer than the max duration</returns>
    bool ExtendToLeft (IMachine machine, ref UtcDateTimeRange range, IList<IReasonSlot> reasonSlots, int maxRecursionNumber = 1500)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Duration.HasValue);

      if (m_maximumShortDuration.HasValue
        && (m_maximumShortDuration.Value < range.Duration.Value)) {
        return false;
      }

      if (0 == maxRecursionNumber) {
        log.Error ($"ExtendToLeft: max recursion number was reached => give up with range={range}");
        return true;
      }

      var reasonSlotBefore = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindWithEnd (machine,
                      range.Lower.Value);
      if (null != reasonSlotBefore) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"ExtendToleft: check if {range} can be extended to {reasonSlotBefore.DateTimeRange.Lower}");
        }
        if (!(IsRunning (reasonSlotBefore) ?? true)) { // Inactive
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"ExtendToLeft: extend {range} to {reasonSlotBefore.DateTimeRange.Lower}");
          }
          var newRange = new UtcDateTimeRange (range.Union (reasonSlotBefore.DateTimeRange));
          if (!newRange.ContainsRange (range) || newRange.Equals (range)) {
            log.Fatal ($"ExtendToLeft: new range {newRange} is not larger than old {range}");
            throw new InvalidOperationException ("Unexpected range in recursion");
          }
          range = newRange;
          reasonSlots.Insert (0, reasonSlotBefore);
          if (((reasonSlotBefore.Reason?.Id ?? 0) == m_inactiveLongReason.Id)
            && Lemoine.Info.ConfigSet.LoadAndGet (INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY, INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT)) {
            // The right reason is already set, there is no reason to continue
            log.Info ($"ExtendToLeft: stop extending the periods since the new slot at {reasonSlotBefore.DateTimeRange} has already the right reason => isLong");
            return false;
          }
          // Try again
          return ExtendToLeft (machine, ref range, reasonSlots, maxRecursionNumber - 1);
        }
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reasonSlots"></param>
    /// <param name="maxRecursionNumber"></param>
    /// <returns>false if interrupted because range duration is longer than the max duration</returns>
    bool ExtendToRight (IMachine machine, ref UtcDateTimeRange range, IList<IReasonSlot> reasonSlots, int maxRecursionNumber = 1500)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Duration.HasValue);

      if (m_maximumShortDuration.HasValue
        && (m_maximumShortDuration.Value < range.Duration.Value)) {
        return false;
      }

      if (0 == maxRecursionNumber) {
        log.Error ($"ExtendToRight: max recursion number was reached => give up with range={range}");
        return true;
      }

      // The range may be with upper bound inclusive (unique date/time)
      var dateTimeAfter = range.UpperInclusive ? range.Upper.Value.AddSeconds (1) : range.Upper.Value;
      var reasonSlotAfter = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAt (machine,
                 dateTimeAfter);
      if (null != reasonSlotAfter) {
        if (!reasonSlotAfter.DateTimeRange.Lower.HasValue) {
          log.Fatal ($"ExtendToRight: no lower value in reason slot after id={reasonSlotAfter.Id} range={reasonSlotAfter.DateTimeRange}");
          throw new InvalidOperationException ("Invalid reason slot, with no lower date/time");
        }
        else if (reasonSlotAfter.DateTimeRange.IsEmpty ()) {
          log.Fatal ($"ExtendToRight: reason slot after id={reasonSlotAfter.Id} with an empty range={reasonSlotAfter.DateTimeRange}");
          throw new InvalidOperationException ("Invalid reason slot, with an empty range");
        }
        else if (Bound.Compare<DateTime> (reasonSlotAfter.DateTimeRange.Upper, dateTimeAfter) <= 0) {
          log.Fatal ($"ExtendToRight: reason slot after id={reasonSlotAfter.Id} range={reasonSlotAfter.DateTimeRange} is not after reason slot range={range}");
          throw new Exception ("Invalid reason slot after");
        }
        else { // Valid next reasonSlotAfter
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"ExtendToRight: check if {range} can be extended to {reasonSlotAfter.DateTimeRange.Upper}");
          }
          if (!(IsRunning (reasonSlotAfter) ?? true)) { // Inactive
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"ExtendToRight: extend {range} to {reasonSlotAfter.DateTimeRange.Upper}");
            }
            var newRange = new UtcDateTimeRange (range.Union (reasonSlotAfter.DateTimeRange));
            if (!newRange.ContainsRange (range) || newRange.Equals (range)) {
              log.Fatal ($"ExtendToRight: new range {newRange} is not larger than old {range}. Slot after={reasonSlotAfter.Id} {reasonSlotAfter.DateTimeRange}. StackTrace={System.Environment.StackTrace}");
              throw new InvalidOperationException ("Unexpected range in recursion");
            }
            range = newRange;
            reasonSlots.Add (reasonSlotAfter);
            if (((reasonSlotAfter.Reason?.Id ?? 0) == m_inactiveLongReason.Id)
              && Lemoine.Info.ConfigSet.LoadAndGet (INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY, INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT)) {
              // The right reason is already set, there is no reason to continue
              log.Info ($"ExtendToRight: stop extending the periods since the new slot at {reasonSlotAfter.DateTimeRange} has already the right reason => isLong");
              return false;
            }
            // Try again
            return ExtendToRight (machine, ref range, reasonSlots, maxRecursionNumber - 1);
          }
        }
      }

      return true;
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
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      var running = IsRunning (machineMode);
      if (running.HasValue) {
        if (running.Value) { // Active
          return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (m_activeReason, null,
            m_score, ReasonSource.DefaultAuto, false,
            m_activeMachineMode,
            null) };
        }
        else { // Inactive
          if (autoManualOnly) {
            log.Info ($"TryGetActiveAt: inactive and autoManualOnly at {at} => return an empty list");
            return new List<IPossibleReason> ();
          }
          else if (!m_maximumShortDuration.HasValue) {
            return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (m_inactiveLongReason, null,
              m_score, ReasonSource.Default, m_overwriteRequired,
              m_inactiveMachineMode,
              null) };
          }
          else {
            UtcDateTimeRange range = new UtcDateTimeRange ();
            {
              IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
                .FindAt (m_machine, at);
              if (null != reasonSlot) {
                Debug.Assert (reasonSlot.MachineMode.Id == machineMode.Id);
                Debug.Assert (reasonSlot.MachineObservationState.Id == machineObservationState.Id);
                range = reasonSlot.DateTimeRange;
              }
              else {
                if (!GetRangeFromCurrentMachineMode (at, machineMode, ref range)) {
                  if (!GetRangeFromFact (at, machineMode, ref range)) {
                    if (log.IsWarnEnabled) {
                      log.Warn ("TryGetActiveAt: no range could be determined from machine mode");
                    }
                  }
                }
              }
            }
            if (range.IsEmpty ()) {
              if (log.IsWarnEnabled) {
                log.Warn ($"TryGetActiveAt: no range could be determined at {at} => use a default range of duration 1s");
              }
              range = new UtcDateTimeRange (at.Subtract (TimeSpan.FromSeconds (1)), at);
            }
            Debug.Assert (!range.IsEmpty ());
            var isShort = TestShort (m_machine, range, out var extendedPeriodReasonSlots, out var extendedPeriodStart, out var extendedPeriodEnd);
            var reason = isShort ? m_inactiveShortReason : m_inactiveLongReason;
            var overwriteRequired = isShort ? false : m_overwriteRequired;
            return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (reason, null,
              m_score, ReasonSource.Default, overwriteRequired,
              m_inactiveMachineMode,
              null) };
          }
        }
      }
      else { // Unknown
        return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (m_unknownReason, null,
            m_score, ReasonSource.DefaultAuto, false,
            m_unknownMachineMode,
            null) };
      }
    }

    bool GetRangeFromCurrentMachineMode (DateTime at, IMachineMode machineMode, ref UtcDateTimeRange range)
    {
      Debug.Assert (null != m_machine);

      var currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
        .Find (m_machine);
      if (null == currentMachineMode) {
        log.ErrorFormat ("GetRangeFromCurrentMachineMode: no current machine mode for machine {0}",
          m_machine.Id);
        return false;
      }

      var currentMachineModeRange = new UtcDateTimeRange (currentMachineMode.Change, currentMachineMode.DateTime);
      if (currentMachineModeRange.ContainsElement (at)) {
        if (currentMachineMode.MachineMode.Id != machineMode.Id) {
          log.ErrorFormat ("GetRangeFromCurrentMachineMode: current machine mode {0} differs from {1} at {2}",
            currentMachineMode.MachineMode.Id, machineMode.Id, at);
          return false;
        }
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetRangeFromCurrentMachineMode: from current machine mode {0}", currentMachineModeRange);
        }
        range = currentMachineModeRange;
        return true;
      }

      // Close future
      var closeFuture = Lemoine.Info.ConfigSet.LoadAndGet (CURRENT_MACHINE_MODE_CLOSE_FUTURE_KEY, CURRENT_MACHINE_MODE_CLOSE_FUTURE_DEFAULT);
      var closeFutureRange = new UtcDateTimeRange (currentMachineMode.Change, currentMachineMode.DateTime.Add (closeFuture));
      if (closeFutureRange.ContainsElement (at)) {
        if (currentMachineMode.MachineMode.Id != machineMode.Id) {
          log.ErrorFormat ("GetRangeFromCurrentMachineMode: current machine mode {0} in close future differs from {1} at {2}",
            currentMachineMode.MachineMode.Id, machineMode.Id, at);
          return false;
        }
        if (log.IsInfoEnabled) {
          log.InfoFormat ("GetRangeFromCurrentMachineMode: from current machine mode in close future {0} at {1}", closeFutureRange, at);
        }
        range = new UtcDateTimeRange (currentMachineMode.Change, at, "[]");
        return true;
      }

      return false;
    }

    bool GetRangeFromFact (DateTime at, IMachineMode machineMode, ref UtcDateTimeRange range)
    {
      IFact fact = ModelDAOHelper.DAOFactory.FactDAO
        .FindAt (m_machine, at);
      if (null == fact) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetRangeFromFact: no fact at {0}", at);
        }
        return GetRangeFromLastFact (at, machineMode, ref range);
      }
      else if (fact.CncMachineMode.Id == machineMode.Id) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetRangeFromFact: get range {0} from fact {1} {2}-{3}", fact.DateTimeRange, fact.Id, fact.Begin, fact.End);
        }
        range = fact.DateTimeRange;
        return true;
      }
      else {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("GetRangeFromFact: machine mode {0} of fact {1}-{2} is different from machine mode {3} at {4}",
            fact.CncMachineMode.Id, fact.Begin, fact.End, machineMode.Id, at);
        }
        return false;
      }
    }

    bool GetRangeFromLastFact (DateTime at, IMachineMode machineMode, ref UtcDateTimeRange range)
    {
      var fact = ModelDAOHelper.DAOFactory.FactDAO
        .GetLast (m_machine);
      if (null == fact) {
        log.ErrorFormat ("GetRangeFromLastFact: machine has no fact (GetLast returned null)");
        return false;
      }
      else if (Bound.Compare<DateTime> (at, fact.Begin) < 0) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("GetRangeFromLastFact: last fact starts after {0}", at);
        }
        return false;
      }
      else if (Bound.Compare<DateTime> (at, fact.End) < 0) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("GetRangeFromLastFact: last fact should have already been returned by the FindAt method");
        }
      }
      Debug.Assert (null != fact);
      if (fact.CncMachineMode.Id == machineMode.Id) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("GetRangeFromLastFact: get range from fact {0}-{1} at {2}", fact.Begin, fact.End, at);
        }
        range = new UtcDateTimeRange (at, fact.End);
        return true;
      }
      else {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("GetRangeFromLastFact: machine mode {0} of fact {1}-{2} is different from machine mode {3} at {4}",
            fact.CncMachineMode.Id, fact.Begin, fact.End, machineMode.Id, at);
        }
        return false;
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
      if (!reasonSource.IsDefault ()) {
        return false;
      }
      else if (reasonScore != m_score) {
        return false;
      }
      else {
        var running = IsRunning (machineMode);
        if (running.HasValue) {
          if (running.Value) { // Active
            return reason.Id == m_activeReason.Id;
          }
          else { // Inactive
            if (reason.Id == m_inactiveLongReason.Id) {
              return true;
            }
            else if (m_inactiveShortReason is null) {
              return false;
            }
            else {
              return reason.Id == m_inactiveShortReason.Id;
            }
          }
        }
        else { // Unknown
          return reason.Id == m_unknownReason.Id;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="includeExtraAutoReasons"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSelection> GetReasonSelections (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
    {
      Debug.Assert (null != m_machine);
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      if (!includeExtraAutoReasons) {
        return new List<IReasonSelection> ();
      }

      var running = IsRunning (machineMode);
      if (running ?? true) { // Active or unknown => Auto
        double reasonScore = Lemoine.Info.ConfigSet.LoadAndGet<double> (REASON_SELECTION_SCORE_KEY,
          REASON_SELECTION_SCORE_DEFAULT);
        var reason = running.HasValue ? m_activeReason : m_unknownReason;
        var ancestorMachineMode = running.HasValue ? m_activeMachineMode : m_unknownMachineMode;
        return new List<IReasonSelection> {
          new Pulse.Extensions.Database.ExtraReasonSelection (ancestorMachineMode,
          machineObservationState,
          reason,
          reasonScore)
        };
      }
      else { // Inactive
        return new List<IReasonSelection> ();
      }
    }

    #region IReasonLegendExtension implementation
    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReason> GetUsedReasons ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var reasons = new List<IReason> {
          m_activeReason,
          m_inactiveLongReason
        };
        if (!m_unknownIsInactive) {
          reasons.Add (m_unknownReason);
        }
        if (null != m_inactiveShortReason) {
          reasons.Add (m_inactiveShortReason);
        }
        // Note: normally not required because of the early fetch of the reason and reason group in FindWithReasonGroup
        foreach (var reason in reasons) {
          if (reason is null) {
            log.Error ($"GetUsedReasons: a reason is null");
          }
          else {
            ModelDAOHelper.DAOFactory.Initialize (reason.ReasonGroup);
          }
        }
        return reasons;
      }
    }
    #endregion // IReasonLegendExtension implementation
  }
}
