// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.WebDataAccess.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for OperationMachineAssociation/Save service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Data/OperationMachineAssociation/Save/", "GET", Summary = "OperationMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&OperationId=&RevisionId=")]
  public class OperationMachineAssociationSave : IReturn<SaveModificationResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    [ApiMember (Name = "Range", Description = "", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Range { get; set; }

    /// <summary>
    /// Operation ID
    /// </summary>
    [ApiMember (Name = "OperationId", Description = "Not set: reset the operation", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? OperationId { get; set; }

    /// <summary>
    /// Revision ID
    /// </summary>
    [ApiMember (Name = "RevisionId", Description = "-1: auto revision", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? RevisionId { get; set; }
  }
}
