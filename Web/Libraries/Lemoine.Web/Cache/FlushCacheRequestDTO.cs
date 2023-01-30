// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Cache/FlushCache service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Cache/Flush", "GET", Summary = "Flush the web service and business cache", Notes = "")]
  [AllowAnonymous]
  public class FlushCacheRequestDTO: IReturn<OkDTO>
  {
  }
}

