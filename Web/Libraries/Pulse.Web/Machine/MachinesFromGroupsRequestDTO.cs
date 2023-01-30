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
  [Api ("Request DTO for /Machine/FromGroups service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/FromGroups/", "GET", Summary = "Get all the machines that are parts of the specified groups", Notes = "To use with ?GroupIds=")]
  [Route ("/Machine/FromGroups/Get/{GroupIds}", "GET", Summary = "Get all the machines that are parts of the specified groups", Notes = "")]
  [Route ("/MachinesFromGroups/", "GET", Summary = "Get all the machines that are parts of the specified groups", Notes = "To use with ?GroupIds=")]
  [AllowAnonymous] // Note: to be used in the reports without the OAuth2 authentication
  public class MachinesFromGroupsRequestDTO : IReturn<MachinesFromGroupsResponseDTO>
  {
    /// <summary>
    /// Ids of the groups
    /// </summary>
    [ApiMember (Name = "GroupIds", Description = "Group Ids, comma (',') separated list", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupIds { get; set; }

    /// <summary>
    /// Include the machine details
    /// </summary>
    [ApiMember (Name = "MachineDetails", Description = "Include the machine details (name, ...)", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool MachineDetails { get; set; }

    /// <summary>
    /// Include the obsolete machines (or groups with only obsolete machines)
    /// </summary>
    [ApiMember (Name = "IncludeObsolete", Description = "Include the obsolete machines (or groups with only obsolete machines)", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeObsolete { get; set; }

    /// <summary>
    /// Include the not monitored machines (or groups with only not monitored machines)
    /// </summary>
    [ApiMember (Name = "IncludeNotMonitored", Description = "Include the not monitored machines (or groups with only not monitored machines)", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool IncludeNotMonitored { get; set; }

    /// <summary>
    /// Login
    /// 
    /// Please prefer to use the authentication methods, but as a work around, you can force the Login here
    /// </summary>
    [ApiMember (Name = "Login", Description = "Restrict the machines for this specific user login", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Login { get; set; }
  }
}
