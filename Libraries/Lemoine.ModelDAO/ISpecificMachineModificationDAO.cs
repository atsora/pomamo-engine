// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a specific machine modification DAO
  /// (potentially partitioned by machine)
  /// 
  /// There is no default MakePersistent method.
  /// 
  /// It is recommended to add only methods that insert items directly in the partition
  /// and return the created id
  /// </summary>
  public interface ISpecificMachineModificationDAO<T>
    where T : IPartitionedByMachine
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    T FindById (long id, IMachine machine);
  }
}
