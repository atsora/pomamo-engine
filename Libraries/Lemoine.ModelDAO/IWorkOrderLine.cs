// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IWorkOrderLine.
  /// </summary>
  public interface IWorkOrderLine: ISlot, IComparable<IWorkOrderLine>, IPartitionedByLine
  {
    /// <summary>
    /// WorkOrder to be associated with a line
    /// </summary>
    IWorkOrder WorkOrder { get; }
    
    /// <summary>
    /// Production wished deadline of a line for a specific workorder
    /// (the End of a WorkOrderLine corresponds to the beginning of the next WorkOrderLine
    /// sharing the same Line)
    /// </summary>
    DateTime Deadline { get; set; }
    
    /// <summary>
    /// Component quantity that has to be produced between the start datetime and
    /// the end date time for the line
    /// </summary>
    int Quantity { get; set; }
    
    /// <summary>
    /// Map Intermediate Work Piece => Quantity
    /// 
    /// The key is the intermediate work piece id
    /// </summary>
    IDictionary<int, IWorkOrderLineQuantity> IntermediateWorkPieceQuantities { get; }

    /// <summary>
    /// Set a quantity of intermediate work pieces
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="quantity"></param>
    void SetIntermediateWorkPieceQuantity (IIntermediateWorkPiece intermediateWorkPiece,
                                           int quantity);

    /// <summary>
    /// Set a quantity of intermediate work pieces
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="quantity"></param>
    void AddIntermediateWorkPieceQuantity (IIntermediateWorkPiece intermediateWorkPiece,
                                           int quantity);
  }
}
