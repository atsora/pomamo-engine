// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Cnc.SQLiteQueue
{
  /// <summary>
  /// connection "synchronous" parameter choices
  /// </summary>
  public enum SQLiteSynchroMode {
    /// <summary>
    /// Full Synchronization Mode
    /// </summary>
    Full,
    /// <summary>
    /// Normal Synchronization Mode
    /// </summary>
    Normal,
    /// <summary>
    /// Off Synchronization Mode
    /// </summary>
    Off
  };
}
