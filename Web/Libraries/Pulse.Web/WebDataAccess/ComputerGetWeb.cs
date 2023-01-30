// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

using System.Net;


namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for Computer/GetWeb service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Computer/GetWeb", "GET", Summary = "Deprecated ComputerDAO.GetWeb", Notes = "")]
  [Route("/Data/Computer/GetWeb", "GET", Summary = "ComputerDAO.GetWeb", Notes = "")]
  [AllowAnonymous]
  public class ComputerGetWeb : IReturn<List<ComputerDTO>>
  {
  }
}
