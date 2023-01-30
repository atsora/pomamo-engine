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
using Pulse.Extensions.Web.Responses;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Extensions.Web.Responses;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Reponse DTO for the UtilizationTarget service
  /// </summary>
  [Api("UtilizationTarget Response DTO")]
  public class UtilizationTargetResponseDTO
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    public MachineDTO Machine { get; set; }
    
    /// <summary>
    /// Target utilization percentage for machine (between 0.0 and 1.0)
    /// </summary>
    public double TargetPercentage { get; set; }
  }
}

