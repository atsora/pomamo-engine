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
  /// Request DTO for GetMachineObservationStateListV2
  /// </summary>
  [Route("/GetMachineObservationStateListV2/", "GET")]
  [Route("/GetMachineObservationStateListV2/{Id}", "GET")]
  [Route("/GetMachineObservationStateListV2/{Id}/{Begin}", "GET")]
  [Route("/GetMachineObservationStateListV2/{Id}/{Begin}/{End}", "GET")]
  public class GetMachineObservationStateListV2
  {
    /// <summary>
    /// Machine id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Optional start of period as ISO string
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// Optional end of period as ISO string
    /// </summary>
    public string End { get; set; }    

  }
}
