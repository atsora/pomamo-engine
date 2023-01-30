// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

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
  [Api ("Request DTO for /Machine/Group service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/Group/", "GET", Summary = "Get the details of a group", Notes = "To use with ?GroupId=")]
  [Route ("/Machine/Group/Get/{GroupId}", "GET", Summary = "Get the details of a group", Notes = "")]
  public class GroupRequestDTO : IReturn<GroupDTO>
  {
    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Include the machine details (id, display)
    /// </summary>
    [ApiMember (Name = "MachineList", Description = "Include the complete machine list and the associated machine ids", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool MachineList { get; set; }
  }
}
