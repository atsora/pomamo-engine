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
  [Api ("Request DTO for /Machine/GroupZoomIn service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/GroupZoomIn/", "GET", Summary = "", Notes = "To use with ?GroupId=")]
  [Route ("/Machine/GroupZoomIn/Get/{GroupId}", "GET", Summary = "", Notes = "")]
  public class GroupZoomInRequestDTO : IReturn<GroupZoomInResponseDTO>
  {
    /// <summary>
    /// Parent group id for zoom in
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Parent group id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Include the group details of the children
    /// </summary>
    [ApiMember (Name = "Details", Description = "Include the group details of the children, including the name and single machine property", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Details { get; set; }
  }
}
