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
using Lemoine.Extensions.Web.Responses;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("OperationCycle/Slots Response DTO")]
  public class OperationCycleSlotsResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<OperationCycleSlotBlockDTO> Blocks { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }

  /// <summary>
  /// Operation cycle slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the operation cycle ID
  /// </summary>
  public class OperationCycleSlotBlockDTO : SlotDTO
  {
    /// <summary>
    /// Details
    /// </summary>
    public List<OperationCycleSlotBlockDetailDTO> Details { get; set; }
  }

  /// <summary>
  /// Block detail
  /// </summary>
  public class OperationCycleSlotBlockDetailDTO
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
