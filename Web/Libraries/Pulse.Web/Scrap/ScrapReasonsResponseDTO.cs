// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Lemoine.Extensions.Web.Attributes;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Response DTO for Scrap/Reasons
  /// </summary>
  [Api ("Scrap/Reasons Response DTO")]
  public class ScrapReasonsResponseDTO
  {
    /// <summary>
    /// Non conformance reasons, sorted by display priority, then by display
    /// </summary>
    public IList<NonConformanceReasonDTO> Reasons { get; set; } = new List<NonConformanceReasonDTO> ();
  }
}
