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
using System.Runtime.CompilerServices;

namespace Lemoine.Plugin.DefaultReasonWithDurationConfig
{
  /// <summary>
  /// Default implementation of IReasonExtension that is using the machinemodedefaultreason table and the duration of the period
  /// </summary>
  public sealed class DefaultReasonWithDuration
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , IReasonExtension
    , IReasonSelectionExtension
    , IReasonLegendExtension
  {
    /// <summary>
    /// Default reason selection score
    /// </summary>
    static readonly string REASON_SELECTION_SCORE_KEY = "Reason.SelectionScore.MachineModeDefaultReason";
    static readonly double REASON_SELECTION_SCORE_DEFAULT = 100.0;

    static readonly string CURRENT_MACHINE_MODE_CLOSE_FUTURE_KEY = "DefaultReasonWithDuration.CurrentMachineMode.CloseFuture";
    static readonly TimeSpan CURRENT_MACHINE_MODE_CLOSE_FUTURE_DEFAULT = TimeSpan.FromSeconds (4);

    static readonly string LOG_ACTIVE_KEY = "Plugin.ReasonDefaultManagement.LogActive";
    static readonly bool LOG_ACTIVE_DEFAULT = false;

    static readonly string INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY = "DefaultReasonWithDuration.InterruptExtendIfRightReason";
    static readonly bool INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT = true;

    IMachine m_machine;
    bool m_batch = false;
    UtcDateTimeRange m_cacheRange = new UtcDateTimeRange ();
    UpperBound<DateTime> m_cacheConsolidationLimit = new UpperBound<DateTime> ();
    IMachineModeDefaultReason m_cacheDefaultReason = null;
    bool m_logActive;

    double m_maxScore = -1.0;
    double m_minScore = -1.0;

    ILog log = LogManager.GetLogger (typeof (DefaultReasonWithDuration).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultReasonWithDuration ()
    {
      m_logActive = Lemoine.Info.ConfigSet.LoadAndGet (LOG_ACTIVE_KEY, LOG_ACTIVE_DEFAULT);
    }

    void ResetCache ()
    {
      m_cacheRange = new UtcDateTimeRange ();
      m_cacheConsolidationLimit = new UpperBound<DateTime> ();
      m_cacheDefaultReason = null;
    }

    bool IsCacheApplicable (UtcDateTimeRange range)
    {
      return m_batch && (null != m_cacheDefaultReason)
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
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (typeof (DefaultReasonWithDuration).FullName + "." + machine.Id);

      var configurations = LoadConfigurations ();
      if (!configurations.Any ()) {
        log.Warn ("Initialize: LoadConfiguration returned no configuration but continue here");
      }
      if (configurations.Any (x => x.TurnOff)) {
        log.Info ($"Initialize: one configuration is set with the TurnOff parameter => omit the plugin");
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var defaultReasons = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
          .FindWith (machine);
        if (defaultReasons.Any ()) {
          m_maxScore = defaultReasons.Max (x => x.Score);
          m_minScore = defaultReasons.Min (x => x.Score);
        }
        else {
          log.Error ($"Initialize: no config for this machine");
          m_maxScore = -1.0;
          m_minScore = -1.0;
          return false;
        }
      }

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

      if (IsCacheApplicable (newReasonSlot.DateTimeRange)) {
        return m_cacheDefaultReason.Score;
      }

      var defaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
                                                         newReasonSlot.MachineObservationState,
                                                         newReasonSlot.MachineMode);
      if (!defaultReasons.Any ()) {
        return -1.0; // None
      }
      else {
        return defaultReasons.Max (d => d.Score);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      if (IsCacheApplicable (newReasonSlot.DateTimeRange)) {
        return m_cacheDefaultReason.Auto;
      }

      var defaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
                                                         newReasonSlot.MachineObservationState,
                                                         newReasonSlot.MachineMode);
      return defaultReasons.Any (d => d.Auto);
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

      IEnumerable<IMachineModeDefaultReason> machineModeDefaultReasons = null;

      if (reasonSlot.ReasonScore < m_minScore) {
        if (machineModeDefaultReasons is null) {
          machineModeDefaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
            newReasonSlot.MachineObservationState, newReasonSlot.MachineMode);
        }
        if (machineModeDefaultReasons.Any ()) {
          return RequiredResetKind.Full;
        }
      }

      if (m_maxScore < reasonSlot.ReasonScore) {
        if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
          // The configuration may have changed or the default reason comes from another plugin
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"GetRequiredResetKind: reason slot with a default reason and a score higher than what is the configuration => Full");
          }
          return RequiredResetKind.Full;
        }
        else { // Not a reason slot with a default reason
          if (machineModeDefaultReasons is null) {
            machineModeDefaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
              newReasonSlot.MachineObservationState, newReasonSlot.MachineMode);
          }
          if (reasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
            if (machineModeDefaultReasons.All (x => x.Auto)) {
              return RequiredResetKind.None;
            }
            else if (machineModeDefaultReasons.Any ()) {
              return RequiredResetKind.ExtraAuto;
            }
            else {
              return RequiredResetKind.None;
            }
          }
          else {
            if (machineModeDefaultReasons.Any (x => x.Auto)) {
              return RequiredResetKind.ExtraAuto;
            }
            else {
              return RequiredResetKind.None;
            }
          }
        }
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode)
        || reasonSlotChange.HasFlag (ReasonSlotChange.MachineObservationState)) {
        // Because of the score comparison, it looks pretty complex to return a better value
        return RequiredResetKind.Full;
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
        if (oldReasonSlot is null) {
          return RequiredResetKind.Full;
        }
        else { // At least for Fast process
          if (newReasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
            if (machineModeDefaultReasons is null) {
              machineModeDefaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
                newReasonSlot.MachineObservationState, newReasonSlot.MachineMode);
            }
            if (!machineModeDefaultReasons.Any ()) {
              if (log.IsWarnEnabled && m_logActive) {
                log.Warn ($"GetRequiredResetKind: new activity, no machine mode default reason");
              }
              // The default reason must come from somewhere else
              return RequiredResetKind.None;
            }
            else if (machineModeDefaultReasons.Any (x => x.MaximumDuration.HasValue)) {
              var defaultReasonWithNoMaximumDuration = machineModeDefaultReasons
                .FirstOrDefault (x => !x.MaximumDuration.HasValue);
              if (defaultReasonWithNoMaximumDuration is null) {
                log.Error ($"GetRequiredResetKind: no default reason with no maximum duration");
                return RequiredResetKind.Main;
              }
              else if ((defaultReasonWithNoMaximumDuration.Reason.Id == newReasonSlot.Reason.Id) && (defaultReasonWithNoMaximumDuration.Score == newReasonSlot.ReasonScore)) {
                if (log.IsDebugEnabled && m_logActive) {
                  log.Debug ($"GetRequiredResetKind: new activity but already the default reason with no maximum duration");
                }
                return RequiredResetKind.None;
              }
              else if (machineModeDefaultReasons.All (x => x.Score < reasonSlot.ReasonScore)) {
                // The default reason must come from another plugin
                if (log.IsWarnEnabled && m_logActive) {
                  log.Warn ($"GetRequiredResetKind: new activity and all default reasons have a score lesser than {reasonSlot.ReasonScore}");
                }
                return RequiredResetKind.None;
              }
              else {
                return RequiredResetKind.Main;
              }
            }
            else { // Unique..
              if (1 != machineModeDefaultReasons.Count ()) {
                log.Error ($"GetRequiredResetKind: more than one machine mode default reason with no maximum duration");
                return RequiredResetKind.Main;
              }
              else {
                var uniqueDefaultReason = machineModeDefaultReasons.Single ();
                if ((uniqueDefaultReason.Reason.Id == reasonSlot.Reason.Id) && (uniqueDefaultReason.Score == reasonSlot.ReasonScore)) {
                  return RequiredResetKind.None;
                }
                else if (reasonSlot.ReasonScore <= uniqueDefaultReason.Score) {
                  return RequiredResetKind.Main;
                }
                else {
                  // The default reason must come from another plugin
                  if (log.IsWarnEnabled && m_logActive) {
                    log.Warn ($"GetRequiredResetKind: new activity and the unique default reason have a score lesser than {reasonSlot.ReasonScore}");
                  }
                  return RequiredResetKind.None;
                }
              }
            }
          }
        }
      }

      if (reasonSlotChange.HasFlag (ReasonSlotChange.Period)) {
        if (machineModeDefaultReasons is null) {
          machineModeDefaultReasons = GetMachineModeDefaultReasons (newReasonSlot.Machine,
            newReasonSlot.MachineObservationState, newReasonSlot.MachineMode);
        }
        if (!machineModeDefaultReasons.Any ()) { // The 'undefined' reason must be used
          if (log.IsWarnEnabled && m_logActive) {
            log.Warn ($"GetRequiredResetKind: only a period change and no default reason");
          }
          // The default reason must come from somewhere else or the configuration changed
          if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
            return RequiredResetKind.Full;
          }
          else if (reasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
            return RequiredResetKind.ExtraAuto;
          }
          else {
            return RequiredResetKind.None;
          }
        }
        if (machineModeDefaultReasons.All (m => !m.MaximumDuration.HasValue)) {
          return RequiredResetKind.None;
        }
        if (!reasonSlot.ReasonSource.HasFlag (ReasonSource.Default)) {
          if (reasonSlot.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
            if (machineModeDefaultReasons.All (x => x.Auto)) {
              return RequiredResetKind.None;
            }
            else {
              return RequiredResetKind.ExtraAuto;
            }
          }
          else {
            if (machineModeDefaultReasons.Any (x => x.Auto)) {
              return RequiredResetKind.ExtraAuto;
            }
            else {
              return RequiredResetKind.None;
            }
          }
        }
        Debug.Assert (reasonSlot.ReasonSource.HasFlag (ReasonSource.Default));
        if (null != oldReasonSlot) { // Test on machine mode default reasons
          if (newReasonSlot.DateTimeRange.ContainsRange (oldReasonSlot.DateTimeRange)) { // New range is larger
            var oldDuration = oldReasonSlot.DateTimeRange.Duration.Value;
            var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
            var withLargerDuration = machineModeDefaultReasons
              .Where (m => m.MaximumDuration.HasValue
                && (oldDuration < m.MaximumDuration.Value)
                && (m.MaximumDuration.Value <= newDuration));
            if (withLargerDuration.Any ()) {
              return RequiredResetKind.Main;
            }
            else {
              return RequiredResetKind.None;
            }
          }
          else if (oldReasonSlot.DateTimeRange.ContainsRange (newReasonSlot.DateTimeRange)) { // New range is shorter
            var oldDuration = oldReasonSlot.DateTimeRange.Duration.Value;
            var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
            var withShorterDuration = machineModeDefaultReasons
              .Where (m => m.MaximumDuration.HasValue)
              .Where (m => m.MaximumDuration.Value <= oldDuration)
              .Where (m => newDuration < m.MaximumDuration.Value);
            if (withShorterDuration.Any ()) {
              return RequiredResetKind.Main;
            }
            else {
              return RequiredResetKind.None;
            }
          }
          else { // Distinct ranges: approximative
            var withNoMaxDuration = machineModeDefaultReasons
              .FirstOrDefault (x => !x.MaximumDuration.HasValue);
            if (withNoMaxDuration is null) {
              log.Error ($"GetRequiredResetKind: no config with no maximum duration");
              return RequiredResetKind.Main;
            }
            var maxDuration = TimeSpan.FromSeconds (machineModeDefaultReasons
              .Where (x => x.MaximumDuration.HasValue)
              .Max (x => x.MaximumDuration.Value.TotalSeconds));
            var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
            if ((maxDuration <= newDuration) && (withNoMaxDuration.Reason.Id == reasonSlot.Reason.Id) && (withNoMaxDuration.Score == reasonSlot.ReasonScore)) {
              return RequiredResetKind.None;
            }
            else {
              return RequiredResetKind.Main;
            }
          }
        }
        else { // null == oldReasonSlot
          var matchedMachineModeDefaultReasons = machineModeDefaultReasons
            .Where (m => m.Reason.Id == newReasonSlot.Reason.Id);
          if (matchedMachineModeDefaultReasons.Any ()) {
            var count = matchedMachineModeDefaultReasons.Count ();
            if (1 < count) {
              log.WarnFormat ("GetRequiredResetKind: different default reasons with the same reason => fallback to Main");
              return RequiredResetKind.Main;
            }
            else if (1 == count) {
              var matchedMachineModeDefaultReason = matchedMachineModeDefaultReasons.First ();
              var newDuration = newReasonSlot.DateTimeRange.Duration.Value;
              if (!matchedMachineModeDefaultReason.MaximumDuration.HasValue) {
                var maxDuration = TimeSpan.FromSeconds (machineModeDefaultReasons
                  .Where (x => x.MaximumDuration.HasValue)
                  .Max (x => x.MaximumDuration.Value.TotalSeconds));
                if ((maxDuration <= newDuration)
                  && (matchedMachineModeDefaultReason.Score == reasonSlot.ReasonScore)) {
                  return RequiredResetKind.None;
                }
                else {
                  return RequiredResetKind.Main;
                }
              }
              else {
                return RequiredResetKind.Main;
              }
            }
            else {
              log.Fatal ($"GetRequiredResetKind: unexpected count {count}");
              Debug.Assert (false);
              throw new InvalidOperationException ();
            }
          }
          else { // Not any
            if (log.IsWarnEnabled && m_logActive) {
              log.Warn ("GetRequiredResetKind: no matching default reason in database");
            }
            return RequiredResetKind.Main;
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

      if (IsCacheApplicable (reasonSlot.DateTimeRange)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"TryResetReason: fast process (batch) for range {reasonSlot.DateTimeRange}");
        }
        var oldSlot = (ISlot)reasonSlot.Clone ();
        this.TryDefaultReason (reasonSlot,
          m_cacheDefaultReason.Reason,
          m_cacheDefaultReason.Score,
          m_cacheDefaultReason.OverwriteRequired,
          m_cacheDefaultReason.Auto,
          m_cacheConsolidationLimit);
        reasonSlot.Consolidate (oldSlot, null);
        return;
      }

      var machineModeDefaultReasons = GetMachineModeDefaultReasons (reasonSlot.Machine,
                                                                    reasonSlot.MachineObservationState,
                                                                    reasonSlot.MachineMode);
      Debug.Assert (null != machineModeDefaultReasons);
      Debug.Assert (machineModeDefaultReasons.Any ());
      if (machineModeDefaultReasons.Any (x => (int)ReasonId.Processing == x.Reason.Id)) {
        log.Error ($"TryResetReason: one of the default reason is processing, skip it");
        machineModeDefaultReasons = machineModeDefaultReasons.Where (x => (int)ReasonId.Processing != x.Reason.Id);
      }

      if (1 == machineModeDefaultReasons.Count ()) { // Unique machine mode default reason, there is no need to check the duration
        var uniqueMachineModeDefaultReason = machineModeDefaultReasons.First ();
        Debug.Assert (!uniqueMachineModeDefaultReason.MaximumDuration.HasValue);
        if (uniqueMachineModeDefaultReason.MaximumDuration.HasValue) {
          log.Error ($"TryResetReason: unique machine mode default reason {uniqueMachineModeDefaultReason} with a maximum duration");
          if (reasonSlot.DateTimeRange.Duration.HasValue
            && (uniqueMachineModeDefaultReason.MaximumDuration.Value < reasonSlot.DateTimeRange.Duration.Value)) {
            return;
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"TryResetReason: apply the unique machine mode default reason {uniqueMachineModeDefaultReason}");
        }
        // Note: there is no need to check the neighbors because there is no limited duration
        var oldSlot = (ISlot)reasonSlot.Clone ();
        this.TryDefaultReason (reasonSlot, uniqueMachineModeDefaultReason.Reason,
          uniqueMachineModeDefaultReason.Score,
          uniqueMachineModeDefaultReason.OverwriteRequired,
          uniqueMachineModeDefaultReason.Auto,
          new UpperBound<DateTime> ());
        reasonSlot.Consolidate (oldSlot, null);

        return;
      }

      Debug.Assert (1 < machineModeDefaultReasons.Count ());

      UtcDateTimeRange range = reasonSlot.DateTimeRange;

      IList<IReasonSlot> extendedPeriodReasonSlots;
      DateTime extendedPeriodStart;
      DateTime extendedPeriodEnd;
      IMachineModeDefaultReason machineModeDefaultReason = TestMachineModeDefaultReasons (m_machine,
        machineModeDefaultReasons, range, out extendedPeriodReasonSlots, out extendedPeriodStart, out extendedPeriodEnd);
      if (null != machineModeDefaultReason) {
        // Apply to this slot
        if (log.IsDebugEnabled) {
          log.Debug ($"UpdateDefaultReason: use default reason {machineModeDefaultReason.Reason.Id}");
        }
        var consolidationLimit = new UpperBound<DateTime> ();
        if (machineModeDefaultReason.MaximumDuration.HasValue) {
          consolidationLimit = extendedPeriodStart.Add (machineModeDefaultReason.MaximumDuration.Value);
        }
        var oldSlot = (ISlot)reasonSlot;
        this.TryDefaultReason (reasonSlot, machineModeDefaultReason.Reason,
          machineModeDefaultReason.Score,
          machineModeDefaultReason.OverwriteRequired,
          machineModeDefaultReason.Auto,
          consolidationLimit);
        reasonSlot.Consolidate (oldSlot, null);

        // Postpone the update on the neighbor slots
        var reasonSlotId = reasonSlot.Id;
        var applicableNeighbors = extendedPeriodReasonSlots
          .Where (s => !s.Id.Equals (reasonSlotId))
          .Where (s => s.Reason.Id != (int)ReasonId.Processing);
        foreach (var neighbor in applicableNeighbors) {
          neighbor.TryUpdateDefaultReason (machineModeDefaultReason, consolidationLimit);
        }
        if (m_batch) {
          m_cacheRange = new UtcDateTimeRange (extendedPeriodStart, extendedPeriodEnd);
          m_cacheConsolidationLimit = consolidationLimit;
          m_cacheDefaultReason = machineModeDefaultReason;
        }

        return;
      }
      else {
        log.Error ($"UpdateDefaultReason: no default reason was found for reasonslot id {reasonSlot.Id}");
        return;
      }
    }
    #endregion

    IMachineModeDefaultReason TestMachineModeDefaultReasons (IMachine machine, IEnumerable<IMachineModeDefaultReason> machineModeDefaultReasons, UtcDateTimeRange range, out IList<IReasonSlot> extendedPeriodReasonSlots, out DateTime extendedPeriodStart, out DateTime extendedPeriodEnd)
    {
      Debug.Assert (range.Duration.HasValue);
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      var effectiveRange = range;
      extendedPeriodStart = range.Lower.Value;
      extendedPeriodEnd = range.Upper.Value;
      extendedPeriodReasonSlots = new List<IReasonSlot> ();

      if (!machineModeDefaultReasons.Any ()) {
        log.Error ("TestMachineModeDefaultReasons: no default reason set => return null");
        return null;
      }

      var defaultReasonToTest = machineModeDefaultReasons.First ();
      Debug.Assert (null != defaultReasonToTest.MachineMode);
      Debug.Assert (null != defaultReasonToTest.MachineObservationState);
      Debug.Assert (null != defaultReasonToTest.Reason);

      // Note: it has already been tested the items of machineModeDefaultReasons match the machine mode,
      // the machine observation state and the machine

      // Test the duration
      if (defaultReasonToTest.MaximumDuration.HasValue
          && (defaultReasonToTest.MaximumDuration.Value < effectiveRange.Duration.Value)) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"TestMachineModeDefaultReasons: machine mode {defaultReasonToTest.MachineMode.Id}, machine observation state {defaultReasonToTest.MachineObservationState.Id}, reason {defaultReasonToTest.Reason.Id}, duration {effectiveRange.Duration.Value} is greater than maximum duration {defaultReasonToTest.MaximumDuration} skip it");
        }
        var remaining = machineModeDefaultReasons.Skip (1);
        return TestMachineModeDefaultReasons (machine, remaining, range, out extendedPeriodReasonSlots, out extendedPeriodStart, out extendedPeriodEnd);
      }

      // Test again the duration with an extended period of time
      // if needed
      bool noDurationInterruption = ExtendToLeft (machine, defaultReasonToTest, ref effectiveRange, extendedPeriodReasonSlots);
      if (noDurationInterruption) {
        noDurationInterruption = ExtendToRight (machine, defaultReasonToTest, ref effectiveRange, extendedPeriodReasonSlots);
      }
      if (!noDurationInterruption) {
        Debug.Assert (defaultReasonToTest.MaximumDuration.HasValue);
        Debug.Assert (effectiveRange.Duration.HasValue);
        Debug.Assert (defaultReasonToTest.MaximumDuration.Value < effectiveRange.Duration.Value);
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ("TestMachineModeDefaultReasons: max duration reached");
        }
        var remaining = machineModeDefaultReasons.Skip (1);
        return TestMachineModeDefaultReasons (machine, remaining, range, out extendedPeriodReasonSlots, out extendedPeriodStart, out extendedPeriodEnd);
      }
      if (defaultReasonToTest.MaximumDuration.HasValue) {
        Debug.Assert (effectiveRange.Duration.HasValue);
        TimeSpan extendedPeriodDuration = effectiveRange.Duration.Value; // It depends on the default reason to test
        if (defaultReasonToTest.MaximumDuration.Value < extendedPeriodDuration) {
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"TestMachineModeDefaultReasons: extended duration {extendedPeriodDuration} is greater than maximum duration {defaultReasonToTest.MaximumDuration} => skip it");
          }
          var remaining = machineModeDefaultReasons.Skip (1);
          return TestMachineModeDefaultReasons (machine, remaining, range, out extendedPeriodReasonSlots, out extendedPeriodStart, out extendedPeriodEnd);
        }
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

      // Ok a good candidate found !
      return defaultReasonToTest;
    }

    IMachineModeDefaultReason TestMachineModeDefaultReasons (IMachine machine, IEnumerable<IMachineModeDefaultReason> machineModeDefaultReasons, UtcDateTimeRange range, Func<IMachineModeDefaultReason, bool> filter)
    {
      Debug.Assert (range.Duration.HasValue);
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      // Note: there is a deadlock on a log
      // I don't know what is the source of the problem yet

      var effectiveRange = range;
      var extendedPeriodReasonSlots = new List<IReasonSlot> ();

      foreach (var defaultReasonToTest in machineModeDefaultReasons.Where (filter)) {

        Debug.Assert (null != defaultReasonToTest.MachineMode);
        Debug.Assert (null != defaultReasonToTest.MachineObservationState);
        Debug.Assert (null != defaultReasonToTest.Reason);

        // Note: it has already been tested the items of machineModeDefaultReasons match the machine mode,
        // the machine observation state and the machine

        // Optimization because extendedPeriodReasonSlots does not need to be returned
        // in this specific method
        if (!defaultReasonToTest.MaximumDuration.HasValue) {
          // Good candidate
          return defaultReasonToTest;
        }

        // Test the duration
        if (defaultReasonToTest.MaximumDuration.HasValue
            && (defaultReasonToTest.MaximumDuration.Value < effectiveRange.Duration.Value)) {
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"TestMachineModeDefaultReasons: duration {effectiveRange?.Duration} is greater than maximum duration {defaultReasonToTest?.MaximumDuration} skip it");
          }
          continue;
        }

        // Test again the duration with an extended period of time
        // if needed
        bool noDurationInterruption = ExtendToLeft (machine, defaultReasonToTest, ref effectiveRange, extendedPeriodReasonSlots);
        if (noDurationInterruption) {
          noDurationInterruption = ExtendToRight (machine, defaultReasonToTest, ref effectiveRange, extendedPeriodReasonSlots);
        }
        if (!noDurationInterruption) {
          Debug.Assert (defaultReasonToTest.MaximumDuration.HasValue);
          Debug.Assert (effectiveRange.Duration.HasValue);
          Debug.Assert (defaultReasonToTest.MaximumDuration.Value < effectiveRange.Duration.Value);
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ("TestMachineModeDefaultReasons: max duration reached");
          }
          continue;
        }
        if (defaultReasonToTest.MaximumDuration.HasValue) {
          Debug.Assert (effectiveRange.Duration.HasValue);
          TimeSpan extendedPeriodDuration = effectiveRange.Duration.Value; // It depends on the default reason to test
          if (defaultReasonToTest.MaximumDuration.Value < extendedPeriodDuration) {
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"TestMachineModeDefaultReasons: extended duration {extendedPeriodDuration} is greater than maximum duration {defaultReasonToTest?.MaximumDuration} => skip it");
            }
            continue;
          }
        }

        // Ok a good candidate found !
        return defaultReasonToTest;
      }

      if (log.IsInfoEnabled && m_logActive) {
        log.Info ($"TestMachineModeDefaultReasons: no default reason was found for {range} with the filter in parameter");
      }
      return null;
    }

    /// <summary>
    /// Get a list of possible machine mode default reasons
    /// 
    /// The result is ordered by ascending duration
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    IEnumerable<IMachineModeDefaultReason> GetMachineModeDefaultReasons (IMachine machine,
                                                                         IMachineObservationState machineObservationState,
                                                                         IMachineMode machineMode)
    {
      var request = new Lemoine.Business.Reason.MachineModeDefaultReasonFind (machine, machineMode, machineObservationState);
      var result = Lemoine.Business.ServiceProvider
        .Get<IEnumerable<IMachineModeDefaultReason>> (request);
      if (!result.Any ()) {
        log.Warn ($"GetMachineModeDefaultReasons: no default reason for machine id {machine.Id} machine observation state id {machineObservationState.Id} and machine mode id {machineMode.Id}");
      }
      if (1 < result.Count (x => !x.MaximumDuration.HasValue)) {
        log.Fatal ($"GetMachineModeDefaultReasons: invalid configuration, different configurations with no maximum duration");
        return new List<IMachineModeDefaultReason> ();
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="defaultReasonToTest"></param>
    /// <param name="range"></param>
    /// <param name="reasonSlots"></param>
    /// <param name="maxRecursionNumber"></param>
    /// <returns>false if interrupted because range duration is longer than the max duration</returns>
    bool ExtendToLeft (IMachine machine, IMachineModeDefaultReason defaultReasonToTest, ref UtcDateTimeRange range, IList<IReasonSlot> reasonSlots, int maxRecursionNumber = 1500)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Duration.HasValue);

      if (defaultReasonToTest.MaximumDuration.HasValue
        && (defaultReasonToTest.MaximumDuration.Value < range.Duration.Value)) {
        return false;
      }

      if (0 == maxRecursionNumber) {
        log.Error ($"ExtendToLeft: max recursion number was reached => give up with range={range} default reason id={defaultReasonToTest.Id}");
        return true;
      }

      var reasonSlotBefore = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindWithEnd (machine,
                      range.Lower.Value);
      if ((null != reasonSlotBefore)
          && (defaultReasonToTest.MachineObservationState.Id == reasonSlotBefore.MachineObservationState.Id)) {
        if (log.IsDebugEnabled && m_logActive) {
          log.Debug ($"ExtendToleft: check if {range} can be extended to {reasonSlotBefore.DateTimeRange.Lower}");
        }
        var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (defaultReasonToTest.MachineMode, reasonSlotBefore.MachineMode);
        var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
          .Get (isDescendantOrSelfOfRequest);
        if (isDescendantOrSelfOf) {
          if (log.IsDebugEnabled && m_logActive) {
            log.Debug ($"ExtendToLeft: extend {range} to {reasonSlotBefore.DateTimeRange.Lower}, default reason id {defaultReasonToTest.Id}");
          }
          var newRange = new UtcDateTimeRange (range.Union (reasonSlotBefore.DateTimeRange));
          if (!newRange.ContainsRange (range) || newRange.Equals (range)) {
            log.Fatal ($"ExtendToLeft: new range {newRange} is not larger than old {range}");
            throw new InvalidOperationException ("Unexpected range in recursion");
          }
          range = newRange;
          reasonSlots.Insert (0, reasonSlotBefore);
          if (!defaultReasonToTest.MaximumDuration.HasValue
            && ((reasonSlotBefore.Reason?.Id ?? 0) == defaultReasonToTest.Reason.Id)
            && Lemoine.Info.ConfigSet.LoadAndGet (INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY, INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT)) {
            // The right reason is already set, there is no reason to continue
            log.Info ($"ExtendToLeft: stop extending the periods since the new slot at {reasonSlotBefore.DateTimeRange} has already the right reason");
            return true;
          }
          // Try again
          return ExtendToLeft (machine, defaultReasonToTest, ref range, reasonSlots, maxRecursionNumber - 1);
        }
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="defaultReasonToTest"></param>
    /// <param name="range"></param>
    /// <param name="reasonSlots"></param>
    /// <param name="maxRecursionNumber"></param>
    /// <returns>false if interrupted because range duration is longer than the max duration</returns>
    bool ExtendToRight (IMachine machine, IMachineModeDefaultReason defaultReasonToTest, ref UtcDateTimeRange range, IList<IReasonSlot> reasonSlots, int maxRecursionNumber = 1500)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Duration.HasValue);

      if (defaultReasonToTest.MaximumDuration.HasValue
        && (defaultReasonToTest.MaximumDuration.Value < range.Duration.Value)) {
        return false;
      }

      if (0 == maxRecursionNumber) {
        log.Error ($"ExtendToRight: max recursion number was reached => give up with range={range} default reason id={defaultReasonToTest.Id}");
        return true;
      }

      // The range may be with upper bound inclusive (unique date/time)
      var dateTimeAfter = range.UpperInclusive ? range.Upper.Value.AddSeconds (1) : range.Upper.Value;
      var reasonSlotAfter = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindAt (machine, dateTimeAfter);
      if ((null != reasonSlotAfter)
          && (defaultReasonToTest.MachineObservationState.Id == reasonSlotAfter.MachineObservationState.Id)) {
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
          if (log.IsDebugEnabled) {
            log.Debug ($"ExtendToRight: check if {range} can be extended to {reasonSlotAfter.DateTimeRange.Upper}");
          }
          var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (defaultReasonToTest.MachineMode, reasonSlotAfter.MachineMode);
          var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
            .Get (isDescendantOrSelfOfRequest);
          if (isDescendantOrSelfOf) {
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"ExtendToRight: extend {range} to {reasonSlotAfter.DateTimeRange.Upper}, default reason id {defaultReasonToTest.Id}");
            }
            var newRange = new UtcDateTimeRange (range.Union (reasonSlotAfter.DateTimeRange));
            if (!newRange.ContainsRange (range) || newRange.Equals (range)) {
              log.Fatal ($"ExtendToRight: new range {newRange} is not larger than old {range}. Slot after={reasonSlotAfter.Id} {reasonSlotAfter.DateTimeRange}. StackTrace={System.Environment.StackTrace}");
              throw new InvalidOperationException ("Unexpected range in recursion");
            }
            range = newRange;
            reasonSlots.Add (reasonSlotAfter);
            if (!defaultReasonToTest.MaximumDuration.HasValue
              && ((reasonSlotAfter.Reason?.Id ?? 0) == defaultReasonToTest.Reason.Id)
              && Lemoine.Info.ConfigSet.LoadAndGet (INTERRUPT_EXTEND_IF_RIGHT_REASON_KEY, INTERRUPT_EXTEND_IF_RIGHT_REASON_DEFAULT)) {
              // The right reason is already set, there is no reason to continue
              log.Info ($"ExtendToRight: stop extending the periods since the new slot at {reasonSlotAfter.DateTimeRange} has already the right reason");
              return true;
            }
            // Try again
            return ExtendToRight (machine, defaultReasonToTest, ref range, reasonSlots, maxRecursionNumber - 1);
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

      var machineModeDefaultReasons = GetMachineModeDefaultReasons (m_machine, machineObservationState, machineMode);
      if (!machineModeDefaultReasons.Any ()) {
        Debug.Assert (false);
        log.Error ($"TryGetActiveAt: no default reason at {at}");
        return new List<IPossibleReason> ();
      }
      else if (autoManualOnly && !machineModeDefaultReasons.Any (r => r.Auto)) { // No auto reason in the list
        log.Info ($"TryGetActiveAt: no auto default reason at {at} => return an empty list");
        return new List<IPossibleReason> ();
      }
      else { // Existing auto default reason
        if (1 == machineModeDefaultReasons.Count ()) {
          var machineModeDefaultReason = machineModeDefaultReasons.First ();
          Debug.Assert (!machineModeDefaultReason.MaximumDuration.HasValue);
          if (machineModeDefaultReason.MaximumDuration.HasValue) {
            log.Error ($"TryGetActiveAt: unique default auto reason with a maximum duration at {at}");
          }
          return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (machineModeDefaultReason.Reason, null,
            machineModeDefaultReason.Score, machineModeDefaultReason.Auto ? ReasonSource.DefaultAuto : ReasonSource.Default, machineModeDefaultReason.OverwriteRequired,
            machineModeDefaultReason.MachineMode,
            machineModeDefaultReason.MachineObservationState) };
        }
        else { // Different default reason, one of them can be an auto one, check the duration
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
              IObservationStateSlot observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
                .FindAt (m_machine, at);
              Debug.Assert (null != observationStateSlot);
              Debug.Assert (observationStateSlot.MachineObservationState.Id == machineObservationState.Id);
              if (observationStateSlot is null) {
                log.Fatal ($"TryGetActiveAt: no observation state slot at {at}");
              }
              else if (observationStateSlot.MachineObservationState.Id != machineObservationState.Id) {
                log.Fatal ($"TryGetActiveAt: unexpected machine observation state at {at}");
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
          }
          if (range.IsEmpty ()) {
            if (log.IsWarnEnabled) {
              log.Warn ($"TryGetActiveAt: no range could be determined at {at} => use a default range of duration 1s");
            }
            range = new UtcDateTimeRange (at.Subtract (TimeSpan.FromSeconds (1)), at);
          }
          Debug.Assert (!range.IsEmpty ());
          IMachineModeDefaultReason machineModeDefaultReason = TestMachineModeDefaultReasons (m_machine,
            machineModeDefaultReasons, range, x => !autoManualOnly || x.Auto);
          if (machineModeDefaultReason is null) {
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"TryGetActiveAt: no valid default reason for machineMode={machineMode.Id} range={range}");
            }
            return new List<IPossibleReason> ();
          }
          else if (!autoManualOnly || machineModeDefaultReason.Auto) {
            return new List<IPossibleReason> { new Pulse.Extensions.Database.PossibleReason (machineModeDefaultReason.Reason, null,
              machineModeDefaultReason.Score, machineModeDefaultReason.Auto ? ReasonSource.DefaultAuto : ReasonSource.Default, machineModeDefaultReason.OverwriteRequired, range) };
          }
          else { // No default reason is applicable
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"TryGetActiveAt: no default reason auto for machineMode={machineMode.Id} range={range}");
            }
            return new List<IPossibleReason> ();
          }
        }
      }
    }

    bool GetRangeFromCurrentMachineMode (DateTime at, IMachineMode machineMode, ref UtcDateTimeRange range)
    {
      Debug.Assert (null != m_machine);

      var currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
        .Find (m_machine);
      if (null == currentMachineMode) {
        log.Error ("GetRangeFromCurrentMachineMode: no current machine mode for machine");
        return false;
      }

      var currentMachineModeRange = new UtcDateTimeRange (currentMachineMode.Change, currentMachineMode.DateTime);
      if (currentMachineModeRange.ContainsElement (at)) {
        if (currentMachineMode.MachineMode.Id != machineMode.Id) {
          log.ErrorFormat ("GetRangeFromCurrentMachineMode: current machine mode {0} differs from {1} at {2}",
            currentMachineMode.MachineMode.Id, machineMode.Id, at);
          return false;
        }
        if (log.IsDebugEnabled && m_logActive) {
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
        if (log.IsDebugEnabled && m_logActive) {
          log.DebugFormat ("GetRangeFromFact: no fact at {0}", at);
        }
        return GetRangeFromLastFact (at, machineMode, ref range);
      }
      else if (fact.CncMachineMode.Id == machineMode.Id) {
        if (log.IsDebugEnabled && m_logActive) {
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
        log.Error ("GetRangeFromLastFact: machine has no fact (GetLast returned null)");
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
      var machineModeDefaultReasons = GetMachineModeDefaultReasons (m_machine, machineObservationState, machineMode);
      return machineModeDefaultReasons.Any (m => m.Reason.Id == reason.Id && m.Score == reasonScore);
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

      var machineModeDefaultReasons = GetMachineModeDefaultReasons (m_machine, machineObservationState, machineMode);
      if (!machineModeDefaultReasons.Any ()) {
        Debug.Assert (false);
        log.Error ($"GetReasonSelections: no default reason in {range}");
        return new List<IReasonSelection> ();
      }
      else if (!machineModeDefaultReasons.Any (r => r.Auto)) { // No auto reason in the list
        if (log.IsDebugEnabled && m_logActive) {
          log.DebugFormat ("GetReasonSelections: no auto default reason in {0} => return an empty list", range);
        }
        return new List<IReasonSelection> ();
      }
      else { // Existing auto default reason
        if (1 == machineModeDefaultReasons.Count ()) {
          var machineModeDefaultReason = machineModeDefaultReasons.First ();
          Debug.Assert (!machineModeDefaultReason.MaximumDuration.HasValue);
          if (machineModeDefaultReason.MaximumDuration.HasValue) {
            log.Error ($"GetReasonSelections: unique default auto reason with a maximum duration in {range}");
          }
          return new List<IReasonSelection> { Convert (machineModeDefaultReason) };
        }
        else { // Different default reason, one of them can be an auto one, check the duration
               // Note: ReasonSlotDAO.FindAllInUtcRange may not return any slot during the analysis process
          IMachineModeDefaultReason machineModeDefaultReason =
            TestMachineModeDefaultReasons (m_machine,
              machineModeDefaultReasons, range, x => x.Auto);
          if (machineModeDefaultReason is null) {
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"GetReasonSelections: no valid default reason");
            }
            return new List<IReasonSelection> ();
          }
          else if (machineModeDefaultReason.Auto) {
            return new List<IReasonSelection> { Convert (machineModeDefaultReason) };
          }
          else { // No default reason is applicable
            if (log.IsDebugEnabled && m_logActive) {
              log.Debug ($"GetReasonSelections: no auto default reason");
            }
            return new List<IReasonSelection> ();
          }
        }
      }
    }

    IReasonSelection Convert (IMachineModeDefaultReason machineModeDefaultReason)
    {
      var reasonScore = Lemoine.Info.ConfigSet.LoadAndGet<double> (REASON_SELECTION_SCORE_KEY,
        REASON_SELECTION_SCORE_DEFAULT);
      return new Pulse.Extensions.Database.ExtraReasonSelection (machineModeDefaultReason.MachineMode,
        machineModeDefaultReason.MachineObservationState,
        machineModeDefaultReason.Reason,
        reasonScore);
    }

    #region IReasonLegendExtension implementation
    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReason> GetUsedReasons ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var reasons = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
          .FindWithReasonGroup ()
          .Select (x => x.Reason)
          .Distinct ();
        // Note: normally not required because of the early fetch of the reason and reason group in FindWithReasonGroup
        foreach (var reason in reasons) {
          ModelDAOHelper.DAOFactory.Initialize (reason.ReasonGroup);
        }
        return reasons;
      }
    }
    #endregion // IReasonLegendExtension implementation
  }
}
