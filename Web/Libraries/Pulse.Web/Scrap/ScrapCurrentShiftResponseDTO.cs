// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Response DTO for Scrap/CurrentShift
  ///
  /// Sum of the scrap counters of the operation slots of the current day and shift
  /// (the shift is not considered when the operation slots are not split by shift)
  /// </summary>
  [Api ("Scrap/CurrentShift Response DTO")]
  public class ScrapCurrentShiftResponseDTO
  {
    /// <summary>
    /// UTC date/time that was considered to determine the current day and shift
    /// </summary>
    public string DateTime { get; set; }

    /// <summary>
    /// Current day in ISO format (omitted if unknown)
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Current shift (omitted if none)
    /// </summary>
    public ShiftDTO Shift { get; set; }

    /// <summary>
    /// Number of cycles covered by the scrap reports
    /// </summary>
    public int ReportedCycleCount { get; set; }

    /// <summary>
    /// Number of parts covered by the scrap reports
    /// </summary>
    public int ReportedCount { get; set; }

    /// <summary>
    /// Number of valid parts
    /// </summary>
    public int ValidCount { get; set; }

    /// <summary>
    /// Number of parts of the set-up
    /// </summary>
    public int SetupCount { get; set; }

    /// <summary>
    /// Number of scrap parts
    /// </summary>
    public int ScrapCount { get; set; }

    /// <summary>
    /// Number of parts that can be fixed
    /// </summary>
    public int FixableCount { get; set; }
  }
}
