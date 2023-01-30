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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /CncValueColor service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CncValueColor", "GET", Summary = "Get the cnc value color slots in a specified range", Notes = "To use with ?MachineId=&Range=&FieldId= or ?MachineModuleId=&Range=&FieldId=")]
  [Route ("/CncValue/Color", "GET", Summary = "Get the cnc value color slots in a specified range", Notes = "To use with ?MachineId=&Range=&FieldId= or ?MachineModuleId=&Range=&FieldId=")]
  public class CncValueColorRequestDTO
    : IReturn<CncValueColorResponseDTO>
  {
    /// <summary>
    /// Id of the machine module
    /// </summary>
    [ApiMember (Name = "MachineModuleId", Description = "Machine Module Id", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineModuleId { get; set; }

    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id in case the main machine module is targetted", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineId { get; set; }

    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }

    /// <summary>
    /// Id of the field
    /// </summary>
    [ApiMember (Name = "FieldId", Description = "Field Id. Main performance field if not set", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int FieldId { get; set; }

    /// <summary>
    /// Skip the details in the answer
    /// 
    /// Default: false (return them)
    /// </summary>
    [ApiMember (Name = "SkipDetails", Description = "Skip the details in the answer", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SkipDetails { get; set; }
  }
}
