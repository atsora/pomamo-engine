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
  [Api("Request DTO for MachineStateTemplate/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineStateTemplate/FindById/", "GET", Summary = "Deprecated MachineStateTemplateDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/MachineStateTemplate/FindById/{Id}", "GET", Summary = "Deprecated MachineStateTemplateDAO.FindById", Notes = "")]
  [Route("/Data/MachineStateTemplate/FindById/", "GET", Summary = "MachineStateTemplateDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Data/MachineStateTemplate/FindById/{Id}", "GET", Summary = "MachineStateTemplateDAO.FindById", Notes = "")]
  public class MachineStateTemplateFindById : IReturn<MachineStateTemplateDTO>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "Id", Description = "MachineStateTemplate Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
