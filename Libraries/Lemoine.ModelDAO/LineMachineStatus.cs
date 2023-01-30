// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Line/machine status
  /// </summary>
  public enum LineMachineStatus {
    /// <summary>
    /// Dedicated to the line
    /// </summary>
    Dedicated = 1,
    /// <summary>
    /// Extra machine (in partial time or to be used in case of production delay)
    /// </summary>
    Extra = 2
  };
}
