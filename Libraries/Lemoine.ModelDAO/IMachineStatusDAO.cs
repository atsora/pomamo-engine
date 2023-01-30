// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStatus.
  /// </summary>
  public interface IMachineStatusDAO: IGenericUpdateDAO<IMachineStatus, int>
  {
    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the reason and of the reason group
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    IMachineStatus FindByIdWithReasonGroup (int machineId);

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    IMachineStatus FindByIdWithMachineMode (int machineId);

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of the machine mode, the reason and of the reason group
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    IMachineStatus FindByIdWithMachineModeReasonGroup (int machineId);

    /// <summary>
    /// Get the machine status for the specified machine ID
    /// with an early fetch of:
    /// <item>the monitored machine</item>
    /// <item>the machine mode</item>
    /// <item>the reason</item>
    /// <item>the reason group</item>
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    IMachineStatus FindByIdWithMonitoredMachineMachineModeReasonGroup (int machineId);

    /// <summary>
    /// Get the machine statuses for a list of machines
    /// with an early fetch of:
    /// <item>the monitored machine</item>
    /// <item>the machine mode</item>
    /// <item>the reason</item>
    /// <item>the reason group</item>
    /// </summary>
    /// <param name="machineIds"></param>
    /// <returns></returns>
    IList<IMachineStatus> FindAllForWithMonitoredMachineMachineModeReasonGroup(int[] machineIds);
  }
}
