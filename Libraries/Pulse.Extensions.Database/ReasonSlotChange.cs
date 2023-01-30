// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Reason slot change flags
  /// </summary>
  [Flags]
  public enum ReasonSlotChange
  {
    /// <summary>
    /// No change was requested
    /// </summary>
    None = 0,
    /// <summary>
    /// The reson slot corresponds to a new CNC activity
    /// </summary>
    NewActivity = 1, // 1 << 0
    /// <summary>
    /// The applicable period changed
    /// </summary>
    Period = 2, // 1 << 1
    /// <summary>
    /// The machine mode changed
    /// </summary>
    MachineMode = 4, // 1 << 2
    /// <summary>
    /// The machine observation state changed
    /// </summary>
    MachineObservationState = 8, // 1 << 3
    /// <summary>
    /// Resetting a manual reason was requested
    /// </summary>
    ResetManual = 16, // 1 << 4
    /// <summary>
    /// The consolidation of the reason slot was requested
    /// </summary>
    Requested = 32, // 1 << 5
    /// <summary>
    /// The reason was updated (for a production state update)
    /// </summary>
    Reason = 64, // 1 << 6
  }

  /// <summary>
  /// Extensions to the reason slot change flags
  /// </summary>
  public static class ReasonSlotChangeExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (ReasonSlotChangeExtensions).ToString ());

    /// <summary>
    /// Returns true if no reason slot change was requested
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsEmpty (this ReasonSlotChange t)
    {
      return t.Equals (ReasonSlotChange.None);
    }

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this ReasonSlotChange t, ReasonSlotChange other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add a change to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static ReasonSlotChange Add (this ReasonSlotChange t, ReasonSlotChange? other)
    {
      if (other.HasValue) {
        return t | other.Value;
      }
      else {
        return t;
      }
    }

    /// <summary>
    /// Remove a change
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static ReasonSlotChange Remove (this ReasonSlotChange t, ReasonSlotChange toRemove)
    {
      return t & ~toRemove;
    }
  }

}
