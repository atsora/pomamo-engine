// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Reason
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICurrentReasonData
  {
    /// <summary>
    /// Current date/time (now)
    /// </summary>
    DateTime CurrentDateTime { get; }

    /// <summary>
    /// Associated reason
    /// </summary>
    IReason Reason { get; }

    /// <summary>
    /// Associated machine mode
    /// </summary>
    IMachineMode MachineMode { get; }

    /// <summary>
    /// UTC Date/time of the period start if the Period parameter is set
    /// </summary>
    DateTime? PeriodStart { get; }

    /// <summary>
    /// UTC Date/time of the response
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Associated reason score
    /// </summary>
    double? ReasonScore { get; }

    /// <summary>
    /// Reason source
    /// </summary>
    ReasonSource? ReasonSource { get; }

    /// <summary>
    /// Auto-reason number
    /// </summary>
    int? AutoReasonNumber { get; }
  }
}
