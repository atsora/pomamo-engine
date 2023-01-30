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
  /// Request DTO for GetMachineAlarms (V2).
  /// </summary>
  [Route("/GetMachineAlarmsV2/", "GET")]
  [Route("/GetMachineAlarmsV2/{Id}", "GET")]  
  public class GetMachineAlarmsV2
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>    
    public int Id { get; set; }
    
    /// <summary>
    /// Optional start of period as offset in ISO string date format
    /// </summary>
    public string Begin { get; set; }
  }
}
