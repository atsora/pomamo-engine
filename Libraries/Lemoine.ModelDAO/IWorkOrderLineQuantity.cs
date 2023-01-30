// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Quantity of IntermediateWorkPiece to produce within a WorkOrderLine
  /// </summary>
  public interface IWorkOrderLineQuantity: IVersionable
  {
    /// <summary>
    /// IntermediateWorkPiece produced by a WorkOrderLine
    /// </summary>
    IIntermediateWorkPiece IntermediateWorkPiece { get; }
    
    /// <summary>
    /// Quantity of IntermediateWorkPiece to produce within the WorkOrderLine
    /// </summary>
    int Quantity { get; set; }
  }
}
