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
  /// Request DTO for GetMachineStatusByIWP.
  /// </summary>
  [Route("/GetMachineStatusByIWP/", "GET")]
  [Route("/GetMachineStatusByIWP/{LineId}/{IwpId}/{Begin}/{End}", "GET")]
  public class GetMachineStatusByIWP
  {
    /// <summary>
    /// Id of Line
    /// </summary>
    public int LineId { get; set; }
    
    /// <summary>
    /// Id of IntermediateWorkPiece
    /// </summary>
    public int IwpId { get; set; }
    
    /// <summary>
    /// Begin of period - ISO string (not null)
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of period - ISO string (not null)
    /// </summary>
    public string End { get; set; }
  }
}
