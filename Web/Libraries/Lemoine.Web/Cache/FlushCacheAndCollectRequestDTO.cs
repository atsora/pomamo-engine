// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /Cache/FlushCacheAndCollect service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/FlushCache", "GET", Summary = "Flush the web service and business cache and collect the data in the garbage collector", Notes = "")]
  [Route("/Cache/FlushCacheAndCollect", "GET", Summary = "Flush the web service and business cache and collect the data in the garbage collector", Notes = "")]
  [AllowAnonymous]
  public class FlushCacheAndCollectRequestDTO: IReturn<OkDTO>
  {
  }
}

