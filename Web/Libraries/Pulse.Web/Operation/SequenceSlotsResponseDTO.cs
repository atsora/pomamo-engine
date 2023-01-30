// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
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
  /// Response DTO for SequenceSlots
  /// </summary>
  [Api("SequenceSlots Response DTO")]
  public class SequenceSlotsResponseDTO
  {
    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<SequenceSlotsByMachineModuleDTO> ByMachineModule { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Data by machine module
  /// </summary>
  public class SequenceSlotsByMachineModuleDTO
  {
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }
    
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<SequenceSlotBlockDTO> Blocks { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="isMain"></param>
    internal SequenceSlotsByMachineModuleDTO (IMachineModule machineModule, bool isMain)
    {
      Debug.Assert (null != machineModule);
      
      this.MachineModule = new MachineModuleDTO ();
      this.MachineModule.Id = machineModule.Id;
      this.MachineModule.Display = machineModule.Display;
      this.MachineModule.Main = isMain;
      this.Blocks = new List<SequenceSlotBlockDTO> ();
    }    
  }
  
  /// <summary>
  /// Sequence slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the sequence ID
  /// </summary>
  public class SequenceSlotBlockDTO: SlotDTO
  {
    /// <summary>
    /// Details
    /// </summary>
    public List<SequenceSlotBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class SequenceSlotBlockDetailDTO
  {
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Tool that is associated to the sequence
    /// </summary>
    public string ToolNumber { get; set; }
  }

}
