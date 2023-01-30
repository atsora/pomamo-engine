// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table MachineCompanyUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a machine and a company
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  public interface IMachineCompanyUpdate: IMachineModification
  { 
    /// <summary>
    /// Old company
    /// 
    /// null in case a new machine / company relation is set
    /// </summary>
    ICompany OldCompany { get; }
    
    /// <summary>
    /// New company
    /// 
    /// null in case the machine / company is deleted
    /// </summary>
    ICompany NewCompany { get; }
  }
}
