// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Sequence name constants
  /// </summary>
  internal class SequenceName
  {
    /// <summary>
    /// Event Id sequence
    /// </summary>
    internal static readonly string EVENT_ID = "event_eventid_seq";
    
    /// <summary>
    /// Right Id sequence
    /// </summary>
    internal static readonly string RIGHT_ID = "right_rightid_seq";
    
    /// <summary>
    /// Machine Filter Item Id sequence
    /// </summary>
    internal static readonly string MACHINE_FILTER_ITEM_ID = "machinefilteritemid_seq";
  }
}
