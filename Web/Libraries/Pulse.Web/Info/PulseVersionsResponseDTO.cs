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

namespace Pulse.Web.Info
{
  /// <summary>
  /// Reponse DTO for the PulseVersions service
  /// </summary>
  [Api("PulseVersions Response DTO")]
  public class PulseVersionsResponseDTO
  {
    /// <summary>
    /// Set of versions
    /// </summary>
    public Dictionary<string, string> Versions { get; set; }
  }
}

