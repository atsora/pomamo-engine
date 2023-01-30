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

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Reponse DTO for the /Config/LastUpdate request
  /// </summary>
  [Api("Response DTO for the /Config/LastUpdate request")]
  public class ConfigLastUpdateResponseDTO
  {
    /// <summary>
    /// UTC date/time of the last configuration update
    /// </summary>
    public string UpdateDateTime { get; set; }
  }
}

