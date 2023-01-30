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

namespace Pulse.Web.Time
{
  /// <summary>
  /// Response DTO for CurrentDay
  /// </summary>
  [Api("CurrentDay Response DTO")]
  public class CurrentDayResponseDTO
  {
    /// <summary>
    /// Current day
    /// </summary>
    public string Day { get; set; }
    
    /// <summary>
    /// Utc date/time range
    /// </summary>
    public string UtcDateTimeRange { get; set; }
    
    /// <summary>
    /// Local date/time range
    /// </summary>
    public string LocalDateTimeRange { get; set; }
  }
}
