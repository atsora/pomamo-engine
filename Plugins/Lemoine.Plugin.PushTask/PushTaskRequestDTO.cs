// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.PushTask
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for PushTask service")]
  [ApiResponse (System.Net.HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Addon/PushTask/", "GET", Summary = "Service to push a task", Notes = "To use with ?MachineId=&TaskId=")]
  [Route ("/Addon/PushTask/Get", "GET", Summary = "Service to push a task", Notes = "To use with ?MachineId=&TaskId=")]
  [Route ("/Addon/PushTask/Get/{MachineId}/", "GET", Summary = "Service to push a task", Notes = "To use with ?TaskId=")]
#if NSERVCEKIT
  [NServiceKit.ServiceHost.Route ("/Addon/PushTask/", "GET", Summary = "Service to push a task", Notes = "To use with ?MachineId=&TaskId=")]
  [NServiceKit.ServiceHost.Route ("/Addon/PushTask/Get", "GET", Summary = "Service to push a task", Notes = "To use with ?MachineId=&TaskId=")]
  [NServiceKit.ServiceHost.Route ("/Addon/PushTask/Get/{MachineId}/", "GET", Summary = "Service to push a task", Notes = "To use with ?TaskId=")]
#endif // NSERVICEKIT
  public class PushTaskRequestDTO
    : IReturn<PushTaskResponseDTO>
#if NSERVICEKIT
    , NServiceKit.ServiceHost.IReturn<PushTaskResponseDTO>
#endif // NSERVICEKIT
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
    public int TaskId { get; set; }

    /// <summary>
    /// Task External code
    /// </summary>
    [ApiMember (Name = "TaskExternalCode", Description = "Task external code", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string TaskExternalCode { get; set; }

    /// <summary>
    /// Task quantity
    /// </summary>
    [ApiMember (Name = "TaskQuantity", Description = "Task quantity", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int TaskQuantity { get; set; }

    /// <summary>
    /// Work order code
    /// </summary>
    [ApiMember (Name = "WorkOrderName", Description = "WorkOrder name", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string WorkorderName { get; set; }

    /// <summary>
    /// Create the task if does not exist
    /// </summary>
    [ApiMember (Name = "CreateTask", Description = "Create the task if it does not exist. Default: true", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool CreateTask { get; set; }

    /// <summary>
    /// Update the task with a work order or machine property
    /// </summary>
    [ApiMember (Name = "UpdateTask", Description = "Update the task if it can be completed. Default: true", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool UpdateTask { get; set; }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    public PushTaskRequestDTO ()
    {
      this.CreateTask = true;
      this.UpdateTask = true;
    }
  }
}
