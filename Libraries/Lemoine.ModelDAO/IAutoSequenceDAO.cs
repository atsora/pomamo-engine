// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IAutoSequence.
  /// </summary>
  public interface IAutoSequenceDAO: IGenericByMachineModuleUpdateDAO<IAutoSequence, int>
  {
    /// <summary>
    /// Get the first auto-sequence after a specific date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IAutoSequence GetFirstAfter (IMachineModule machineModule,
                                 DateTime dateTime);

    /// <summary>
    /// Find all the auto-sequences at and after a given date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<IAutoSequence> FindAllAtAndAfter (IMachineModule machineModule,
                                            DateTime dateTime);

    /// <summary>
    /// Find all the auto-sequences for a given monitored machine
    /// 
    /// Order the result by ascending begin date/time
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    IList<IAutoSequence> FindAll (IMonitoredMachine monitoredMachine);
    
    /// <summary>
    /// Find all the auto-sequences for a specified machine module in a specific range
    /// 
    /// Order the result by ascending begin date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IAutoSequence> FindAllBetween (IMachineModule machineModule,
                                         UtcDateTimeRange range);
    
    /// <summary>
    /// Delete all the auto-sequences before a specified date/time
    /// (because they have already been processed)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    void DeleteBefore (IMachineModule machineModule,
                       DateTime dateTime);
  }
}
