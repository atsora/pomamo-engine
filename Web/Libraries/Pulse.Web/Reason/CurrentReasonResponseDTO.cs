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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Reponse DTO for the CurrentReason service
  /// </summary>
  [Api("CurrentReason Response DTO")]
  public class CurrentReasonResponseDTO
  {
    /// <summary>
    /// Current date/time (now)
    /// </summary>
    public string CurrentDateTime { get; set; }

    /// <summary>
    /// Associated reason
    /// </summary>
    public ReasonDTO Reason { get; set; }

    /// <summary>
    /// Associated machine mode
    /// </summary>
    public MachineModeDTO MachineMode { get; set; }
    
    /// <summary>
    /// UTC Date/time of the period start if the Period parameter is set
    /// </summary>
    public string PeriodStart { get; set; }

    /// <summary>
    /// UTC Date/time of the response
    /// </summary>
    public string DateTime { get; set; }

    /// <summary>
    /// Associated reason score
    /// </summary>
    public double? ReasonScore { get; set; }

    /// <summary>
    /// Reason source
    /// </summary>
    public ReasonSourceDTO ReasonSource { get; set; }

    /// <summary>
    /// Auto-reason number
    /// </summary>
    public int? AutoReasonNumber { get; set; }

    /// <summary>
    /// Severity in case the machine is idle for a long time for example
    /// 
    /// This is triggered by a plugin
    /// </summary>
    public SeverityDTO Severity { get; set; }
  }
}

