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
  [Api("Request DTO for Shift/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Shift/FindById/", "GET", Summary = "Deprecated ShiftDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Shift/FindById/{Id}", "GET", Summary = "Deprecated ShiftDAO.FindById", Notes = "")]
  [Route("/Data/Shift/FindById/", "GET", Summary = "ShiftDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Data/Shift/FindById/{Id}", "GET", Summary = "ShiftDAO.FindById", Notes = "")]
  public class ShiftFindById : IReturn<ShiftDTO>
  {
    /// <summary>
    /// Id of requested Shift
    /// </summary>
    [ApiMember(Name = "Id", Description = "Shift Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
