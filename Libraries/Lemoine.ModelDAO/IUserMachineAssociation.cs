// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserMachineAssociation
  /// 
  /// This new modification table records
  /// any new association between a user and a list of machines with a default machine observation state
  /// </summary>
  public interface IUserMachineAssociation: IGlobalModification, IPeriodAssociation
  {
    /// <summary>
    /// Reference to a User
    /// 
    /// Not null
    /// </summary>
    IUser User { get; }
    
    /// <summary>
    /// List of associated machine state templates / machines
    /// </summary>
    IDictionary<int, IUserMachineAssociationMachine> Machines { get; }
    
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
