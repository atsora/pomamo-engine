// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICurrentCncAlarm.
  /// </summary>
  public interface ICurrentCncAlarmDAO: IGenericByMachineModuleUpdateDAO<ICurrentCncAlarm, int>
  {
    /// <summary>
    /// Find all the ICurrentCncAlarm for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IList<ICurrentCncAlarm> FindByMachineModule(IMachineModule machineModule);
    
    /// <summary>
    /// Get all CurrentCncAlarm of a particular cnc, after a specific date
    /// The severity is loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <returns></returns>
    IList<ICurrentCncAlarm> FindByCncWithSeverity(string cncType);
    
    /// <summary>
    /// Find all the ICurrentCncAlarm for the specified machine module
    /// The severity and machine modules are loaded
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    IList<ICurrentCncAlarm> FindByMachineModuleWithSeverity(IMachineModule machineModule);
  }
}
