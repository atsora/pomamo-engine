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
  /// Reponse DTO for the CurrentMachineMode service
  /// </summary>
  [Api ("CurrentMachineMode Response DTO")]
  public class CurrentMachineModeResponseDTO
  {
    /// <summary>
    /// Current date/time (now)
    /// </summary>
    public string CurrentDateTime { get; set; }

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
    /// If true, this is a manual activity
    /// </summary>
    public bool? ManualActivity { get; set; }
  }
}

