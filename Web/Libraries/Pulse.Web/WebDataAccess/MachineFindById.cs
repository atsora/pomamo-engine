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
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for Machine/FindById service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Machine/FindById/", "GET", Summary = "Deprecated MachineDAO.FindById", Notes = "To use with ?Id=")]
  [Route ("/Machine/FindById/{Id}", "GET", Summary = "Deprecated MachineDAO.FindById", Notes = "")]
  [Route ("/Data/Machine/FindById/", "GET", Summary = "MachineDAO.FindById", Notes = "To use with ?Id=")]
  [Route ("/Data/Machine/FindById/{Id}", "GET", Summary = "MachineDAO.FindById", Notes = "")]
  public class MachineFindById : IReturn<MachineDTO>
  {
    /// <summary>
    /// Id of requested Machine
    /// </summary>
    [ApiMember (Name = "Id", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
