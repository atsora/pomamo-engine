// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Connection status
  /// </summary>
  public enum ConnectionStatus
  {
    /// <summary>
    /// Not connected yet
    /// </summary>
    Stopped = 0,
    /// <summary>
    /// Connecting
    /// </summary>
    Starting = 1,
    /// <summary>
    /// The connection is up and running
    /// </summary>
    Started = 2,
    /// <summary>
    /// Error
    /// </summary>
    Error = 4,
  }
}
