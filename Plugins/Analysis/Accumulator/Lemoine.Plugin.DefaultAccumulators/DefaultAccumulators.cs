// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.DefaultAccumulators
{
  /// <summary>
  /// Default management of reasons
  /// </summary>
  public static class DefaultAccumulators
  {
    /// <summary>
    /// Install the plugin
    /// </summary>
    public static void Install ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (MachineActivitySummaryAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ProductionAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ReasonSummaryAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ShiftByMachineAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ProductionStateSummaryAccumulatorExtension));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ProductionRateSummaryAccumulatorExtension));
    }
  }
}
