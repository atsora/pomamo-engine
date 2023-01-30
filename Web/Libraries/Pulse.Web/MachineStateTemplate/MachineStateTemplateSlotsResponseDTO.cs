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

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Response DTO for MachineStateTemplateSlots
  /// </summary>
  [Api("MachineStateTemplateSlots Response DTO")]
  public class MachineStateTemplateSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<MachineStateTemplateSlotDTO> MachineStateTemplateSlots { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Override of SlotDTO to include the MachineStateTemplateSlot category
  /// </summary>
  public class MachineStateTemplateSlotDTO: SlotDTO
  {
    /// <summary>
    /// Machine state template slot category (1: Production / 2: SetUp)
    /// </summary>
    public int? Category { get; set; }
  }
}
