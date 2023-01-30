// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Collections;
using Pulse.Extensions.Web.Responses;
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
  /// Response DTO for ComponentSlots
  /// </summary>
  [Api ("ComponentSlots Response DTO")]
  public class ComponentSlotsResponseDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ComponentSlotsResponseDTO ()
    {
      this.Components = new List<ComponentSlotsDTO> ();
    }

    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<ComponentSlotsDTO> Components { get; set; }

    /// <summary>
    /// Requested range
    /// </summary>
    public string RequestedRange { get; set; }

    /// <summary>
    /// Range that applies (when there is a component)
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Range without any infinite bounds and limited to now
    /// </summary>
    public string LimitedRange { get; set; }

    /// <summary>
    /// WorkOrder Id
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Project Id
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Project display
    /// </summary>
    public string ProjectDisplay { get; set; }
  }

  /// <summary>
  /// Group here the slot blocks for a specific component
  /// </summary>
  public class ComponentSlotsDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="component">not null</param>
    public ComponentSlotsDTO (Lemoine.Model.IComponent component)
    {
      this.Id = ((IDataWithId)component).Id;
      this.Display = component.Display;
      this.Blocks = new List<ComponentSlotBlockDTO> ();
    }

    /// <summary>
    /// Component Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Component display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor { get; set; }

    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor { get; set; }

    /// <summary>
    /// List of slot blocks for this specific component
    /// </summary>
    public List<ComponentSlotBlockDTO> Blocks { get; set; }

    /// <summary>
    /// Impacted machines
    /// </summary>
    public List<MachineDTO> Machines { get; set; }

    /// <summary>
    /// List of machine Ids separated by a comma
    /// </summary>
    public string MachineIds { get; set; }
  }

  /// <summary>
  /// Component slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the component ID
  /// </summary>
  public class ComponentSlotBlockDTO
  {
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="range"></param>
    public ComponentSlotBlockDTO (string range)
    {
      this.Range = range;
      this.Machines = new List<MachineDTO> ();
    }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Impacted machines
    /// </summary>
    public List<MachineDTO> Machines { get; set; }

    /// <summary>
    /// List of machine Ids separated by a comma
    /// </summary>
    public string MachineIds { get; set; }
  }
}
