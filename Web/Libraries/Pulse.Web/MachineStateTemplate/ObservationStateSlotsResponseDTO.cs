// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
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
  /// Response DTO for ObservationStateSlots
  /// </summary>
  [Api("ObservationStateSlots Response DTO")]
  public class ObservationStateSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ObservationStateSlotDTO> ObservationStateSlots { get; set; }
        
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Observation state slot DTO
  /// </summary>
  public class ObservationStateSlotDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="slot"></param>
    public ObservationStateSlotDTO (IObservationStateSlot slot)
    {
      this.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (null != slot.MachineStateTemplate) {
        this.MachineStateTemplate = new MachineStateTemplateDTOAssembler ().Assemble (slot.MachineStateTemplate);
      }
      if (null != slot.MachineObservationState) {
        this.MachineObservationState = new MachineObservationStateDTOAssembler ().Assemble (slot.MachineObservationState);
      }
      if (null != slot.Shift) {
        this.Shift = new ShiftDTOAssembler ().Assemble (slot.Shift);
      }
    }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display {
      get
      {
        if (null == this.MachineObservationState) {
          return "";
        }
        else { // null != this.MachineObservationState
          if (null == this.Shift) {
            return this.MachineObservationState.Display;
          }
          else { // null != this.Shift
            return string.Format ("{0} ({1})",
                                  this.MachineObservationState.Display,
                                  this.Shift.Display);
          }
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
    
    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor {
      get
      {
        if (null != this.MachineObservationState) {
          return this.MachineObservationState.FgColor;
        }
        else { // null == this.MachineObservationState
          return null;
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor {
      get
      {
        if (null != this.MachineObservationState) {
          return this.MachineObservationState.BgColor;
        }
        else { // null == this.MachineObservationState
          return null;
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// Name of the pattern
    /// 
    /// One of the following:
    /// <item>circles-n with n=1..9</item>
    /// <item>diagonal-stripe-n with n=1..6</item>
    /// <item>dots-n with n=1..9</item>
    /// <item>vertical-stripe-n with n=1..9</item>
    /// <item>crosshatch</item>
    /// <item>houndstooth</item>
    /// <item>lightstripe</item>
    /// <item>smalldot</item>
    /// <item>verticalstripe</item>
    /// <item>whitecarbon</item>
    /// 
    /// See: http://iros.github.io/patternfills/sample_svg.html
    /// </summary>
    public string PatternName {
      get
      {
        if (null != this.Shift) {
          return "circles-5";
        }
        else { // null == this.Shift
          return null;
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// Color of the pattern
    /// </summary>
    public string PatternColor {
      get
      {
        if (null != this.Shift) {
          return this.Shift.Color;
        }
        else { // null == this.Shift
          return null;
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// Associated machine state template
    /// </summary>
    MachineStateTemplateDTO MachineStateTemplate { get; set; }
    
    /// <summary>
    /// Associated machine observation state
    /// </summary>
    MachineObservationStateDTO MachineObservationState { get; set; }
    
    /// <summary>
    /// Associated shift
    /// </summary>
    ShiftDTO Shift { get; set; }
  }
}
