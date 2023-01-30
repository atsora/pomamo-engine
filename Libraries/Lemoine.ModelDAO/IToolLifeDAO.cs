// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IToolLife.
  /// </summary>
  public interface IToolLifeDAO: IGenericByMachineModuleUpdateDAO<IToolLife, int>
  {
    /// <summary>
    /// Get all IToolLifes for a specific position in a machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    IList<IToolLife> FindAllByMachinePosition(IMachineModule machineModule, IToolPosition position);
    
    /// <summary>
    /// Get all IToolLife for a specific machine module with an eager fetch of IToolPosition
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IList<IToolLife> FindAllByMachineModule(IMachineModule machineModule);
    
    /// <summary>
    /// Get all IToolLife for a specific machine with an eager fetch of IToolPosition
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IEnumerable<IToolLife> FindAllByMachine(IMonitoredMachine machine);

    /// <summary>
    /// Find all monitored machines with an expired or 'in warning' tool
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindMachinesWithExpiredOrInWarningTools ();

    /// <summary>
    /// Find all monitored machines with an potentially expired tools
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindMachinesWithPotentiallyExpiredTools (TimeSpan maxRemainingDuration);
  }
}
