// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Automachinestatetemplate model, where a new machine state template can be automatically
  /// determined from a new machine mode and an optional current machine state template
  /// </summary>
  public interface IAutoMachineStateTemplate: IVersionable
  {
    /// <summary>
    /// New detected machine mode
    /// 
    /// Not null
    /// </summary>
    IMachineMode MachineMode { get; set; }
    
    /// <summary>
    /// Current machine state template
    /// 
    /// If null, any current machine state template applies
    /// </summary>
    IMachineStateTemplate Current { get; set; }
    
    /// <summary>
    /// New machine state template
    /// 
    /// Not null
    /// </summary>
    IMachineStateTemplate New { get; set; }
  }
}
