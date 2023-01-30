// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO for entities that may be updated
  /// for tables that are partitioned by machine module
  /// </summary>
  public interface IGenericByMachineModuleUpdateDAO<T, ID>
    : IBaseGenericUpdateDAO<T, ID>, IGenericByMachineModuleDAO<T, ID>
    where T: Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByMachineModule
  {
    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, IMachineModule machineModule);

    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, int machineModuleId);
  }
}
