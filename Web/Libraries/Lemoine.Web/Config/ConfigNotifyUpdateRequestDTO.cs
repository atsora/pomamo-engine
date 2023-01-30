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

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Config/NotifyUpdate service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Config/NotifyUpdate", "GET", Summary = "Notify an update of a configuration", Notes = "")]
  [AllowAnonymous]
  public class ConfigNotifyUpdateRequestDTO: IReturn<OkDTO>
  {
  }
}

