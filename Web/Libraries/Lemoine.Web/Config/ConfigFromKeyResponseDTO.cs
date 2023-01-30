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
  /// Response DTO for ConfigFromKey
  /// </summary>
  [Api("ConfigFromKey Response DTO")]
  public class ConfigFromKeyResponseDTO
  {
    /// <summary>
    /// Config value
    /// </summary>
    public object Value { get; set; }
  }
}
