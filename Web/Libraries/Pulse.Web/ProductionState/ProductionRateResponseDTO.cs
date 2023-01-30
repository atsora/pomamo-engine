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

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Response DTO for ProductionRate
  /// </summary>
  [Api ("ProductionRate Response DTO")]
  public class ProductionRateResponseDTO
  {
    /// <summary>
    /// Day range if applicable
    /// </summary>
    public string DayRange { get; set; }

    /// <summary>
    /// Range if applicable
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Production rate if available
    /// </summary>
    public double? ProductionRate { get; set; }
  }
}
