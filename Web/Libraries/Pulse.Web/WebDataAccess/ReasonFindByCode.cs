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
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for Reason/FindByCode service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Reason/FindByCode/", "GET", Summary = "Deprecated ReasonDAO.FindByCode", Notes = "To use with ?Code=")]
  [Route("/Reason/FindByCode/{Code}", "GET", Summary = "Deprecated ReasonDAO.FindByCode", Notes = "")]
  [Route("/Data/Reason/FindByCode/", "GET", Summary = "ReasonDAO.FindByCode", Notes = "To use with ?Code=")]
  [Route("/Data/Reason/FindByCode/{Code}", "GET", Summary = "ReasonDAO.FindByCode", Notes = "")]
  public class ReasonFindByCode : IReturn<ReasonDTO>
  {
    /// <summary>
    /// Code of requested reason
    /// </summary>
    [ApiMember(Name = "Id", Description = "Reason Code", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Code { get; set; }
  }
}
