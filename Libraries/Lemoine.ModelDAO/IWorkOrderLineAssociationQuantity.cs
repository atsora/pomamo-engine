// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IWorkOrderLineAssociationQuantity.
  /// </summary>
  public interface IWorkOrderLineAssociationQuantity
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
