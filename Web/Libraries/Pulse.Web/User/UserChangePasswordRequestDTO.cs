// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

namespace Pulse.Web.User
{
  /// <summary>
  /// Request DTO of /User/ChangePassword
  /// </summary>
  [Api ("Request DTO for /User/ChangePassword")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/User/ChangePassword/", "POST", Summary = "Service to change a user password", Notes = "?Login= is optional, it may be in the POST data instead")]
  [Route ("/ChangePassword/", "POST", Summary = "Service to change a user password", Notes = "?Login= is optional, it may be in the POST data instead")]
  public class UserChangePasswordRequestDTO : IReturn<OkDTO>
  {
  }

  /// <summary>
  /// POST Dto for /User/ChangePassword
  /// </summary>

  public class UserChangePasswordPostDto
  {
    /// <summary>
    /// Login if not set in the URL
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// Old password
    /// </summary>
    public string OldPassword { get; set; }

    /// <summary>
    /// New passowrd
    /// </summary>
    public string NewPassword { get; set; }
  }
}
