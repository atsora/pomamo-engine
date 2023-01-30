// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Response DTO for ProductionStateSlots
  /// </summary>
  [Api ("ProductionStateSlots Response DTO")]
  public class ProductionStateSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ProductionStateSlotDTO> ProductionStateSlots { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }

  /// <summary>
  /// Override of SlotDTO to include additional properties
  /// </summary>
  public class ProductionStateSlotDTO : SlotDTO
  {
  }
}
