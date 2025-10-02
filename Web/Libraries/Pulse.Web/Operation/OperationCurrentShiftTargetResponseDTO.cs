// Copyright (C) 2025 Atsora Solutions
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
  [Api ("/Operation/OperationCurrentShiftTarget/ Response DTO")]
  public class OperationCurrentShiftTargetResponseDTO
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
    /// Considered UTC date/time range: from operation start to shift end
    /// </summary>
    public string Range { get; set; }

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
  }
}
