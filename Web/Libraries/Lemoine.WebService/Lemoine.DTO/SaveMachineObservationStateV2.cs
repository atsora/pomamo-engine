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
  /// Request DTO for SaveMachineObservationStateV2.
  /// </summary>
  [Route("/SaveMachineObservationStateV2/", "GET")]
  [Route("/SaveMachineObservationStateV2/{Id}/{MachineObservationStateId}/{Begin}/{End}", "GET")]
  public class SaveMachineObservationStateV2
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Machine Observation State id to set
    /// </summary>
    public int MachineObservationStateId { get; set; }
    
    /// <summary>
    /// Start of period as ISO String
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// Optional end of period as ISO string (can be null)
    /// </summary>
    public string End { get; set; }
    
  }
}
