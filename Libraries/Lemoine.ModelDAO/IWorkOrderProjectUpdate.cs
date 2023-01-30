// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Accepted types of modification
  /// </summary>
  public enum WorkOrderProjectUpdateModificationType {
    /// <summary>
    /// New work order / project association
    /// </summary>
    NEW = 1,
    /// <summary>
    /// Remove a work order / project association
    /// </summary>
    DELETE = 2,
    /// <summary>
    /// Change a work order / project association
    /// </summary>
    MODIFICATION = 3
  };

  /// <summary>
  /// Model of table WorkOrderProjectUpdate
  /// 
  /// This table tracks all the modifications
  /// that are made in the WorkOrderProject table:
  /// creation / delete / modification.
  /// 
  /// It is necessary to allow the Analyzer service
  /// to update correctly all the Analysis tables.
  /// </summary>
  public interface IWorkOrderProjectUpdate: IGlobalModification
  {
    /// <summary>
    /// Work order
    /// 
    /// Not null
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
    
    /// <summary>
    /// Project
    /// 
    /// Not null
    /// </summary>
    IProject Project { get; set; }
    
    /// <summary>
    /// Old quantity of projects associated to a work order
    /// </summary>
    int OldQuantity { get; set; }
    
    /// <summary>
    /// New quantity of projects associated to a work order
    /// </summary>
    int NewQuantity { get; set; }
    
    /// <summary>
    /// Modification type
    /// </summary>
    WorkOrderProjectUpdateModificationType TypeOfModification { get; set; }
  }
}
