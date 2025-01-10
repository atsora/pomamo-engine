// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Response DTO for ReasonOnlySlots
  /// </summary>
  [Api("ReasonOnlySlots Response DTO")]
  public class ReasonOnlySlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ReasonOnlySlotDTO> ReasonOnlySlots { get; set; }
    
    /// <summary>
    /// Range: only set if RangeNumber=1
    /// </summary>
    public string Range { get; set; }
    
    /// <summary>
    /// Number of ranges
    /// </summary>
    public int RangeNumber { get; set; }
  }
  
  /// <summary>
  /// Override of SlotDTO to add the OverwriteRequired property of a reason slot
  /// </summary>
  public class ReasonOnlySlotDTO: SlotDTO
  {
    /// <summary>
    /// Long display: reason with its description
    /// </summary>
    public string LongDisplay { get; set; }

    /// <summary>
    /// Description of the reason
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Running property
    /// </summary>
    public bool Running { get; set; }

    /// <summary>
    /// Reason score
    /// 
    /// If the SelectionOption is on, it will be only the score
    /// of the reason slot at the specified date/time,
    /// not on the full returned range
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Reason source
    /// 
    /// If the SelectionOption is on, it will be only the source
    /// of the reason slot at the specified date/time,
    /// not on the full returned range
    /// </summary>
    public ReasonSourceDTO Source { get; set; }

    /// <summary>
    /// Auto-reason number
    /// 
    /// If the SelectionOption is on, it will be only the auto-reason number
    /// of the reason slot at the specified date/time,
    /// not on the full returned range
    /// </summary>
    public int? AutoReasonNumber { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    public string Details { get; set; }
    
    /// <summary>
    /// Overwrite required property
    /// </summary>
    public bool OverwriteRequired { get; set; }
    
    /// <summary>
    /// Is it a default reason ?
    /// </summary>
    public bool DefaultReason { get; set; }
    
    /// <summary>
    /// true if it may be considered as the current slot
    /// </summary>
    public bool? Current { get; set; }
    
    /// <summary>
    /// Sub-slots for the machine modes
    /// </summary>
    public List<ReasonOnlyMachineModeSubSlotDTO> MachineModes { get; set; }
    
    /// <summary>
    /// Is there at least a reason selection for this ReasonOnlySlot ?
    /// 
    /// Option: SelectableOption must be true so that a data is returned here
    /// </summary>
    public bool? IsSelectable { get; set; }
    
    /// <summary>
    /// The lower limit was reached, meaning the effective lower date/time of the range
    /// is lower than the returned one (it starts before the returned date/time)
    /// </summary>
    public bool? LowerLimitReached { get; set; }
    
    /// <summary>
    /// The upper limit was reached, meaning the effective upper date/time of the range
    /// is upper than the returned one (it ends after the returned date/time)
    /// </summary>
    public bool? UpperLimitReached { get; set; }
  }
  
  /// <summary>
  /// Machine mode sub-slot dtos
  /// </summary>
  public class ReasonOnlyMachineModeSubSlotDTO: SlotDTO
  {
    /// <summary>
    /// Machine mode category
    /// </summary>
    public MachineModeCategoryDTO Category { get; set; }
  }
}
