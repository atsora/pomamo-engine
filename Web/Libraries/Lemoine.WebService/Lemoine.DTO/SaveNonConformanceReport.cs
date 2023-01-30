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
  /// Request DTO for SaveNonConformanceReport
  /// </summary>
  [Route("/SaveNonConformanceReport/", "GET")]
  [Route("/SaveNonConformanceReport/{DeliverablePieceId}/{NonConformanceReasonId}/{IntermediateWorkPieceId}/{MachineId}/{Fixable}/{OperationDateTime}", "GET")]
  [Route("/SaveNonConformanceReport/{DeliverablePieceId}/{NonConformanceReasonId}/{IntermediateWorkPieceId}/{MachineId}/{Fixable}/{OperationDateTime}/{NonConformanceDetails}", "GET")]
  public class SaveNonConformanceReport
  {

    /// <summary>
    /// DeliverablePiece id
    /// </summary>
    public int DeliverablePieceId { get; set; }

    /// <summary>
    /// NonConformanceReason id
    /// </summary>
    public int? NonConformanceReasonId { get; set; }

    /// <summary>
    /// IntermediateWorkPiece id
    /// </summary>
    public int? IntermediateWorkPieceId { get; set; }
    
    /// <summary>
    /// Machine id
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// if nonconformance is fixable or not
    /// </summary>
    public bool? Fixable { get; set;}
    
    /// <summary>
    /// Operation datetime in format yyyy-MM-dd
    /// </summary>
    public string OperationDateTime { get; set;}
    
    /// <summary>
    /// non conformance details
    /// </summary>
    public string NonConformanceDetails { get; set;}
    
    
  }
}
