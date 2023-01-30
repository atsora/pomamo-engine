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

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Response DTO for MachineModeColorSlots
  /// </summary>
  [Api("MachineModeColorSlots Response DTO")]
  public class MachineModeColorSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<MachineModeColorSlotBlockDTO> Blocks { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
    
    /// <summary>
    /// Total duration (in s)
    /// </summary>
    public int TotalDuration { get; set; }
    
    /// <summary>
    /// Motion duration (in s)
    /// </summary>
    public int MotionDuration { get; set; }
  }
  
  /// <summary>
  /// MachineMode color slots: block
  /// </summary>
  public class MachineModeColorSlotBlockDTO
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
    /// Main machine mode color, without considering the details
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Details
    /// </summary>
    public List<MachineModeColorSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class MachineModeColorSlotBlockDetailDTO
  {
    /// <summary>
    /// MachineMode color
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Duration in s
    /// </summary>
    public int Duration { get; set; }
  }

}
