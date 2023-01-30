// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Interface for the plugins to update the default reason
  /// </summary>
  public interface IReasonSelectionExtension : IInitializedByMachineExtension
  {
    /// <summary>
    /// Try to get additional reason selections at the specified date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="includeExtraAutoReasons">include in the reason selections the extra auto-reasons, else only consider the manual reasons</param>
    /// <returns></returns>
    IEnumerable<IReasonSelection> GetReasonSelections (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons);
  }
}
