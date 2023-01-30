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
  [Api("Request DTO for MachineModification/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineModification/FindById/", "GET", Summary = "Deprecated MachineModificationDAO.FindById", Notes = "To use with ?Id=&MachineId=")]
  [Route("/MachineModification/FindById/{Id}/{MachineId}", "GET", Summary = "Deprecated MachineModificationDAO.FindById", Notes = "")]
  [Route("/Data/MachineModification/FindById/", "GET", Summary = "MachineModificationDAO.FindById", Notes = "To use with ?Id=&MachineId=")]
  [Route("/Data/MachineModification/FindById/{Id}/{MachineId}", "GET", Summary = "MachineModificationDAO.FindById", Notes = "")]
  public class MachineModificationFindById : IReturn<MachineModificationDTO>
  {
    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "Id", Description = "MachineModification Id", ParameterType = "path", DataType = "long", IsRequired = true)]
    public long Id { get; set; }

    /// <summary>
    /// Id of requested reason
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
