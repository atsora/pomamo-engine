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
using Lemoine.Extensions.Web.Responses;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("/Operation/ReserveCapacityCurrentShift/ Response DTO")]
  public class ReserveCapacityCurrentShiftResponseDTO
  {
    /// <summary>
    /// Date/time of the data
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Current operation (long display version)
    /// </summary>
    public OperationDTO Operation { get; set; }

    /// <summary>
    /// Current component (nullable)
    /// </summary>
    public ComponentDTO Component { get; set; }

    /// <summary>
    /// UTC date/time range of the effective operation
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Number of pieces done during the current shift
    /// </summary>
    public double? NbPieceCurrentShift { get; set; }

    /// <summary>
    /// Number of pieces that should have been done during the current shift
    /// </summary>
    public double? GoalNowShift { get; set; }

    /// <summary>
    /// Day in ISO string
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Shift
    /// </summary>
    public ShiftDTO Shift { get; set; }

    /// <summary>
    /// Goal for the whole shift (until the end of the shift)
    /// </summary>
    public double? ShiftGoal { get; set; }

    /// <summary>
    /// Remaining capacity until the end of the shift
    /// </summary>
    public double? RemainingCapacity { get; set; }

    /// <summary>
    /// Reserve capacity
    /// 
    /// RemainingCapacity - (ShiftGoal - NbPiecesDoneDuringShift)
    /// </summary>
    public double? ReserveCapacity { get; set; }

    /// <summary>
    /// Deprecated reserve capacity
    /// 
    /// To remove in version 11
    /// </summary>
    public double? ReservedCapacity
    {
      get { return this.ReserveCapacity; }
      set { this.ReserveCapacity = value; }
    }
  }
}
