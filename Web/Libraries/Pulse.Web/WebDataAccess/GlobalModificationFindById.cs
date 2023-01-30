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
  [Api("Request DTO for GlobalModification/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/GlobalModification/FindById/", "GET", Summary = "Deprecated GlobalModificationDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/GlobalModification/FindById/{Id}", "GET", Summary = "Deprecated GlobalModificationDAO.FindById", Notes = "")]
  [Route("/Data/GlobalModification/FindById/", "GET", Summary = "GlobalModificationDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Data/GlobalModification/FindById/{Id}", "GET", Summary = "GlobalModificationDAO.FindById", Notes = "")]
  public class GlobalModificationFindById : IReturn<GlobalModificationDTO>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "Id", Description = "GlobalModification Id", ParameterType = "path", DataType = "long", IsRequired = true)]
    public long Id { get; set; }
  }
}
