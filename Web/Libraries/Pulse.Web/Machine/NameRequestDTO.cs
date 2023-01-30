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

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Machine/Name service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/Name/", "GET", Summary = "Get the name of a machine or a group", Notes = "To use with ?MachineId= or ?GroupId=")]
  [Route ("/Machine/Name/Get/{GroupId}", "GET", Summary = "Get the name of a machine or a group", Notes = "")]
  public class NameRequestDTO : IReturn<NameResponseDTO>
  {
    /// <summary>
    /// Machine Id for compatibility with the old service
    /// </summary>
    [ApiMember (Name = "Id", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int Id { get; set; }

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
  }
}
