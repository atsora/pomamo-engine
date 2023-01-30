// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Response DTO for PastRange web service
  /// </summary>
  class PastRangeResponseDTO
  {
    /// <summary>
    /// UTC date/time range
    /// </summary>
    public string UtcDateTimeRange { get; set; }

    /// <summary>
    /// Local date/time range
    /// </summary>
    public string LocalDateTimeRange { get; set; }

    /// <summary>
    /// Range in system day
    /// </summary>
    public string DayRange { get; set; }
  }
}
