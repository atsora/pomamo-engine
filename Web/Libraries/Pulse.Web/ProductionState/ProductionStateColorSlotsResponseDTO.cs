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
  /// Response DTO for ProductionStateColorSlots
  /// </summary>
  [Api ("ProductionStateColorSlots Response DTO")]
  public class ProductionStateColorSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ProductionStateColorSlotBlockDTO> Blocks { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Total duration (in s)
    /// </summary>
    public int TotalDuration { get; set; }

    /// <summary>
    /// Total duration when a production rate is set
    /// </summary>
    public int ProductionRateDuration { get; set; }

    /// <summary>
    /// Average production rate on the period when it is defined
    /// <see cref="ProductionRateDuration"/>
    /// 
    /// Set if ProductionRateDuration if &gt; 0
    /// </summary>
    public double? AverageProductionRate { get; set; }
  }

  /// <summary>
  /// Production state color slots: block
  /// </summary>
  public class ProductionStateColorSlotBlockDTO
  {
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Day if applicable
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Main color, without considering the details
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Details
    /// </summary>
    public List<ProductionStateColorSlotBlockDetailDTO> Details { get; set; }
  }

  /// <summary>
  /// Block detail
  /// </summary>
  public class ProductionStateColorSlotBlockDetailDTO
  {
    /// <summary>
    /// Reason color
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Duration in s
    /// </summary>
    public int Duration { get; set; }
  }

}
