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
  /// Response DTO
  /// </summary>
  [Api ("Operation/ProductionMachiningStatus Response DTO")]
  public class ProductionMachiningStatusResponseDTO
  {
    /// <summary>
    /// ProductionMachiningStatus is not available for this specific group.
    /// 
    /// events may be returned
    /// </summary>
    public bool EventsOnly { get; set; }

    /// <summary>
    /// Current operation (long display version)
    /// </summary>
    public OperationDTO Operation { get; set; }

    /// <summary>
    /// Current component (nullable)
    /// </summary>
    public ComponentDTO Component { get; set; }

    /// <summary>
    /// Current work order (nullable)
    /// </summary>
    public WorkOrderDTO WorkOrder { get; set; }

    /// <summary>
    /// Current task (nullable)
    /// </summary>
    public TaskDTO Task { get; set; }

    /// <summary>
    /// UTC date/time range of the effective operation
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// [Deprecated] Work piece informations
    /// 
    /// To keep the compatibility with some existing web components
    /// </summary>
    public List<WorkInformationDTO> WorkInformations { get; set; }

    /// <summary>
    /// Number of pieces done during the whole task
    /// </summary>
    public double? NbPiecesDoneGlobal { get; set; }

    /// <summary>
    /// Number of pieces done during the current shift
    /// </summary>
    public double? NbPiecesDoneDuringShift { get; set; }

    /// <summary>
    /// Number of pieces that should have been done during the whole task
    /// </summary>
    public double? GoalNowGlobal { get; set; }

    /// <summary>
    /// Number of pieces that should have been done during the current shift
    /// </summary>
    public double? GoalNowShift { get; set; }

    /// <summary>
    /// Day in ISO string
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Shift
    /// </summary>
    public ShiftDTO Shift { get; set; }

    /// <summary>
    /// Coming events (ordered by ascending estimated date/time)
    /// 
    /// For the moment, only available for a single machine
    /// </summary>
    public List<EventDTO> ComingEvents { get; set; }

    /// <summary>
    /// Current active events.
    /// 
    /// Set by order of importance, more important active event first
    /// 
    /// For the moment, only available for a single machine
    /// </summary>
    public List<EventDTO> ActiveEvents { get; set; }

    /// <summary>
    /// Current machine activity in case a coming event is returned
    /// (and no active event is returned)
    /// </summary>
    public bool? Running { get; set; }
  }

  /// <summary>
  /// Various kind of WorkInformations.
  /// </summary>
  public enum WorkInformationKind
  {
    /// <summary>
    /// WorkOrder kind
    /// </summary>
    WorkOrder,

    /// <summary>
    /// Component kind
    /// </summary>
    Component,

    /// <summary>
    /// Operation kind
    /// </summary>
    Operation,
  }

  /// <summary>
  /// [Deprecated] DTO for Work Information
  /// </summary>
  public class WorkInformationDTO
  {
    /// <summary>
    /// Kind
    /// </summary>
    public WorkInformationKind Kind { get; set; }

    /// <summary>
    /// Value (may be null)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public WorkInformationDTO ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="value"></param>
    public WorkInformationDTO (WorkInformationKind kind, string value)
    {
      this.Kind = kind;
      this.Value = value;
    }
  }
}
