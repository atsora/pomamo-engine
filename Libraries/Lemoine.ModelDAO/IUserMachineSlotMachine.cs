// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IUserMachineSlotMachine.
  /// </summary>
  public interface IUserMachineSlotMachine
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    IMachine Machine { get; }
    
    /// <summary>
    /// Machine state template that is associated to the machine
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }
  }
}
