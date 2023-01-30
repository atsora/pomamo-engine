// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO
  /// for tables that are partitioned by machine module
  /// 
  /// By default, an entity can't be updated, only saved.
  /// If you want your entity to be updated, please use IGenericByMachineModuleUpdateDAO instead.
  /// </summary>
  public interface IGenericByMachineModuleDAO<T, ID>
    : IBaseGenericDAO<T, ID>
    where T: IPartitionedByMachineModule
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    T FindById (ID id, IMachineModule machineModule);

    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModuleId"></param>
    /// <returns></returns>
    T FindById (ID id, int machineModuleId);
  }
}
