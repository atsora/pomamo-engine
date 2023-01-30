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
using System.ComponentModel;
using Lemoine.Web.User;

namespace Pulse.Web.User
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /User/ValidateAuthenticationCode service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/User/ValidateAuthenticationCode/", "POST", Summary = "", Notes = "")]
  [AllowAnonymous]
  public class ValidateAuthenticationCodeRequestDTO : IReturn<UserPermissionsResponseDTO>
  {
  }

  /// <summary>
  /// Post DTO
  /// </summary>
  public class ValidateAuthenticationCodePostDTO
  {
    /// <summary>
    /// Kind of authentication to use
    /// </summary>
    public string AuthenticationKind { get; set; }

    /// <summary>
    /// [Optional] Authentication name in case there are several configurations with the same kind of authentication
    /// </summary>
    [DefaultValue("")]
    public string AuthenticationName { get; set; } = "";

    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// State if required
    /// </summary>
    [DefaultValue ("")]
    public string State { get; set; } = "";
  }
}
