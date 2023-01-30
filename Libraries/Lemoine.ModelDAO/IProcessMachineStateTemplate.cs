// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for ProcessMachineStateTemplate modification
  /// </summary>
  public interface IProcessMachineStateTemplate: IMachineModification
  {
    /// <summary>
    /// UTC date/time range
    /// </summary>
    UtcDateTimeRange Range { get; }    
  }
}
