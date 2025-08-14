// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table OperationMachineAssociation
  /// 
  /// In this new table is stored a new row each time
  /// an operation is manually associated to a machine.
  /// Usually the operation is automatically associated
  /// to the machine thanks to the ISO file identification and stamping.
  /// This table allows to manually set an operation to a machine period.
  /// 
  /// It is also very useful to associate yellow periods to an operation
  /// (where the ISO file identification is not available)
  /// following some predefined rules.
  /// 
  /// It does not represent the current relation between the operation
  /// and a machining resource, but all the manual or automatic associations
  /// that are made between an operation and a machine.
  /// 
  /// To know the current relation between a machining resource
  /// and an operation, the table Operation Slot that is filled in
  /// by the Analyzer must be used.
  /// </summary>
  public interface IOperationMachineAssociation: IMachineAssociation
  {
    /// <summary>
    /// Reference to the Operation
    /// </summary>
    IOperation Operation { get; set; }

    /// <summary>
    /// Determined work order from the operation
    /// It is possible to force it while it remains compatible with the operation
    /// 
    /// Note: This is usually only used when the object is only kept in memory and not persistent.
    ///       There is no associated database column
    /// </summary>
    IWorkOrder WorkOrder { get; set;  }

    /// <summary>
    /// Determined line from the operation
    /// It is possible to force it while it remains compatible with the operation
    /// 
    /// Note: This is usually only used when the object is only kept in memory and not persistent.
    ///       There is no associated database column
    /// </summary>
    ILine Line { get; set; }

    /// <summary>
    /// Task that was determined from the operation
    /// It is possible to force it while it remains compatible with the operation
    /// 
    /// Note: This is usually only used when the object is only kept in memory and not persistent.
    ///       There is no associated database column
    /// </summary>
    IManufacturingOrder ManufacturingOrder { get; set; }

    /// <summary>
    /// Determined component from the operation
    /// It is possible to force it while it remains compatible with the operation
    /// 
    /// Note: This is usually only used when the object is only kept in memory and not persistent.
    ///       There is no associated database column
    /// </summary>
    IComponent Component { get; set; }

    /// <summary>
    /// Association option
    /// </summary>
    Nullable<AssociationOption> Option { get; set; }
  }
}
