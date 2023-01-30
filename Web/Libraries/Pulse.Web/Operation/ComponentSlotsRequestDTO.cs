// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/ComponentSlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/ComponentSlots/", "GET", Summary = "Get the component slots in a specified range", Notes = "To use with ?WorkOrderId=&Range=&MachineIds=")]
  [Route ("/Operation/ComponentSlots/Get/{WorkOrderId}/{Range}", "GET", Summary = "Get the component slots in a specified range", Notes = "")]
  public class ComponentSlotsRequestDTO : IReturn<ComponentSlotsResponseDTO>
  {
    /// <summary>
    /// Work order Id
    /// 
    /// One of the two parameters is required: WorkOrderId or ProjectId
    /// </summary>
    [ApiMember (Name = "WorkOrderId", Description = "Work order ID of the job", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Project Id
    /// 
    /// One of the two parameters is required: WorkOrderId or ProjectId
    /// </summary>
    [ApiMember (Name = "ProjectId", Description = "Project ID of the job", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int ProjectId { get; set; }

    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that corresponds to Today
    /// 
    /// This is recommended it to set it for some performance reasons, although this is not absolutely required
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }

    /// <summary>
    /// List of machine ids (separated by a comma ',')
    /// 
    /// Default: an empty list that corresponds to all machines
    /// 
    /// This is recommended it to set the machine ids for some performance reasons, although this is not absolutely required
    /// </summary>
    [ApiMember (Name = "MachineIds", Description = "List of machine ids separated by ','. An empty list corresponds to all machines", ParameterType = "path", DataType = "List(int)", IsRequired = false)]
    public IList<int> MachineIds { get; set; }
  }
}
