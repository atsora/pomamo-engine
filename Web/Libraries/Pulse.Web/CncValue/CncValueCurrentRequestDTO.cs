// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /CncValue/Current service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/CncValue/Current", "GET", Summary = "Get a current cnc value", Notes = "To use with ?MachineId=")]
  [Route ("/CncValue/Current/Get/ByMachine/{MachineId}", "GET", Summary = "Get a current cnc value for the specified machine ID")]
  [Route ("/CncValue/Current/Get/ByMachine/{MachineId}/{PredefinedFields}", "GET", Summary = "Get a current cnc value for the specified machine ID")]
  public class CncValueCurrentRequestDTO : IReturn<CncValueCurrentResponseDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id to target the main machine module", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineId { get; set; }

    /// <summary>
    /// Id of the machine module
    /// </summary>
    [ApiMember (Name = "MachineModuleId", Description = "Machine Module Id. You must set either a MachineId or a MachineModuleId. If not set, the main machine module of the specified MachineId is considered", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int MachineModuleId { get; set; }

    /// <summary>
    /// Predefined set of field Ids
    /// 
    /// One of these values:
    /// <item>all: all fields for the specified machine module</item>
    /// <item>main (default): main performance field of the selected machine</item>
    /// </summary>
    [ApiMember (Name = "PredefinedFields", Description = "Predfined fields: all (all fields for the specified machine module) or main (default: main performance field of the machine)", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string PredefinedFields { get; set; }

    /// <summary>
    /// List of Field Ids to return.
    /// 
    /// If not empty, PredefinedFields parameter is ignored
    /// </summary>
    [ApiMember (Name = "FieldIds", Description = "Field Ids. If not empty, PredefinedFields parameter is ignored", ParameterType = "path", DataType = "List<int>", IsRequired = false)]
    public IList<int> FieldIds { get; set; }
  }
}
