// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for OperationCycleDeliverablePieceDTO
  /// </summary>
  public class OperationCycleDeliverablePieceDTO
  {
    /// <summary>
    /// OperationCycleDeliverablePiece id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// OperationCycle id
    /// </summary>
    public int OperationCycleId { get; set; }

    /// <summary>
    /// DeliverablePiece id
    /// </summary>
    public int DeliverablePieceId { get; set; }
    
    /// <summary>
    /// OperationCycle begin in format YYYY-MM-DDTHH:mm:ss
    /// </summary>
    public string OperationCycleBegin  { get; set; }
    
    /// <summary>
    /// OperationCycle end in format YYYY-MM-DDTHH:mm:ss
    /// </summary>
    public string OperationCycleEnd { get; set; }

    /// <summary>
    /// DeliverablePiece code
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// Component id
    /// </summary>
    public int? ComponentId { get; set; }
    
    /// <summary>
    /// WorkOrder id
    /// </summary>
    public int? WorkOrderId { get; set; }

    /// <summary>
    /// Machine id
    /// </summary>
    public int MachineId { get; set; }
    
    /// <summary>
    /// Operation id
    /// </summary>
    public int? OperationId { get; set; }

    /// <summary>
    /// IntermediateWorkPiece id
    /// </summary>
    public int IntermediateWorkPieceId { get; set; }
    
    /// <summary>
    /// Non conformance reason
    /// </summary>
    public int? NonConformanceReasonId { get; set; }
    
    /// <summary>
    /// Non conformance details
    /// </summary>
    public string NonConformanceReasonDetails { get; set; }
    
  }
}
