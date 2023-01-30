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
using Lemoine.Web.User;

namespace Pulse.Web.User
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /User/RenewToken service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/User/RenewToken/", "POST", Summary = "", Notes = "")]
  [AllowAnonymous]
  public class RenewTokenRequestDTO : IReturn<UserPermissionsResponseDTO>
  {
  }

  /// <summary>
  /// POST Dto for /User/RenewToken
  /// </summary>
  public class RenewTokenPostDto
  {
    /// <summary>
    /// Login (optional)
    /// </summary>
    public string Login { get; set; } = "";

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; }
  }
}
