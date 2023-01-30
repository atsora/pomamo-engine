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
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Misc
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Misc/SwitchCatchUpMode")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Misc/SwitchCatchUpMode/", "GET", Summary = "", Notes = "")]
  [AllowAnonymous]
  public class SwitchCatchUpModeRequestDTO : IReturn<OkDTO>
  {
    /// <summary>
    /// Id
    /// </summary>
    [ApiMember (Name = "Active", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public bool Active { get; set; } = true;
  }
}
