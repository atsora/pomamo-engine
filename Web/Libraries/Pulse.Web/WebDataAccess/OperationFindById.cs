// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for Operation/FindById service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Data/Operation/FindById/", "GET", Summary = "OperationDAO.FindById", Notes = "To use with ?Id=")]
  [Route ("/Data/Operation/FindById/{Id}", "GET", Summary = "OperationDAO.FindById", Notes = "")]
  public class OperationFindById : IReturn<OperationDTO>
  {
    /// <summary>
    /// Id of the requested operation
    /// </summary>
    [ApiMember (Name = "Id", Description = "Operation Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
