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

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("OperationCycle/At Response DTO")]
  public class OperationCycleAtResponseDTO
  {
    /// <summary>
    /// Range. Empty string if no operation cycle was found in this range
    /// </summary>
    public string Range { get; set; } = "";

    /// <summary>
    /// Is the start date/time estimated ?
    /// </summary>
    public bool EstimatedStart { get; set; }

    /// <summary>
    /// Is the end date/time estimated ?
    /// </summary>
    public bool EstimatedEnd { get; set; }

    /// <summary>
    /// List of deliverable pieces
    /// </summary>
    public List<DeliverablePieceDTO> DeliverablePieces { get; set; }
  }
}
