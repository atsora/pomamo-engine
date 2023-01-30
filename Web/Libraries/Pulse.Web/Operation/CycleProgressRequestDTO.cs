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
  [Api("Request DTO for /CycleProgress service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/CycleProgress/", "GET", Summary = "Get the progress of the current cycle for the specified group or machine", Notes = "To use with ?MachineId= or ?GroupId=")]
  [Route("/CycleProgress/Get/{GroupId}", "GET", Summary = "Get the progress of the current cycle for the specified group or machine", Notes = "")]
  [Route("/Operation/CycleProgress/", "GET", Summary = "Get the progress of the current cycle for the specified group or machine", Notes = "To use with ?MachineId= or ?GroupId=")]
  [Route("/Operation/CycleProgress/Get/{GroupId}", "GET", Summary = "Get the progress of the current cycle for the specified group or machine", Notes = "")]
  public class CycleProgressRequestDTO: IReturn<CycleProgressResponseDTO>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public CycleProgressRequestDTO ()
    {
    }

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
    /// Do not return the details by machine module
    /// </summary>
    [ApiMember (Name="Light", Description="Do not return the details by machine module too", ParameterType="path", DataType="boolean", IsRequired=false)]
    public bool Light { get; set; }

    /// <summary>
    /// Include the events in the response (default is false)
    /// </summary>
    [ApiMember (Name = "IncludeEvents", Description = "Include the events in the response", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeEvents { get; set; }
  }
}
