// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineStateTemplateAssociation
  /// 
  /// This new table is designed to add a Machine State Template / Machine
  /// association.
  /// </summary>
  public interface IMachineStateTemplateAssociation: IMachineAssociation
  {
    /// <summary>
    /// Clone a MachineStateTemplateAssociation with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IMachineStateTemplateAssociation Clone (UtcDateTimeRange range);

    /// <summary>
    /// Reference to the Machine State Template
    /// 
    /// Not null
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }
    
    /// <summary>
    /// Optional reference to the User
    /// </summary>
    IUser User { get; set; }
    
    /// <summary>
    /// Optional reference to the Shift
    /// </summary>
    IShift Shift { get; set; }
    
    /// <summary>
    /// Force re-building the machine state templates
    /// 
    /// Default is False
    /// </summary>
    bool Force { get; set; }
    
    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; } 
  }
}
