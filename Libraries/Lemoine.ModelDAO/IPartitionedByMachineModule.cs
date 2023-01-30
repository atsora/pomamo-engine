// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that may be partioned by machine module
  /// </summary>
  public interface IPartitionedByMachineModule
  {
    /// <summary>
    /// Associated machine module
    /// 
    /// Can't be null
    /// </summary>
    IMachineModule MachineModule { get; }
  }
}
