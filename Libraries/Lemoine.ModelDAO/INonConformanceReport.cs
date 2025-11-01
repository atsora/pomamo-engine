// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table nonconformancereport
  /// 
  /// This table tracks the nonconformance reason detected
  /// on deliverable piece during operation
  /// </summary>
  public interface INonConformanceReport: IMachineModification
  {
    /// <summary>
    /// Deliverable piece with nonconformance
    /// 
    /// not null
    /// </summary>
    IDeliverablePiece DeliverablePiece { get; set; }
    
    /// <summary>
    /// Intermediate work piece
    /// 
    /// not null
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; set; }
    
    /// <summary>
    /// Nonconformance reason
    /// 
    /// null in case nonconformance has been resolved
    /// </summary>
    INonConformanceReason NonConformanceReason { get; set; }
    
    /// <summary>
    /// Tells if nonconformance is fixable
    /// 
    /// </summary>
    bool? NonConformanceFixable { get; set; }
    
    /// <summary>
    /// It is used to retrieve in which operationcylce nonconformance occurs.
    /// It matchs end date of operationcycle.
    /// 
    /// if it is null, it corresponds to last operation cycle
    /// </summary>
    DateTime? NonConformanceOperationDateTime { get; set; }
    
    /// <summary>
    /// Non conformance reason Details
    /// </summary>
    string NonConformanceDetails { get; set; }
  }
}
