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
  /// Response DTO for CurrentTime
  /// </summary>
  [Api("CurrentTime Response DTO")]
  public class CurrentTimeResponseDTO
  {
    /// <summary>
    /// Utc current time
    /// </summary>
    public string Utc { get; set; }
    
    /// <summary>
    /// Local current time
    /// </summary>
    public string Local { get; set; }
  }
}
