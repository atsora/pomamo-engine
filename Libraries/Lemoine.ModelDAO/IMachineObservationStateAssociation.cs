// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineObservationStateAssociation
  /// 
  /// This new table is designed to add any Machine Observation State / Machine
  /// association, with the reference to a User when applicable.
  /// 
  /// It does not represent the current observation state of a machine,
  /// but all the manual or automatic reason changes that have been made.
  /// 
  /// To know the current observation states of a machine,
  /// the table Observation State Slot that is filled in by the Analyzer must be used.
  /// </summary>
  public interface IMachineObservationStateAssociation: IMachineAssociation
  {
    /// <summary>
    /// Reference to the Machine Observation State
    /// 
    /// Not null
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
    
    /// <summary>
    /// Reference to the Machine State Template
    /// 
    /// Nullable
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; set; }
    
    /// <summary>
    /// Reference to the User, according to the Machine Observation State
    /// </summary>
    IUser User { get; set; }
    
    /// <summary>
    /// Optional reference to the Shift
    /// </summary>
    IShift Shift { get; set; }
    
    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; }
  }
}
