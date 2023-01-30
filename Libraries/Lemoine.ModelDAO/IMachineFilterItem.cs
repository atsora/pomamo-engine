// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Rule to apply on the item
  /// </summary>
  public enum MachineFilterRule
  {
    /// <summary>
    /// Add the item to the set of machines
    /// </summary>
    Add = 1,
    /// <summary>
    /// Remove the item to the set of machines
    /// </summary>
    Remove = -1
  }
  
  /// <summary>
  /// Individual item of Machine Filter
  /// </summary>
  public interface IMachineFilterItem: Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Order in the list of items
    /// </summary>
    int Order { get; }
    
    /// <summary>
    /// Rule to apply to the item
    /// </summary>
    MachineFilterRule Rule { get; }
    
    /// <summary>
    /// Check if a machine matches this machine filter
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    bool IsMatch (IMachine machine);    
  }
}
