// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Reponse DTO for the ReasonUnanswered service
  /// </summary>
  [Api("ReasonUnanswered Response DTO")]
  public class ReasonUnansweredResponseDTO
  {
    /// <summary>
    /// Is there an unsanwered period in the specific range ? 
    /// </summary>
    public bool IsUnansweredPeriod { get; set; }

    /// <summary>
    /// If the Number parameter is set to true in the request,
    /// this field contains the number of non-consecutive unanswered periods in the specific range.
    /// Otherwise, it is set to -1. 
    /// </summary>
    public int UnansweredPeriodsNumber { get; set; } = -1;

    /// <summary>
    /// If the Number parameter is set to true in the request,
    /// this field contains the number of unanswered periods (including it they are consecutive)
    /// in the specific range.
    /// Otherwise, it is set to null. 
    /// </summary>
    public int? NumberIncludingConsecutive { get; set; } = null;
  }
}

