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

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO for OperationSlots
  /// </summary>
  [Api("OperationSlots Response DTO")]
  public class OperationSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<OperationSlotBlockDTO> Blocks { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Operation slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the operation ID
  /// </summary>
  public class OperationSlotBlockDTO: SlotDTO
  {
    // TODO: more on the operation slots, like:
    // - total cycles
    // - number of parts
    // - ...
    // To do later
    
    /// <summary>
    /// Details
    /// </summary>
    public List<OperationSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class OperationSlotBlockDetailDTO
  {
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
    
    // TODO: Work information details. To complete later
  }

}
