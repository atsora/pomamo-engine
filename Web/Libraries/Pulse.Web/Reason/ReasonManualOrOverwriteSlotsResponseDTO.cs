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
  /// Response DTO for ReasonManualOrOverwriteSlots
  /// </summary>
  [Api ("ReasonManualOrOverwriteSlots Response DTO")]
  public class ReasonManualOrOverwriteSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ReasonManualOrOverwriteSlotDTO> ReasonManualOrOverwriteSlots { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }

  /// <summary>
  /// Reason manual or overwrite slot DTO
  /// </summary>
  public class ReasonManualOrOverwriteSlotDTO : SlotDTO
  {
    /// <summary>
    /// Overwrite required property
    /// </summary>
    public bool OverwriteRequired { get; set; }

    /// <summary>
    /// true if it may be considered as the current slot
    /// </summary>
    public bool? Current { get; set; }
  }
}
