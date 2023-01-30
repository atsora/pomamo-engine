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

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Response DTO for Utilization
  /// </summary>
  [Api("Utilization Response DTO")]
  public class UtilizationResponseDTO
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
    /// Total duration (in s)
    /// </summary>
    public int TotalDuration { get; set; }
    
    /// <summary>
    /// Motion duration (in s)
    /// </summary>
    public int MotionDuration { get; set; }

    /// <summary>
    /// Not running duration (in s)
    /// </summary>
    public int NotRunningDuration { get; set; }
  }

}
