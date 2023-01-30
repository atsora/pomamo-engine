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

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ExceptionTest service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/ExceptionTest/", "GET", Summary = "", Notes = "")]
  [Route ("/Info/ExceptionTest/", "GET", Summary = "", Notes = "")]
  public class ExceptionTestRequestDTO : IReturn<ExceptionTestResponseDTO>
  {
  }
}
