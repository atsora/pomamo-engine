// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Available options for the Production machining status applications 
  /// </summary>
  public enum ProductionMachiningOption
  {
    /// <summary>
    /// No task or work order is considered, only the operation is considered (default)
    /// </summary>
    TrackOperation = 0,
    /// <summary>
    /// Consider the task for the period to track
    /// </summary>
    TrackTask = 1,
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/ProductionMachiningStatus service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/ProductionMachiningStatus/", "GET", Summary = "", Notes = "To use with ?MachineId= or ?GroupId= or ?MachineId=xxx&Option=TrackTask")]
  [Route ("/Operation/ProductionMachiningStatus/Get/{GroupId}", "GET", Summary = "", Notes = "")]
  [Route ("/GetProductionMachiningStatus/", "GET", Summary = "Default route to be used in combination with ?MachineId=xxx or ?GroupId=xxx or ?Machine=Id=xxx&Option=TrackTask")]
  [Route ("/GetProductionMachiningStatus/{GroupId}", "GET", Summary = "Default request, without considering the work order")]
  [Route ("/GetProductionMachiningStatus/{GroupId}/{Option}", "GET", Summary = "Request with the option to consider or not the work order")]
  public class ProductionMachiningStatusRequestDTO : IReturn<ProductionMachiningStatusResponseDTO>
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineId { get; set; }

    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string GroupId { get; set; }

    /// <summary>
    /// Option
    /// </summary>
    [ApiMember (Name = "Option", Description = "Option to consider or not the work order or the task", ParameterType = "path", DataType = "string", IsRequired = false)]
    public ProductionMachiningOption Option { get; set; }

    /// <summary>
    /// Include the events in the response (default is false)
    /// </summary>
    [ApiMember (Name = "IncludeEvents", Description = "Include the events in the response", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeEvents { get; set; }
  }
}
