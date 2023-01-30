// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICncVariable : IWithRange, IVersionable, IPartitionedByMachineModule
  {
    /// <summary>
    /// Cnc variable key
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Cnc variable value
    /// </summary>
    object Value { get ; set; }

    /// <summary>
    /// Make the Cnc variable slot shorter
    /// </summary>
    /// <param name="newUpperBound"></param>
    void Stop (UpperBound<DateTime> newUpperBound);
  }
}
