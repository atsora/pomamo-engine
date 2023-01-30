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
  /// for tables that are partitioned by monitored machine
  /// 
  /// By default, an entity can't be updated, only saved.
  /// If you want your entity to be updated, please use IGenericByMonitoredMachineUpdateDAO instead.
  /// </summary>
  public interface IGenericByMonitoredMachineDAO<T, ID>
    : IBaseGenericDAO<T, ID>
    where T: IPartitionedByMonitoredMachine
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    T FindById (ID id, IMonitoredMachine machine);

    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    T FindById (ID id, int machineId);
  }
}
