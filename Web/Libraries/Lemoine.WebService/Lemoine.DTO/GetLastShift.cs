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

namespace Lemoine.DTO
{
  /// <summary>
  /// Request DTO for GetLastShiftStatus
  /// </summary>
  [Api("Request DTO for GetLastShift service")]
  [Route("/GetLastShift/", "GET", Summary="Default route to be used in combination with ?MachineId=xxx")]
  [Route("/GetLastShift/{MachineId}", "GET", Summary="Default request")]
  public class GetLastShift
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    [ApiMember(Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
