// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Cache/ClearDaySlotCache service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ClearDaySlotCache", "GET", Summary = "Clear the day slot cache", Notes = "")]
  [Route("/Cache/ClearDaySlotCache", "GET", Summary = "Clear the day slot cache", Notes = "")]
  [AllowAnonymous]
  public class ClearDaySlotCacheRequestDTO: IReturn<OkDTO>
  {
  }
}

