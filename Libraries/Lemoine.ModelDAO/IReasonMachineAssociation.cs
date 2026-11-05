// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.I18N;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lemoine.Model
{
  /// <summary>
  /// Reason machine association kind
  /// </summary>
  [Flags]
  public enum ReasonMachineAssociationKind
  {
    /// <summary>
    /// Not set (must be set to a different values before processing it)
    /// 
    /// It was previously Consolidate, which is now deprecated
    /// </summary>
    NotSet = 0,
    /// <summary>
    /// Auto reason (no overwrite required)
    /// </summary>
    Auto = 2,
    /// <summary>
    /// Auto reason (with an ovewrite required)
    /// </summary>
    AutoWithOverwriteRequired = 3,
    /// <summary>
    /// Manual reason (no overwrite required if the reason id is not null)
    /// </summary>
    Manual = 4,
    /// <summary>
    /// Track only a dynamic end
    /// </summary>
    TrackDynamicEnd = 8,
    /// <summary>
    /// Reset the reason slots because the specified reason was cancelled.
    /// 
    /// Switch to the Processing reason is required.
    /// </summary>
    Reset = 16,
  }

  /// <summary>
  /// Extension to the enum ReasonMachineAssociationKind
  /// </summary>
  public static class ReasonMachineAssociationKindExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (ReasonMachineAssociationKindExtensions).FullName);

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this ReasonMachineAssociationKind t, ReasonMachineAssociationKind other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static ReasonMachineAssociationKind Add (this ReasonMachineAssociationKind t, ReasonMachineAssociationKind? other)
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
    public static ReasonMachineAssociationKind Remove (this ReasonMachineAssociationKind t, ReasonMachineAssociationKind toRemove)
    {
      return t & ~toRemove;
    }

    /// <summary>
    /// Does the Reason machine association correspond to an auto-reason
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsAuto (this ReasonMachineAssociationKind kind)
    {
      return kind.Equals (ReasonMachineAssociationKind.Auto)
        || kind.Equals (ReasonMachineAssociationKind.AutoWithOverwriteRequired);
    }

    /// <summary>
    /// Does the Reason machine association correspond to a manual reason (Manual or Reset)
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsManual (this ReasonMachineAssociationKind kind)
    {
      return kind.Equals (ReasonMachineAssociationKind.Manual);
    }

    /// <summary>
    /// Does the reason machine association correspond to an 'overwrite required' reason
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsOvewriteRequired (this ReasonMachineAssociationKind kind)
    {
      return kind.Equals (ReasonMachineAssociationKind.AutoWithOverwriteRequired);
    }

    /// <summary>
    /// Convert the reason machine association kind to a reason source
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static ReasonSource ConvertToReasonSource (this ReasonMachineAssociationKind kind)
    {
      switch (kind) {
        case ReasonMachineAssociationKind.Auto:
        case ReasonMachineAssociationKind.AutoWithOverwriteRequired:
          return ReasonSource.Auto;
        case ReasonMachineAssociationKind.Manual:
          return ReasonSource.Manual;
        default:
          log.ErrorFormat ("ConvertToReasonSource: {0} can't be converted. StackTrace={1}", kind, System.Environment.StackTrace);
          throw new InvalidCastException ("Invalid cast from ReasonMachineAssociationKind to ReasonSource");
      }
    }

    /// <summary>
    /// Convert it to a ReasonProposalKind
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static ReasonProposalKind ConvertToReasonProposalKind (this ReasonMachineAssociationKind kind)
    {
      switch (kind) {
        case ReasonMachineAssociationKind.Auto:
          return ReasonProposalKind.Auto;
        case ReasonMachineAssociationKind.AutoWithOverwriteRequired:
          return ReasonProposalKind.AutoWithOverwriteRequired;
        case ReasonMachineAssociationKind.Manual:
          return ReasonProposalKind.Manual;
        default:
          log.ErrorFormat ("ConvertToReasonProposalKind: {0} can't be converted. StackTrace={1}", kind, System.Environment.StackTrace);
          throw new InvalidCastException ("Invalid cast from ReasonMachineAssociationKind to ReasonProposalKind");
      }
    }
  }

  /// <summary>
  /// Model of table ReasonMachineAssociation
  /// 
  /// This new table is designed to add any reason change to a machine.
  /// This is required since the new Fact table now records
  /// only the raw data from the CNC.
  /// 
  /// It does not represent the current reasons of a machine,
  /// but all the manual or automatic reason changes that have been made.
  /// 
  /// To know the current reasons of a machine, the table Reason Slot
  /// that is filled in by the Analyzer must be used.
  /// </summary>
  public interface IReasonMachineAssociation : IMachineAssociation, IPossibleReason
  {
    /// <summary>
    /// Is the reason machine association applicable on top of the specified reason slot ?
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    bool IsApplicable (IReasonSlot reasonSlot);

    /// <summary>
    /// Clone a ReasonMachineAssociation with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IReasonMachineAssociation Clone (UtcDateTimeRange range);

    /// <summary>
    /// Set a manual reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    void SetManualReason (IReason reason, double? reasonScore, string details = null, string jsonData = null);

    /// <summary>
    /// Set a manual reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="details"></param>
    void SetManualReason (IReason reason, string details = null, string jsonData = null);

    /// <summary>
    /// Reset a manual reason
    /// </summary>
    void ResetManualReason ();

    /// <summary>
    /// Set an auto reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="details"></param>
    void SetAutoReason (IReason reason, double score, bool overwriteRequired, string details = null, string jsonData = null);

    /// <summary>
    /// Consider it as a dynamic end tracker
    /// </summary>
    /// <param name="dynamicEnd"></param>
    /// <param name="hint">not empty</param>
    void SetDynamicEndTracker (string dynamicEnd, UtcDateTimeRange hint);

    /// <summary>
    /// Reason score
    /// </summary>
    double? OptionalReasonScore { get; }

    /// <summary>
    /// Kind of reason machine association
    /// </summary>
    ReasonMachineAssociationKind Kind { get; }

    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; }

    /// <summary>
    /// Dynamic times (start/end) description: start,end
    /// 
    /// If start or end ends with +, then the reference date/time is the upper bound of the date/time range,
    /// else it is by default the lower bound of the date/time range
    /// 
    /// If Dynamic starts with "?", then switch to aggressive mode
    /// </summary>
    string Dynamic { get; set; }

    /// <summary>
    /// Dynamic start
    /// </summary>
    string DynamicStart { get; }

    /// <summary>
    /// Dynamic end
    /// </summary>
    string DynamicEnd { get; }

    /// <summary>
    /// Force the end of a reason machine association whose dynamic end is not resolved yet,
    /// as if the dynamic end was reached at the specified date/time.
    ///
    /// It must be called on a root modification (with no parent)
    /// that is not completed yet (including PendingSubModifications).
    ///
    /// <list type="bullet">
    /// <item>the reason proposal is shortened, and a Reset sub-modification is created to cancel the reason slots after end</item>
    /// <item>the not completed sub-modifications that track the dynamic end are cancelled</item>
    /// <item>the not completed sub-modifications (or the root itself) that are still applying the reason with a dynamic end (aggressive or progressive)
    /// or after end, are cancelled and replaced by a sub-modification that applies the reason until end</item>
    /// <item>the final status of the root modification will be Cancel</item>
    /// <item>an analysis log with level INFO is added</item>
    /// </list>
    ///
    /// The reason slots are not updated directly: this is done later by the analysis service
    /// that processes the new sub-modifications.
    ///
    /// The caller is responsible for the transaction (preferably with a lock on the machine partitions
    /// of the tables machinemodification and reasonmachineassociation).
    /// </summary>
    /// <param name="end">UTC date/time when the reason must stop</param>
    /// <param name="message">message to add in the analysis log</param>
    /// <returns>description of the actions that were made</returns>
    IList<string> ForceDynamicEnd (DateTime end, string message);

    /// <summary>
    /// Cancel completely a reason machine association whose dynamic end is not resolved yet,
    /// including the part that was already applied.
    ///
    /// This is equivalent to <see cref="ForceDynamicEnd(DateTime, string)"/>
    /// with an end at the start of the modification (or of its reason proposal if it starts before):
    /// the reason proposal is removed, the reason slots are reset in its whole range
    /// and all the not completed modifications of the tree are cancelled.
    ///
    /// The same conditions and transaction rules as <see cref="ForceDynamicEnd(DateTime, string)"/> apply.
    /// </summary>
    /// <param name="message">message to add in the analysis log</param>
    /// <returns>description of the actions that were made</returns>
    IList<string> CancelAutoReason (string message);
  }

  /// <summary>
  /// Extensions to <see cref="IReasonMachineAssociation"/>
  /// </summary>
  public static class ReasonMachineAssociationExtension
  {
  }
}
