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
  /// Response DTO for ReasonOverwriteRequiredSlots
  /// </summary>
  [Api ("ReasonOverwriteRequiredSlots Response DTO")]
  public class ReasonOverwriteRequiredSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ReasonOverwriteRequiredSlotDTO> ReasonOverwriteRequiredSlots { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }

  /// <summary>
  /// Reason overwrite required slot DTO
  /// </summary>
  public class ReasonOverwriteRequiredSlotDTO : SlotDTO
  {
    /// <summary>
    /// Reason color
    /// 
    /// transparent may be returned
    /// </summary>
    public string Color { get; set; }
  }
}
