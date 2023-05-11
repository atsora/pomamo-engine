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
  [Api("Request DTO for TaskMachineAssociation/Save service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Data/TaskMachineAssociation/Save/", "GET", Summary = "TaskMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&TaskId=&ResetTask=&RevisionId=")]
  public class TaskMachineAssociationSave : IReturn<SaveModificationResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    [ApiMember(Name="Range", Description="", ParameterType="path", DataType="string", IsRequired=true)]
    public string Range { get; set; }
    
    /// <summary>
    /// Task ID
    /// </summary>
    [ApiMember(Name="TaskId", Description="", ParameterType="path", DataType="int", IsRequired=false)]
    public int? TaskId { get; set; }
    
    /// <summary>
    /// Revision ID
    /// </summary>
    [ApiMember(Name="RevisionId", Description="-1: auto revision", ParameterType="path", DataType="int", IsRequired=false)]
    public int? RevisionId { get; set; }
  }
}
