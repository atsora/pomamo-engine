// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.Plugin.ProductionTracker
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("ProductionTracker Response DTO")]
  public class ProductionTrackerResponseDTO
  {
    /// <summary>
    /// Data per hour
    /// </summary>
    public List<ProductionTrackerResponsePerHourDTO> HourlyData { get; set; } = new List<ProductionTrackerResponsePerHourDTO> ();

    /// <summary>
    /// Requested range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Global target for the requested range
    /// 
    /// Only if requested and if a unique operation is used on the whole range
    /// </summary>
    public double? GlobalTarget { get; set; }

    /// <summary>
    /// Production capacity for the whole range
    /// 
    /// Only if requested and if a unique operation ise used on the whole range
    /// </summary>
    public double? ProductionCapacity { get; set; }
  }

  public class ProductionTrackerResponsePerHourDTO
  {
    /// <summary>
    /// Local hour
    /// </summary>
    public string LocalHour { get; set; }

    /// <summary>
    /// Date/time range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Number of parts
    /// </summary>
    public double Actual { get; set; }

    /// <summary>
    /// Target (nullable)
    /// 
    /// null if no standard duration is defined for the operation
    /// </summary>
    public double? Target { get; set; }

    /// <summary>
    /// Is the data static ? (in the past)
    /// </summary>
    public bool Static { get; set; }

    /// <summary>
    /// Production capacity for the local range
    /// 
    /// Only set if applicable and if the RemainingCapacity of the request is on
    /// </summary>
    public double? ProductionCapacity { get; set; }
  }
}
