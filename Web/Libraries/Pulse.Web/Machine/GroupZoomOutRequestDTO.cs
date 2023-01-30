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
  [Api ("Request DTO for /Machine/GroupZoomOut service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/GroupZoomOut/", "GET", Summary = "", Notes = "To use with ?GroupId=")]
  [Route ("/Machine/GroupZoomOut/Get/{GroupId}", "GET", Summary = "", Notes = "")]
  public class GroupZoomOutRequestDTO : IReturn<GroupZoomOutResponseDTO>
  {
    /// <summary>
    /// Child group id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }
  }
}
