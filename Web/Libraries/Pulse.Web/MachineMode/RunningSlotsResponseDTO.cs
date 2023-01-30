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
  /// Response DTO for RunningSlots
  /// </summary>
  [Api("RunningSlots Response DTO")]
  public class RunningSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<RunningSlotBlockDTO> Blocks { get; set; }
    
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

    /// <summary>
    /// Not running duration (in s)
    /// </summary>
    public int NotRunningDuration { get; set; }
  }
  
  /// <summary>
  /// Reason color slots: block
  /// </summary>
  public class RunningSlotBlockDTO
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
    /// Running
    /// </summary>
    public bool Running { get; set; }
    
    /// <summary>
    /// Not running
    /// </summary>
    public bool NotRunning { get; set; }
    
    /// <summary>
    /// Details
    /// </summary>
    public List<RunningSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class RunningSlotBlockDetailDTO
  {
    /// <summary>
    /// Running status
    /// </summary>
    public bool Running { get; set; }
    
    /// <summary>
    /// Not runnin status
    /// </summary>
    public bool NotRunning { get; set; }
    
    /// <summary>
    /// Duration in s
    /// </summary>
    public int Duration { get; set; }
  }

}
