// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Extensions;
using System.Text.Json;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Required reset kind
  /// </summary>
  [Flags]
  public enum RequiredResetKind
  {
    /// <summary>
    /// No reset is required
    /// </summary>
    None = 0,
    /// <summary>
    /// The reset of the number of auto-reason number is required
    /// </summary>
    ExtraAuto = 1,
    /// <summary>
    /// The reset of the extra manual reason flag is required
    /// </summary>
    ExtraManual = 2,
    /// <summary>
    /// A reset of the main reason is required
    /// </summary>
    Main = 4,
    /// <summary>
    /// A full reset is required (including of the main and extra reasons)
    /// </summary>
    Full = 7,
  }

  /// <summary>
  /// Extensions to the RequiredResetKind enum
  /// </summary>
  public static class RequiredResetKindExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (RequiredResetKindExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this RequiredResetKind t, RequiredResetKind other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static RequiredResetKind Add (this RequiredResetKind t, RequiredResetKind? other)
    {
      if (other.HasValue) {
        return t | other.Value;
      }
      else {
        return t;
      }
    }

    /// <summary>
    /// Remove an option
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static RequiredResetKind Remove (this RequiredResetKind t, RequiredResetKind toRemove)
    {
      return t & ~toRemove;
    }
  }

  /// <summary>
  /// Possible extra reason
  /// </summary>
  public class PossibleReason : IPossibleReason
  {
    /// <summary>
    /// Associated reason
    /// </summary>
    public IReason Reason { get; set; }

    /// <summary>
    /// Additional data in Json format
    /// </summary>
    public string JsonData => JsonSerializer.Serialize (this.Data);

    /// <summary>
    /// Additional data
    /// </summary>
    public IDictionary<string, object> Data { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    public string ReasonDetails { get; set; }

    /// <summary>
    /// Reason score
    /// </summary>
    public double ReasonScore { get; set; }

    /// <summary>
    /// Reason source
    /// </summary>
    public ReasonSource ReasonSource { get; set; }

    /// <summary>
    /// Overwrite required
    /// </summary>
    public bool OverwriteRequired { get; set; }

    /// <summary>
    /// <see cref="IPossibleReason"/> 
    /// </summary>
    public UtcDateTimeRange RestrictedRange { get; set; }

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineMode RestrictedMachineMode { get; set; }

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineObservationState RestrictedMachineObservationState { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="details"></param>
    /// <param name="score"></param>
    /// <param name="source"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="restrictedRange"></param>
    public PossibleReason (IReason reason, string details, double score, ReasonSource source, bool overwriteRequired)
    {
      this.Reason = reason;
      this.ReasonDetails = details;
      this.ReasonScore = score;
      this.ReasonSource = source;
      this.OverwriteRequired = overwriteRequired;
      this.RestrictedRange = new UtcDateTimeRange ("(,)");
      this.RestrictedMachineMode = null;
      this.RestrictedMachineObservationState = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="details"></param>
    /// <param name="score"></param>
    /// <param name="source"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="restrictedRange"></param>
    public PossibleReason (IReason reason, string details, double score, ReasonSource source, bool overwriteRequired, UtcDateTimeRange restrictedRange)
    {
      this.Reason = reason;
      this.ReasonDetails = details;
      this.ReasonScore = score;
      this.ReasonSource = source;
      this.OverwriteRequired = overwriteRequired;
      this.RestrictedRange = restrictedRange;
      this.RestrictedMachineMode = null;
      this.RestrictedMachineObservationState = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="details"></param>
    /// <param name="score"></param>
    /// <param name="source"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="restrictedMachineMode"></param>
    /// <param name="restrictedMachineObservationState"></param>
    public PossibleReason (IReason reason, string details, double score, ReasonSource source, bool overwriteRequired, IMachineMode restrictedMachineMode, IMachineObservationState restrictedMachineObservationState)
    {
      this.Reason = reason;
      this.ReasonDetails = details;
      this.ReasonScore = score;
      this.ReasonSource = source;
      this.OverwriteRequired = overwriteRequired;
      this.RestrictedRange = new UtcDateTimeRange ("(,)");
      this.RestrictedMachineMode = restrictedMachineMode;
      this.RestrictedMachineObservationState = restrictedMachineObservationState;
    }
  }

  /// <summary>
  /// Possible additional reason selection
  /// </summary>
  public class ExtraReasonSelection : IReasonSelection
  {
    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public bool DetailsRequired
    {
      get; set;
    }

    /// <summary>
    /// Id. Not used, always 0
    /// </summary>
    public int Id
    {
      get { return 0; }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public IMachineFilter MachineFilter
    {
      get; set;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public IMachineMode MachineMode
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public IMachineObservationState MachineObservationState
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public IReason Reason
    {
      get; set;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public double ReasonScore
    {
      get; private set;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IReasonSelection" />
    /// </summary>
    public bool Selectable
    {
      get; set;
    }

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public string AlternativeText { get; set; }

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public string AlternativeLongText { get; set; }

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public string AlternativeDescription { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    public IDictionary<string, object> Data { get; set; }

    /// <summary>
    /// Version. Not used (always 0)
    /// </summary>
    public int Version
    {
      get { return 0; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    public ExtraReasonSelection (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, string alternativeText = null, IDictionary<string, object> data = null)
    {
      this.MachineMode = machineMode;
      this.MachineObservationState = machineObservationState;
      this.Reason = reason;
      this.ReasonScore = reasonScore;
      this.Selectable = true;
      this.AlternativeText = alternativeText;
      this.Data = data;
    }
  }

  /// <summary>
  /// Interface for the plugins to update the default reason
  /// </summary>
  public interface IReasonExtension : IExtension, IInitializedByMachineExtension
  {
    /// <summary>
    /// Start a batch process of ProcessingReasonSlotsAnalysis
    /// </summary>
    /// <returns></returns>
    void StartBatch ();

    /// <summary>
    /// Preload some reference data in case of a batch process
    /// </summary>
    /// <param name="range"></param>
    void PreLoad (UtcDateTimeRange range);

    /// <summary>
    /// Start a batch process of ProcessingReasonSlotsAnalysis
    /// </summary>
    /// <returns></returns>
    void EndBatch ();

    /// <summary>
    /// Maximum reason score this plugin may associate to a reason for the specified reason slot
    /// 
    /// This has an impact on performance
    /// 
    /// <item>null may be returned (although not recommended) if this is too complex to get the maximum score</item>
    /// <item>return a negative value if no reason may apply</item>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    /// <returns></returns>
    double? GetMaximumScore (IReasonSlot newReasonSlot);

    /// <summary>
    /// May this extension apply auto reasons ?
    /// </summary>
    /// <param name="newReasonSlot"></param>
    /// <returns></returns>
    bool MayApplyAutoReasons (IReasonSlot newReasonSlot);

    /// <summary>
    /// May this extension apply manual reasons
    /// </summary>
    /// <param name="newReasonSlot"></param>
    /// <returns></returns>
    bool MayApplyManualReasons (IReasonSlot newReasonSlot);

    /// <summary>
    /// Is a reset required ?
    /// 
    /// There is no need to consider ReasonSlotChange.Requested and ReasonSlotChange.ResetManual here
    /// (they are automatically considered as required).
    /// 
    /// This is not necessary to take care of the cases where:
    /// <item>the reason of newReasonSlot is null</item>
    /// <item>the reason of newReasonSlot is Processing</item>
    /// <item>the reason score is negative</item>
    /// All these cases won't happen because they are processes elsewhere.
    /// </summary>
    /// <param name="oldReasonSlot"></param>
    /// <param name="newReasonSlot"></param>
    /// <param name="modification">nullable</param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange);

    /// <summary>
    /// Is reset applicable for this extension ?
    /// </summary>
    /// <param name="reasonSource"></param>
    /// <param name="reasonScore"></param>
    /// <param name="autoReasonNumber"></param>
    /// <returns></returns>
    bool IsResetApplicable (ReasonSource reasonSource, double reasonScore, int autoReasonNumber);

    /// <summary>
    /// Update the default reason after it was set to Processing
    /// 
    /// reasonSlot may be updated and the new reason slot may match a shorter range
    /// </summary>
    /// <param name="reasonSlot">reason slot (not null)</param>
    void TryResetReason (ref IReasonSlot reasonSlot);

    /// <summary>
    /// Try to get a set of possible reasons at the specified date/time.
    /// 
    /// There is an option to return only auto and manual reasons,
    /// and not return the default (only) reasons
    /// </summary>
    /// <param name="at"></param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="autoManualOnly"></param>
    /// <returns></returns>
    IEnumerable<IPossibleReason> TryGetActiveAt (DateTime at, IMachineMode machineMode, IMachineObservationState machineObservationState, bool autoManualOnly);

    /// <summary>
    /// Is a reason compatible with the machine mode and the machine observation state ?
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="reasonSource"></param>
    /// <returns></returns>
    bool IsCompatible (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, ReasonSource reasonSource);
  }

  /// <summary>
  /// Extensions to the interface IReasonExtension
  /// </summary>
  public static class ReasonExtensionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonExtensionExtensions));

    /// <summary>
    /// Try to set a default reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="t"></param>
    /// <param name="reasonSlot">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="consolidationLimit"></param>
    public static void TryDefaultReason (this IReasonExtension t, IReasonSlot reasonSlot, IReason reason, double score, bool overwriteRequired, bool auto, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != reasonSlot);

      reasonSlot.TryDefaultReasonInReset (reason, score, overwriteRequired, auto, consolidationLimit);
    }

    /// <summary>
    /// Try to set a manual reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="t"></param>
    /// <param name="reasonSlot">not null</param>
    /// <param name="reasonMachineAssociation">not null and reasonMachineAssociation.Reason not null</param>
    /// <param name="consolidationLimit"></param>
    /// <returns>a manual reason was applied</returns>
    public static bool TryManualReason (this IReasonExtension t, IReasonSlot reasonSlot, IReasonMachineAssociation reasonMachineAssociation, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != reasonSlot);

      return reasonSlot.TryManualReasonInReset (reasonMachineAssociation, consolidationLimit);
    }

    /// <summary>
    /// Try to set an auto reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// after checking the reason is compatible in reasonSlot
    /// </summary>
    /// <param name="t"></param>
    /// <param name="reasonSlot">not null</param>
    /// <param name="reasonProposal">not null and reasonProposal.Reason not null</param>
    /// <param name="consolidationLimit"></param>
    /// <param name="compatibilityCheck">check the compatibility is ok first (false for the reasons from the reasonproposal table)</param>
    /// <returns>a reason was applied</returns>
    public static bool TryAutoReason (this IReasonExtension t, IReasonSlot reasonSlot, IReasonProposal reasonProposal, UpperBound<DateTime> consolidationLimit, bool compatibilityCheck)
    {
      Debug.Assert (null != reasonSlot);

      return reasonSlot.TryAutoReasonInReset (reasonProposal, consolidationLimit, compatibilityCheck);
    }
  }
}
