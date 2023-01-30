// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IWorkOrderLineAssociation.
  /// </summary>
  public interface IWorkOrderLineAssociation: ILineAssociation
  {
    /// <summary>
    /// Reference to the WorkOrder
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Production wished deadline of a line for a specific workorder
    /// (the End of a WorkOrderLine corresponds to the beginning of the next WorkOrderLine
    /// sharing the same Line)
    /// </summary>
    DateTime Deadline { get; }
    
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
    IDictionary<int, IWorkOrderLineAssociationQuantity> IntermediateWorkPieceQuantities { get; }

      /// <summary>
    /// Set a quantity of intermediate work pieces
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="quantity"></param>
    void SetIntermediateWorkPieceQuantity (IIntermediateWorkPiece intermediateWorkPiece,
                                           int quantity);
  }
}
