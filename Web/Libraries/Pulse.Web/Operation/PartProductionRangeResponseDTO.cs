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

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("Operation/PartProductionRange Response DTO")]
  public class PartProductionRangeResponseDTO
  {
    /// <summary>
    /// Requested range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Number of parts
    /// </summary>
    public double NbPieces { get; set; }

    /// <summary>
    /// Target (nullable)
    /// 
    /// null if no standard duration is defined for the operation
    /// </summary>
    public double? Goal { get; set; }

    /// <summary>
    /// Is the data in progress ?
    /// </summary>
    public bool InProgress { get; set; }
  }
}
