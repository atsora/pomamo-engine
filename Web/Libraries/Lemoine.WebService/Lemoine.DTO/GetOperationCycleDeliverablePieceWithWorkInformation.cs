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
  /// Request DTO for GetOperationCycleDeliverablePieceWithWorkInformation
  /// </summary>
  [Route("/GetOperationCycleDeliverablePieceWithWorkInformation/Machine", "GET")]
  [Route("/GetOperationCycleDeliverablePieceWithWorkInformation/Department", "GET")]
  [Route("/GetOperationCycleDeliverablePieceWithWorkInformation/{Begin}/{End}", "GET")]
  [Route("/GetOperationCycleDeliverablePieceWithWorkInformation/Machine/{MachineId}/{Begin}/{End}", "GET")]
  [Route("/GetOperationCycleDeliverablePieceWithWorkInformation/Department/{DepartmentId}/{MachineId}/{Begin}/{End}", "GET")]
  public class GetOperationCycleDeliverablePieceWithWorkInformation
  {
    /// <summary>
    /// Id of department
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Id of machine
    /// </summary>
    public int? MachineId { get; set; }

    /// <summary>
    /// Begin of period with format YYYY-MM-DDTHH:mm:ss
    /// </summary>
    public string Begin { get; set; }

    /// <summary>
    /// End of period with format YYYY-MM-DDTHH:mm:ss
    /// </summary>
    public string End { get; set; }
  }
}
