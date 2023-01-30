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
  [Api("Request DTO for Reason/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Reason/FindById/", "GET", Summary = "Deprecated ReasonDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Reason/FindById/{Id}", "GET", Summary = "Deprecated ReasonDAO.FindById", Notes = "")]
  [Route("/Data/Reason/FindById/", "GET", Summary = "ReasonDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Data/Reason/FindById/{Id}", "GET", Summary = "ReasonDAO.FindById", Notes = "")]
  public class ReasonFindById : IReturn<ReasonDTO>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "Id", Description = "Reason Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
