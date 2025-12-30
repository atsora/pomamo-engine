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
    IMachineStateTemplate MachineStateTemplate { get; set; }
    
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

    /// <summary>
    /// Dynamic times (start/end) description: start,end
    /// 
    /// If start or end ends with +, then the reference date/time is the upper bound of the date/time range,
    /// else it is by default the lower bound of the date/time range
    /// 
    /// If Dynamic starts with "?", then switch to aggressive mode
    /// </summary>
    string Dynamic { get; set; }

    /// <summary>
    /// Dynamic start
    /// </summary>
    string DynamicStart { get; }

    /// <summary>
    /// Dynamic end
    /// </summary>
    string DynamicEnd { get; }
  }
}
