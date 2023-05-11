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


namespace Pulse.Web.Reason
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for ReasonSave service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonSave", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?MachineId=&Range=&ReasonId=&ReasonDetails=")]
  [Route("/ReasonSave/Get/{MachineId}", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?Range=&ReasonId=&ReasonDetails=")]
  [Route("/ReasonSave/Get/{MachineId}/{ReasonId}", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?Range=&ReasonDetails=")]
  [Route("/Reason/ReasonSave", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?MachineId=&Range=&ReasonId=&ReasonDetails=")]
  [Route("/Reason/ReasonSave/Get/{MachineId}", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?Range=&ReasonId=&ReasonDetails=")]
  [Route("/Reason/ReasonSave/Get/{MachineId}/{ReasonId}", "GET", Summary = "Service to save reasons for user interfaces", Notes = "To use with ?Range=&ReasonDetails=")]
  public class ReasonSaveRequestDTO : IReturn<ReasonSaveResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Range
    /// </summary>
    [ApiMember(Name="Range", Description="Required if GET is used", ParameterType="path", DataType="string", IsRequired=false)]
    public string Range { get; set; }
    
    /// <summary>
    /// Reason ID
    /// </summary>
    [ApiMember(Name="ReasonId", Description="If not set, reset the reason", ParameterType="path", DataType="int", IsRequired=false)]
    public int? ReasonId { get; set; }

    /// <summary>
    /// Reason score to use (default: 100.0)
    /// </summary>
    [ApiMember (Name = "ReasonScore", Description = "Reason score", ParameterType = "path", DataType = "double", IsRequired = false)]
    public double? ReasonScore { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    [ApiMember (Name="ReasonDetails", Description="Reason details", ParameterType="path", DataType="string", IsRequired=false)]
    public string ReasonDetails { get; set; }
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for ReasonSave service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/ReasonSave/Post", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "To use with ?MachineId=&ReasonId=&ReasonDetails=")]
  [Route("/ReasonSave/Post/{MachineId}/{ReasonId}", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "")]
  [Route("/ReasonSave/Post/{MachineId}/{ReasonId}/{ReasonDetails}", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "")]
  [Route("/Reason/ReasonSave/Post", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "To use with ?MachineId=&ReasonId=&ReasonDetails=")]
  [Route("/Reason/ReasonSave/Post/{MachineId}/{ReasonId}", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "")]
  [Route("/Reason/ReasonSave/Post/{MachineId}/{ReasonId}/{ReasonDetails}", "POST", Summary = "Service to save reasons for user interfaces. Compare to the /ReasonMachineAssociation/Save service, a post request is possible to post different ranges", Notes = "")]
  public class ReasonSavePostRequestDTO : IReturn<ReasonSaveResponseDTO>
  {
    /// <summary>
    /// Machine ID
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Reason ID
    /// </summary>
    [ApiMember (Name = "ReasonId", Description = "If not set, reset the reason", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? ReasonId { get; set; }

    /// <summary>
    /// Reason score to use (default: 100.0)
    /// </summary>
    [ApiMember (Name = "ReasonScore", Description = "Reason score", ParameterType = "path", DataType = "double", IsRequired = false)]
    public double? ReasonScore { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    [ApiMember (Name = "ReasonDetails", Description = "Reason details", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string ReasonDetails { get; set; }
  }
}
