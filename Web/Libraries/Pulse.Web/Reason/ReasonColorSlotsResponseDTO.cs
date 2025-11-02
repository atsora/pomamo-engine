// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Response DTO for ReasonColorSlots
  /// </summary>
  [Api("ReasonColorSlots Response DTO")]
  public class ReasonColorSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ReasonColorSlotBlockDTO> Blocks { get; set; }
    
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

    /// <summary>
    /// One of the period is a processing period
    /// </summary>
    public bool Processing { get; set; }
  }
  
  /// <summary>
  /// Reason color slots: block
  /// </summary>
  public class ReasonColorSlotBlockDTO
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
    /// This block contains at least one processing period
    /// </summary>
    public bool Processing { get; set; }

    /// <summary>
    /// Main color, without considering the details
    /// 
    /// transparent may be returned
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// One of the details has 'overwrite required' set
    /// </summary>
    public bool OverwriteRequired { get; set; }

    /// <summary>
    /// Auto-reason
    /// </summary>
    public bool Auto { get; set; }

    /// <summary>
    /// Details
    /// </summary>
    public List<ReasonColorSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class ReasonColorSlotBlockDetailDTO
  {
    /// <summary>
    /// Processing period
    /// </summary>
    public bool Processing { get; set; }

    /// <summary>
    /// Reason color
    /// 
    /// transparent may be returned
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Overwrite required property
    /// </summary>
    public bool OverwriteRequired { get; set; }

    /// <summary>
    /// Auto-reason
    /// </summary>
    public bool Auto { get; set; }

    /// <summary>
    /// Duration in s
    /// </summary>
    public int Duration { get; set; }
  }

}
