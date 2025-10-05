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
  /// Request DTO for the GroupsService
  /// </summary>
  [Api ("Request DTO for /Machine/Groups service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/Groups/", "GET", Summary = "Get all the groups", Notes = "")]
  [Route ("/MachineGroups/", "GET", Summary = "Get all the groups", Notes = "")]
  [AllowAnonymous] // Temporary solution to use this service in AtrackingReporting, until the full authentication in AtrackingReporting is supported
  public class GroupsRequestDTO : IReturn<GroupsResponseDTO>
  {
    /// <summary>
    /// Zoom in groups when available
    /// </summary>
    [ApiMember (Name = "Zoom", Description = "Zoom in groups when available", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Zoom { get; set; }

    /// <summary>
    /// Return all the groups, not only the ones for the machine selection
    /// 
    /// By default, only the groups for the machine selection are returned
    /// </summary>
    [ApiMember (Name = "All", Description = "Return all the groups, not only the groups for the machine selection", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool All { get; set; }
    
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
    /// Include the machine details (id, display) in all static groups
    /// </summary>
    [ApiMember (Name = "MachineList", Description = "Include the complete machine list and the associated machine ids in all static groups", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool MachineList { get; set; }

    /// <summary>
    /// Login
    /// 
    /// Please prefer to use the authentication methods, but as a work around, you can force the Login here
    /// </summary>
    [ApiMember (Name = "Login", Description = "Restrict the groups for this specific user login", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Login { get; set; }
  }
}
