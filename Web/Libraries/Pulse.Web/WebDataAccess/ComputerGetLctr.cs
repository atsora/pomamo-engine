// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
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
  [Api("Request DTO for Computer/GetLctr service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Computer/GetLctr", "GET", Summary = "Deprecated ComputerDAO.GetLctr", Notes = "")]
  [Route("/Data/Computer/GetLctr", "GET", Summary = "ComputerDAO.GetLctr", Notes = "")]
  [AllowAnonymous]
  public class ComputerGetLctr : IReturn<ComputerDTO>
  {
  }
}
