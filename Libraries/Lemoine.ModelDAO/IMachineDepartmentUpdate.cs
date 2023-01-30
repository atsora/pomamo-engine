// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table MachineDepartmentUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a machine and a department
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  public interface IMachineDepartmentUpdate: IMachineModification
  {
    /// <summary>
    /// Old department
    /// 
    /// null in case a new machine / department relation is set
    /// </summary>
    IDepartment OldDepartment { get; }
    
    /// <summary>
    /// New department
    /// 
    /// null in case the machine / department is deleted
    /// </summary>
    IDepartment NewDepartment { get; }
  }
}
