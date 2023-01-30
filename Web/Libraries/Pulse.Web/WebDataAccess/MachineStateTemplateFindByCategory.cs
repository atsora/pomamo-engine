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
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for MachineStateTemplate/FindByCategory service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineStateTemplate/FindByCategory/", "GET", Summary = "Deprecated MachineStateTemplateDAO.FindByCategory", Notes = "To use with ?CategoryId=")]
  [Route("/MachineStateTemplate/FindByCategory/{CategoryId}", "GET", Summary = "Deprecated MachineStateTemplateDAO.FindByCategory", Notes = "")]
  [Route("/Data/MachineStateTemplate/FindByCategory/", "GET", Summary = "MachineStateTemplateDAO.FindByCategory", Notes = "To use with ?CategoryId=")]
  [Route("/Data/MachineStateTemplate/FindByCategory/{CategoryId}", "GET", Summary = "MachineStateTemplateDAO.FindByCategory", Notes = "")]
  public class MachineStateTemplateFindByCategory : IReturn<List<MachineStateTemplateDTO>>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "CategoryId", Description = "MachineStateTemplate CategoryId", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int CategoryId { get; set; }
  }
}
