// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table WorkOrderMachineAssociation
  /// 
  /// In this table is stored a new row each time a work order
  /// is associated to a machine.
  /// 
  /// It does not represent the current relation
  /// between the work order and a machining resource,
  /// but all the manual or automatic associations that are made
  /// between a work order and a machining resource.
  /// </summary>
  public interface IWorkOrderMachineAssociation : IMachineAssociation
  {
    /// <summary>
    /// Work order to associate to a machine
    /// </summary>
    IWorkOrder WorkOrder { get; set; }

    /// <summary>
    /// Line to associate to a machine with a work order
    /// </summary>
    ILine Line { get; set; }

    /// <summary>
    /// Task to associate to a machine with a work order
    /// </summary>
    ITask Task { get; set; }

    /// <summary>
    /// Is the option to reset the task active ?
    /// </summary>
    bool? ResetTask { get; set; }

    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; }
  }
}
