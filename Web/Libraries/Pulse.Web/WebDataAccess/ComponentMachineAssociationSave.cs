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
using Pulse.Web.WebDataAccess.CommonResponseDTO;


namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for ComponentMachineAssociation/Save service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Data/ComponentMachineAssociation/Save/", "GET", Summary = "ComponentMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&ComponentId=&RevisionId=")]
  public class ComponentMachineAssociationSave: IReturn<SaveModificationResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range in UTC
    /// </summary>
    [ApiMember (Name = "Range", Description = "range in UTC", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Range { get; set; }

    /// <summary>
    /// Component ID
    /// </summary>
    [ApiMember (Name = "ComponentId", Description = "", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? ComponentId { get; set; }

    /// <summary>
    /// Revision ID
    /// </summary>
    [ApiMember (Name = "RevisionId", Description = "-1: auto revision", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? RevisionId { get; set; }
  }
}
