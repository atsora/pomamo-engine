// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for OperationCycleDeliverablePieceWithWorkInformationDTO
  /// </summary>
  public class OperationCycleDeliverablePieceWithWorkInformationDTO
  {
    
    /// <summary>
    /// List of Components
    /// </summary>
    public List<ComponentDTO> Components { get; set; }

    /// <summary>
    /// List of WorkOrders
    /// </summary>
    public List<WorkOrderDTO> WorkOrders { get; set; }

    /// <summary>
    /// List of Operations
    /// </summary>
    public List<OperationDTO> Operations { get; set; }

    /// <summary>
    /// List of OperationCycleDeliverablePiece
    /// </summary>
    public List<OperationCycleDeliverablePieceDTO> OperationCycleDeliverablePieces { get; set; }
    
  }
}
