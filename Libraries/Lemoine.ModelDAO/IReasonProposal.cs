// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Collections;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Reason proposal kind
  /// </summary>
  public enum ReasonProposalKind
  {
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
  }

  /// <summary>
  /// Extension to the enum ReasonProposalKind
  /// </summary>
  public static class ReasonProposalKindExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (ReasonProposalKindExtensions).FullName);

    /// <summary>
    /// Does the Reason machine association correspond to an auto-reason
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsAuto (this ReasonProposalKind kind)
    {
      return kind.Equals (ReasonProposalKind.Auto)
        || kind.Equals (ReasonProposalKind.AutoWithOverwriteRequired);
    }

    /// <summary>
    /// Does the Reason machine association correspond to a manual reason (Manual or Reset)
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsManual (this ReasonProposalKind kind)
    {
      return kind.Equals (ReasonProposalKind.Manual);
    }

    /// <summary>
    /// Does the reason machine association correspond to an 'overwrite required' reason
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static bool IsOverwriteRequired (this ReasonProposalKind kind)
    {
      return kind.Equals (ReasonProposalKind.AutoWithOverwriteRequired);
    }

    /// <summary>
    /// Convert the kind to a reason source
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static ReasonSource ConvertToReasonSource (this ReasonProposalKind kind)
    {
      switch (kind) {
        case ReasonProposalKind.Auto:
        case ReasonProposalKind.AutoWithOverwriteRequired:
          return ReasonSource.Auto;
        case ReasonProposalKind.Manual:
          return ReasonSource.Manual;
        default:
          log.ErrorFormat ("ConvertToReasonSource: {0} can't be converted", kind);
          throw new InvalidCastException ();
      }
    }
  }

  /// <summary>
  /// Model interface for the table reasonproposal
  /// </summary>
  public interface IReasonProposal
    : IDataWithId
    , IVersionable
    , IPartitionedByMachine
    , IPossibleReason
  {
    /// <summary>
    /// Associated reason machine association
    /// </summary>
    long ModificationId { get; }
    
    /// <summary>
    /// Associated range
    /// </summary>
    UtcDateTimeRange DateTimeRange { get; set; }

    /// <summary>
    /// Reason kind: auto / manual
    /// </summary>
    ReasonProposalKind Kind { get; }
  }
}
