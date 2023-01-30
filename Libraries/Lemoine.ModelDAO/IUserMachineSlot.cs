// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserMachineSlot
  /// 
  /// Analysis table where are stored all the periods where a user is associated to set of machines.
  /// </summary>
  public interface IUserMachineSlot: ISlot, IPartitionedByUser, IComparable<IUserMachineSlot>, ICloneable
  {
    /// <summary>
    /// List of associated machine state templates / machines
    /// the key is the machine id
    /// </summary>
    IDictionary<int, IUserMachineSlotMachine> Machines { get; }
    
    /// <summary>
    /// Remove all the associated machines
    /// </summary>
    void RemoveAll ();
    
    /// <summary>
    /// Add a machine and its associate machine state template
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate"></param>
    void Add (IMachine machine, IMachineStateTemplate machineStateTemplate);
    
    /// <summary>
    /// Remove a machine
    /// </summary>
    /// <param name="machine"></param>
    void Remove (IMachine machine);
  }
}
