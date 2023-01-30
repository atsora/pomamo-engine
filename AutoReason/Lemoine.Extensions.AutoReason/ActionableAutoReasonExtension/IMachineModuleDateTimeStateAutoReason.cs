// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// Auto-reason with a date/time state per machine module
  /// </summary>
  public interface IMachineModuleDateTimeStateAutoReason: IAutoReasonExtension
  {
    /// <summary>
    /// Get the date/time for a specific machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    DateTime GetDateTime (IMachineModule machineModule);

    /// <summary>
    /// Update the date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    void UpdateDateTime (IMachineModule machineModule, DateTime dateTime);

    /// <summary>
    /// Reset date/time (in case of a failure)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    void ResetDateTime (IMachineModule machineModule, DateTime dateTime);
  }
}
