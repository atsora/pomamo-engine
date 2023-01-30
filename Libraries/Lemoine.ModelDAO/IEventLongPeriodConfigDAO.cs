// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventLongPeriodConfig.
  /// </summary>
  public interface IEventLongPeriodConfigDAO: IGenericUpdateDAO<IEventLongPeriodConfig, int>
  {
    /// <summary>
    /// FindAllForConfig implementation with EagerFetch
    /// 
    /// The result is sorted by:
    /// <item>not null monitored machines first</item>
    /// <item>not null machine modes first</item>
    /// <item>not null machine observation states first</item>
    /// <item>trigger duration</item>
    /// </summary>
    /// <returns></returns>
    IList<IEventLongPeriodConfig> FindAllForConfig();
    
    /// <summary>
    /// Get the used IEventLevel in EventLongPeriodConfig
    /// </summary>
    /// <returns></returns>
    IList<IEventLevel> GetLevels();
    
    /// <summary>
    /// Return the EventLongPeriodConfig corresponding to the key
    /// MonitoredMachine - MachineMode - MachineObservationState - TriggerDuration
    /// </summary>
    /// <param name="monitoredMachine">null means all</param>
    /// <param name="machineMode">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IList<IEventLongPeriodConfig> FindByKey(IMonitoredMachine monitoredMachine,
                                            IMachineMode machineMode,
                                            IMachineObservationState mos,
                                            TimeSpan duration);
    
    /// <summary>
    /// Return the EventLongPeriodConfig corresponding to the key
    /// MonitoredMachine - MachineMode - MachineObservationState - EventLevel
    /// </summary>
    /// <param name="monitoredMachine">null means all</param>
    /// <param name="machineMode">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="level"></param>
    /// <returns></returns>
    IList<IEventLongPeriodConfig> FindByKey(IMonitoredMachine monitoredMachine,
                                            IMachineMode machineMode,
                                            IMachineObservationState mos,
                                            IEventLevel level);
  }
}
