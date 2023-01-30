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

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Response DTO for IsoFileSlots
  /// </summary>
  [Api("IsoFileSlots Response DTO")]
  public class IsoFileSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<IsoFileSlotBlockDTO> IsoFileSlots { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Iso file slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the operation ID
  /// </summary>
  public class IsoFileSlotBlockDTO: SlotDTO
  {
    /// <summary>
    /// Details
    /// </summary>
    public List<IsoFileSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class IsoFileSlotBlockDetailDTO
  {
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
  }
}
