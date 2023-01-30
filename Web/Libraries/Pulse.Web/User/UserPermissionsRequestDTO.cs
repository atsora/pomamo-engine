// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Attributes;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

namespace Pulse.Web.User
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /User/Permissions service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [AllowAnonymous]
  [Route ("/User/Permissions", "POST", Summary = "Get the user permissions", Notes = "?Login= is optional, it may be in the POST data instead")]
  [Route ("/UserPermissions/Post", "POST", Summary = "Get the user permissions", Notes = "?Login= is optional, it may be in the POST data instead")]
  public class UserPermissionsPostRequestDTO : IReturn<UserPermissionsResponseDTO>
  {
  }

  /// <summary>
  /// POST Dto for /User/Permissions
  /// </summary>
  public class UserPermissionsPostDto
  {
    /// <summary>
    /// Login
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }
  }
}
