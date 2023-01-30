// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Default management of reasons
  /// </summary>
  public static class ReasonDefaultManagement
  {
    /// <summary>
    /// Install the plugin
    /// </summary>
    public static void Install ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (DefaultReasonUndefined));
      Lemoine.Extensions.ExtensionManager.Add (typeof (DefaultReasonWithDurationConfig.DefaultReasonWithDuration));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ReasonModificationAuto));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ReasonModificationManual));
      Lemoine.Extensions.ExtensionManager.Add (typeof (ReasonSelectionTable));
    }
  }
}
