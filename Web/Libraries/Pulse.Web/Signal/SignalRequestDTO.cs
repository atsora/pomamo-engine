// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Signal
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Signal service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Signal/", "GET", Summary = "Get the active signals for the specified group or machine", Notes = "To use with ?GroupId=")]
  [Route ("/Signal/Get/{GroupId}", "GET", Summary = "Get the active signal for the specified group or machine", Notes = "")]
  public class SignalRequestDTO : IReturn<SignalResponseDTO>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SignalRequestDTO ()
    {
    }

    /// <summary>
    /// Group Id
    /// </summary>
    [ApiMember (Name = "GroupId", Description = "Group Id", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string GroupId { get; set; }

    /// <summary>
    /// Key of the role that makes the request
    /// 
    /// Default: "" (not specified)
    /// </summary>
    [ApiMember (Name = "RoleKey", Description = "Role Key of the requester", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string RoleKey { get; set; }
  }
}
