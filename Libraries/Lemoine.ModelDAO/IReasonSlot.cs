// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Model
{
  /// <summary>
  /// Reason source
  /// </summary>
  [Flags]
  public enum ReasonSource
  {
    /// <summary>
    /// Invalid one (not set)
    /// </summary>
    Invalid = 0,
    /// <summary>
    /// Default reason only
    /// </summary>
    Default = 1,
    /// <summary>
    /// The main reason is an Auto reason (with no extra manual reason)
    /// </summary>
    Auto = 2,
    /// <summary>
    /// Default reason that can be considered as an Auto reason as well
    /// </summary>
    DefaultAuto = 3,
    /// <summary>
    /// The main reason is a manual reason
    /// </summary>
    Manual = 4,
    /// <summary>
    /// Default reason (main one) with an extra manual reason
    /// 
    /// This should not be used in theory, this configuration does not make much sense
    /// </summary>
    DefaultManual = 5,
    /// <summary>
    /// Auto reason (main one) with an extra manual reason
    /// </summary>
    AutoManual = 6,
    /// <summary>
    /// AutoDefault reason (main one) with an extra manual reason
    /// 
    /// This should not be used in theory, this configuration does not make much sense
    /// </summary>
    DefaultAutoManual = 7,
    /// <summary>
    /// Is the Manual flag unsafe ?
    /// </summary>
    UnsafeManualFlag = 8,
    /// <summary>
    /// Is the auto-reason number unsafe ?
    /// </summary>
    UnsafeAutoReasonNumber = 16,
    /// <summary>
    /// The associated default reason (not the main one) is an auto-reason
    /// </summary>
    DefaultIsAuto = 32,
  }

  /// <summary>
  /// Extensions to the reason source
  /// </summary>
  public static class ReasonSourceExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (ReasonSourceExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this ReasonSource t, ReasonSource other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static ReasonSource Add (this ReasonSource t, ReasonSource? other)
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
    public static ReasonSource Remove (this ReasonSource t, ReasonSource toRemove)
    {
      return t & ~toRemove;
    }

    /// <summary>
    /// Is this a default reason ?
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsDefault (this ReasonSource t)
    {
      return t.HasFlag (ReasonSource.Default);
    }

    /// <summary>
    /// Is this an auto reason ?
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsAuto (this ReasonSource t)
    {
      return t.HasFlag (ReasonSource.Auto);
    }

    /// <summary>
    /// Set the main source as an auto source (not default)
    /// </summary>
    /// <param name="source"></param>
    public static ReasonSource SetMainAuto (this ReasonSource source)
    {
      if (source.HasFlag (ReasonSource.Manual)) {
        return ReasonSource.AutoManual;
      }
      else {
        return ReasonSource.Auto;
      }
    }

    /// <summary>
    /// Set the main source as a default source (not auto)
    /// </summary>
    /// <param name="source"></param>
    public static ReasonSource SetMainDefault (this ReasonSource source)
    {
      if (source.HasFlag (ReasonSource.Manual)) {
        return ReasonSource.DefaultManual;
      }
      else {
        return ReasonSource.Default;
      }
    }

    /// <summary>
    /// Set the main source as an default+auto source
    /// </summary>
    /// <param name="source"></param>
    public static ReasonSource SetMainDefaultAuto (this ReasonSource source)
    {
      if (source.HasFlag (ReasonSource.Manual)) {
        return ReasonSource.DefaultAutoManual;
      }
      else {
        return ReasonSource.DefaultAuto;
      }
    }

    /// <summary>
    /// Add an extra manual source to an existing reason
    /// </summary>
    /// <param name="source"></param>
    public static ReasonSource AddExtraManual (this ReasonSource source)
    {
      return source.Add (ReasonSource.Manual);
    }

    /// <summary>
    /// Reset the manual source
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReasonSource ResetManual (this ReasonSource source)
    {
      if (source.Equals (ReasonSource.Manual)) {
        return ReasonSource.Default;
      }
      else {
        return source.Remove (ReasonSource.Manual);
      }
    }

    /// <summary>
    /// Set the number of auto-reasons as unsafe
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReasonSource SetUnsafeAutoReasonNumber (this ReasonSource source)
    {
      return source.Add (ReasonSource.UnsafeAutoReasonNumber);
    }

    /// <summary>
    /// Set the manual flag as unsafe
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReasonSource SetUnsafeManualFlag (this ReasonSource source)
    {
      return source.Add (ReasonSource.UnsafeManualFlag);
    }

    /// <summary>
    /// Check if two reason sources correspond to the same main source
    /// </summary>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool IsSameMainSource (this ReasonSource source, ReasonSource other)
    {
      return (source.Equals (other))
        || (source.IsAuto () && other.IsAuto ())
        || (source.IsDefault () && other.IsDefault ())
        || (source.Equals (ReasonSource.Manual) && other.Equals (ReasonSource.Manual));
    }
  }

  /// <summary>
  /// IPossibleReason interface
  /// 
  /// This is used by IReasonSlot and some other extensions
  /// </summary>
  public interface IPossibleReason
  {
    /// <summary>
    /// Reference to the Reason
    /// 
    /// null corresponds to a default reason in IReasonSlot
    /// </summary>
    IReason Reason { get; }

    /// <summary>
    /// Additional reason data
    /// </summary>
    IDictionary<string, object> Data { get; }

    /// <summary>
    /// Additional reason data in Json format
    /// </summary>
    string JsonData { get; }

    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    bool OverwriteRequired { get; }

    /// <summary>
    /// Reason details
    /// </summary>
    string ReasonDetails { get; }

    /// <summary>
    /// Reason score
    /// </summary>
    double ReasonScore { get; }

    /// <summary>
    /// Reason source
    /// </summary>
    ReasonSource ReasonSource { get; }

    /// <summary>
    /// Restricted range if applicable.
    /// 
    /// If not applicable, (-oo,+oo) can be used
    /// </summary>
    UtcDateTimeRange RestrictedRange { get; }

    /// <summary>
    /// Restrict this possible reason to this machine mode.
    /// 
    /// Alternative to the RestrictedRange property.
    /// 
    /// If not applicable, null
    /// </summary>
    IMachineMode RestrictedMachineMode { get; }

    /// <summary>
    /// Restricted this possible reason to this machine observation state.
    /// 
    /// Alternative to the RestrictedRange property.
    /// 
    /// If not applicable, null
    /// </summary>
    IMachineObservationState RestrictedMachineObservationState { get; }
  }

  /// <summary>
  /// Model of table ReasonSlot
  /// 
  /// Analysis table where are stored all
  /// the Machine Mode and Reason periods of a given machine.
  /// </summary>
  public interface IReasonSlot
    : ISlot
    , IPossibleReason
    , IPartitionedByMachine
  {
    /// <summary>
    /// Reference to the Machine Mode
    /// 
    /// Not null
    /// </summary>
    IMachineMode MachineMode { get; set; }

    /// <summary>
    /// Cancel any data in the reason slot, that is related to the reason (number of auto-reasons, kind...)
    /// </summary>
    void CancelData ();

    /// <summary>
    /// Switch the reason slot to a processing status (so that the slot is consolidated again later)
    /// </summary>
    void SwitchToProcessing ();

    /// <summary>
    /// Try to set a default reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="consolidationLimit"></param>
    /// <returns>the new main reason is the default one</returns>
    bool TryDefaultReasonInReset (IReason reason, double score, bool overwriteRequired, bool auto, UpperBound<DateTime> consolidationLimit);

    /// <summary>
    /// Update if applicable the default reason (which may be not valid any more)
    /// on a neighbor reason slot that is not processing
    /// </summary>
    /// <param name="machineModeDefaultReason"></param>
    /// <param name="consolidationLimit"></param>
    /// <returns>the default reason was applied and it the main one</returns>
    bool TryUpdateDefaultReason (IMachineModeDefaultReason machineModeDefaultReason, UpperBound<DateTime> consolidationLimit);

    /// <summary>
    /// Try to set a manual reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="reasonMachineAssociation">not null</param>
    /// <param name="consolidationLimit"></param>
    /// <returns>a manual reason was applied</returns>
    bool TryManualReasonInReset (IReasonMachineAssociation reasonMachineAssociation, UpperBound<DateTime> consolidationLimit);

    /// <summary>
    /// Try to set an auto reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="possibleReason">not null, possibleReason.Reason not null</param>
    /// <param name="consolidationLimit"></param>
    /// <param name="compatibilityCheck"></param>
    /// <returns>a reason was applied</returns>
    bool TryAutoReasonInReset (IPossibleReason possibleReason, UpperBound<DateTime> consolidationLimit, bool compatibilityCheck);

    /// <summary>
    /// Update the machine status if applicable
    /// </summary>
    void UpdateMachineStatusIfApplicable ();

    /// <summary>
    /// Merge a reason slot with the next one
    /// 
    /// MakePersistent on this and MakeTransient on reasonSlot after calling it
    /// </summary>
    /// <param name="reasonSlot"></param>
    void MergeWithNext (IReasonSlot reasonSlot);

    /// <summary>
    /// Reference to the MachineObservationState
    /// 
    /// A null MachineObservationState can't be set
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }

    /// <summary>
    /// Reference to the Shift
    /// 
    /// nullable
    /// </summary>
    IShift Shift { get; set; }

    /// <summary>
    /// Default reason ?
    /// </summary>
    bool DefaultReason { get; }

    /// <summary>
    /// Number of auto reasons
    /// </summary>
    int AutoReasonNumber { get; }

    /// <summary>
    /// Flag the number of auto-reasons as unsafe
    /// </summary>
    void SetUnsafeAutoReasonNumber ();

    /// <summary>
    /// Set the extra manual flag as unsafe
    /// </summary>
    void SetUnsafeManualFlag ();

    /// <summary>
    /// Is the machine module considered running ?
    /// </summary>
    bool Running { get; }

    /// <summary>
    /// Is the machine module considered as not running ?
    /// </summary>
    bool NotRunning { get; }

    /// <summary>
    /// Reference to the production state
    /// 
    /// nullable
    /// </summary>
    IProductionState ProductionState { get; set; }

    /// <summary>
    /// Production rate
    /// 
    /// nullable
    /// </summary>
    double? ProductionRate { get; set; }

    /// <summary>
    /// Consolidate the production state and rate
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="association"></param>
    void ConsolidateProductionStateRate (IReasonSlot oldSlot, IPeriodAssociation association);
  }

  /// <summary>
  /// Extensions to <see cref="IReasonSlot"/>
  /// </summary>
  public static class ReasonSlotExtensions
  {
    /// <summary>
    /// Does this reason slot correspond to a processing period ?
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public static bool IsProcessing (this IReasonSlot reasonSlot) => (int)ReasonId.Processing == reasonSlot.Reason.Id;
  }
}
