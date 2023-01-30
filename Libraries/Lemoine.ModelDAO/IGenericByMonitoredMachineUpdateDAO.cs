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
  /// for tables that are partitioned by monitored machine
  /// </summary>
  public interface IGenericByMonitoredMachineUpdateDAO<T, ID>
    : IBaseGenericUpdateDAO<T, ID>, IGenericByMonitoredMachineDAO<T, ID>
    where T: Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByMonitoredMachine
  {
    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, IMonitoredMachine machine);

    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineId"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, int machineId);
  }
}
