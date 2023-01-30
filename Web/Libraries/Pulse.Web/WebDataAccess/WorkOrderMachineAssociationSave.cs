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
  [Api("Request DTO for WorkOrderMachineAssociation/Save service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/WorkOrderMachineAssociation/Save/", "GET", Summary = "Deprecated WorkOrderMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&WorkOrderId=&ResetTask=&RevisionId=")]
  [Route("/Data/WorkOrderMachineAssociation/Save/", "GET", Summary = "WorkOrderMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&WorkOrderId=&ResetTask=&RevisionId=")]
  public class WorkOrderMachineAssociationSave : IReturn<SaveModificationResponseDTO>
  {
    /// <summary>
    /// User ID
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range in UTC
    /// </summary>
    [ApiMember(Name="Range", Description="range in UTC", ParameterType="path", DataType="string", IsRequired=true)]
    public string Range { get; set; }
    
    /// <summary>
    /// WorkOrder ID
    /// </summary>
    [ApiMember(Name="WorkOrderId", Description="", ParameterType="path", DataType="int", IsRequired=false)]
    public int? WorkOrderId { get; set; }
    
    /// <summary>
    /// WorkOrder details
    /// </summary>
    [ApiMember(Name="ResetTask", Description="", ParameterType="path", DataType="bool", IsRequired=false)]
    public bool? ResetTask { get; set; }
    
    /// <summary>
    /// Revision ID
    /// </summary>
    [ApiMember(Name="RevisionId", Description="-1: auto revision", ParameterType="path", DataType="int", IsRequired=false)]
    public int? RevisionId { get; set; }
  }
}
