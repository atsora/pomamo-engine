// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("Pie Response DTO")]
  public class PieResponseDTO
  {
    /// <summary>
    /// Pie type:
    /// <item>cycleprogresspie</item>
    /// <item>partproductionstatuspie</item>
    /// <item>reasonslotpie</item>
    /// <item>runningslotpie</item>
    /// <item>operationprogresspie</item>.
    /// 
    /// If an empty string is returned, no pie must be displayed
    /// </summary>
    public string PieType { get; set; }

    /// <summary>
    /// The pie is permanent, the pie type won't never change (no need to request the service regularly)
    /// </summary>
    public bool Permanent { get; set; }
  }
}
