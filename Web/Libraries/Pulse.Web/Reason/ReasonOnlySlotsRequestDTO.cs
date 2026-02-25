// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for /ReasonOnlySlots service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonOnlySlots/", "GET", Summary = "Get the reason (only) slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/ReasonOnlySlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason (only) slots in a specified range", Notes = "")]
  [Route ("/Reason/ReasonOnlySlots/", "GET", Summary = "Get the reason (only) slots in a specified range", Notes = "To use with ?MachineId=&Range=")]
  [Route("/Reason/ReasonOnlySlots/Get/{MachineId}/{Range}", "GET", Summary = "Get the reason (only) slots in a specified range", Notes = "")]
  public class ReasonOnlySlotsRequestDTO: IReturn<ReasonOnlySlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember(Name="MachineId", Description="Machine Id", ParameterType="path", DataType="int", IsRequired=true)]
    public int MachineId { get; set; }
    
    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember(Name="Range", Description="Requested range. Default is [now, now]", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
    
    /// <summary>
    /// Option to retrieve in the same time if there is a possible reason selection for each ReasonOnlySlot 
    /// </summary>
    [ApiMember(Name="SelectableOption", Description="Option to retrieve in the same time if there is a possible reason selection for each ReasonOnlySlot ", ParameterType="path", DataType="boolean", IsRequired=false)]
    public bool SelectableOption { get; set; }
    
    /// <summary>
    /// Do not consider the extended period to get the sub machine mode periods
    /// </summary>
    [ApiMember(Name="NoPeriodExtension", Description="Do not consider the extended period to get the sub machine mode periods", ParameterType="path", DataType="string", IsRequired=false)]
    public bool NoPeriodExtension { get; set; }
    
    /// <summary>
    /// Limit Date/time range to extend a reason only slot
    /// 
    /// Default: "" that corresponds to (-oo,+oo)
    /// </summary>
    [ApiMember(Name="ExtendLimitRange", Description="Limit UTC date/time range to extend a reason only slot. Default is (-oo,+oo)", ParameterType="path", DataType="string", IsRequired=false)]
    public string ExtendLimitRange { get; set; }
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /ReasonOnlySlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonOnlySlots/Post", "POST", Summary = "Get the reason (only) slots in a specified range", Notes = "To use with ?MachineId=")]
  [Route("/ReasonOnlySlots/Post/{MachineId}", "POST", Summary = "Get the reason (only) slots in 1 or more specified ranges", Notes = "")]
  [Route("/Reason/ReasonOnlySlots/Post", "POST", Summary = "Get the reason (only) slots in a specified range", Notes = "To use with ?MachineId=")]
  [Route("/Reason/ReasonOnlySlots/Post/{MachineId}", "POST", Summary = "Get the reason (only) slots in 1 or more specified ranges", Notes = "")]
  public class ReasonOnlySlotsPostRequestDTO : IReturn<ReasonOnlySlotsResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Option to retrieve in the same time if there is a possible reason selection for each ReasonOnlySlot 
    /// </summary>
    [ApiMember (Name = "SelectableOption", Description = "Option to retrieve in the same time if there is a possible reason selection for each ReasonOnlySlot ", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SelectableOption { get; set; }

    /// <summary>
    /// Do not consider the extended period to get the sub machine mode periods
    /// </summary>
    [ApiMember (Name = "NoPeriodExtension", Description = "Do not consider the extended period to get the sub machine mode periods", ParameterType = "path", DataType = "string", IsRequired = false)]
    public bool NoPeriodExtension { get; set; }

    /// <summary>
    /// Limit Date/time range to extend a reason only slot
    /// 
    /// Default: "" that corresponds to (-oo,+oo)
    /// </summary>
    [ApiMember (Name = "ExtendLimitRange", Description = "Limit UTC date/time range to extend a reason only slot. Default is (-oo,+oo)", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string ExtendLimitRange { get; set; }
  }
}
