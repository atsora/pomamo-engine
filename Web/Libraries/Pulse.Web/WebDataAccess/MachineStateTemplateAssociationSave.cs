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
  [Api("Request DTO for MachineStateTemplateMachineAssociation/Save service. By default the process is synchronous")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/MachineStateTemplateMachineAssociation/Save/", "GET", Summary = "Deprecated MachineStateTemplateMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&MachineStateTemplateId=&UserId=&ShiftId=&Force=&RevisionId=")]
  [Route("/Data/MachineStateTemplateMachineAssociation/Save/", "GET", Summary = "MachineStateTemplateMachineAssociation.MakePersistent", Notes = "To use with ?MachineId=&Range=&MachineStateTemplateId=&UserId=&ShiftId=&Force=&RevisionId=")]
  public class MachineStateTemplateMachineAssociationSave : IReturn<SaveModificationResponseDTO>
  {
    /// <summary>
    /// User ID
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired=true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    [ApiMember(Name="Range", Description="", ParameterType="path", DataType="string", IsRequired=true)]
    public string Range { get; set; }
    
    /// <summary>
    /// MachineStateTemplate ID
    /// </summary>
    [ApiMember(Name="MachineStateTemplateId", Description="", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineStateTemplateId { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    [ApiMember(Name="UserId", Description="", ParameterType="path", DataType="int", IsRequired=false)]
    public int? UserId { get; set; }
    
    /// <summary>
    /// Shift ID
    /// </summary>
    [ApiMember(Name="ShiftId", Description="", ParameterType="path", DataType="int", IsRequired=false)]
    public int? ShiftId { get; set; }
    
    /// <summary>
    /// Force
    /// </summary>
    [ApiMember(Name="Force", Description="", ParameterType="path", DataType="bool", IsRequired=false)]
    public bool? Force { get; set; }
    
    /// <summary>
    /// Revision ID
    /// </summary>
    [ApiMember(Name="RevisionId", Description="-1: auto revision", ParameterType="path", DataType="int", IsRequired=false)]
    public int? RevisionId { get; set; }
  }
}
