// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.PushManufacturingOrder
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for PushManufacturingOrder service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Addon/PushManufacturingOrder/", "GET", Summary = "Service to push a manufacturing order", Notes = "To use with ?MachineId=&ManufacturingOrderId=")]
  [Route ("/Addon/PushManufacturingOrder/Get", "GET", Summary = "Service to push a manufacturing order", Notes = "To use with ?MachineId=&ManufacturingOrderId=")]
  [Route ("/Addon/PushManufacturingOrder/Get/{MachineId}/", "GET", Summary = "Service to push a manufacturing order", Notes = "To use with ?ManufacturingOrderId=")]
  public class PushManufacturingOrderRequestDTO
    : IReturn<PushManufacturingOrderResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    [ApiMember (Name = "Range", Description = "Range. Default is [now,)", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }

    /// <summary>
    /// Task Id
    /// </summary>
    [ApiMember (Name = "TaskId", Description = "Task Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int ManufacturingOrderId { get; set; }

    /// <summary>
    /// Task External code
    /// </summary>
    [ApiMember (Name = "TaskExternalCode", Description = "Task external code", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string ManufacturingOrderExternalCode { get; set; }

    /// <summary>
    /// Task quantity
    /// </summary>
    [ApiMember (Name = "TaskQuantity", Description = "Task quantity", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int ManufacturingOrderQuantity { get; set; }

    /// <summary>
    /// Work order code
    /// </summary>
    [ApiMember (Name = "WorkOrderName", Description = "WorkOrder name", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string WorkorderName { get; set; }

    /// <summary>
    /// Create the manufacturing order if does not exist
    /// </summary>
    [ApiMember (Name = "CreateManufacturingOrder", Description = "Create the manufacturing order if it does not exist. Default: true", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool CreateManufacturingOrder { get; set; }

    /// <summary>
    /// Update the manufacturing order with a work order or machine property
    /// </summary>
    [ApiMember (Name = "UpdateManufacturingOrder", Description = "Update the manufacturing order if it can be completed. Default: true", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool UpdateManufacturingOrder { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public PushManufacturingOrderRequestDTO ()
    {
      this.CreateManufacturingOrder = true;
      this.UpdateManufacturingOrder = true;
    }
  }
}
