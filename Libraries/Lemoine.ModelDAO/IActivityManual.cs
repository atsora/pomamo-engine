// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ActivityManual
  /// 
  /// This new table is designed to add any manual activity change
  /// to a machine.
  /// 
  /// This makes the analysis of the different tables much more convenient.
  /// 
  /// It does not represent the current activity of a machine,
  /// but all the manual activity changes that have been made.
  /// 
  /// To know the current activity of a machine,
  /// you can still use the Fact table though,
  /// because it contains an additional column for overwritten activities.
  /// </summary>
  public interface IActivityManual: IMachineAssociation
  {
    /// <summary>
    /// Reference to the manual MachineMode
    /// </summary>
    IMachineMode MachineMode { get; }
  }
}
