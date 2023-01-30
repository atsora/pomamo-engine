// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventToolLifeConfig.
  /// </summary>
  public interface IEventToolLifeConfigDAO: IGenericUpdateDAO<IEventToolLifeConfig, int>
  {
    /// <summary>
    /// FindAllForConfig implementation with EagerFetch
    /// </summary>
    /// <returns></returns>
    IList<IEventToolLifeConfig> FindAllForConfig();
    
    /// <summary>
    /// Get the used IEventLevel in EventToolLifeConfig
    /// </summary>
    /// <returns></returns>
    IList<IEventLevel> GetLevels();
    
    /// <summary>
    /// Return the EventToolLifeConfig corresponding to the key
    /// MachineFilter - MachineObservationState - EventToolLifeType
    /// </summary>
    /// <param name="machineFilter">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="toolLifeEventType"></param>
    /// <returns></returns>
    IList<IEventToolLifeConfig> FindByKey(IMachineFilter machineFilter,
                                          IMachineObservationState mos,
                                          EventToolLifeType toolLifeEventType);
    
    /// <summary>
    /// Return the EventToolLifeConfig corresponding to the key
    /// MachineFilter - MachineObservationState
    /// </summary>
    /// <param name="machineFilter">null means all</param>
    /// <param name="mos">null means all</param>
    /// <returns></returns>
    IList<IEventToolLifeConfig> FindByKey(IMachineFilter machineFilter,
                                          IMachineObservationState mos);
    
    /// <summary>
    /// Return all EventToolLifeConfig corresponding to an event type
    /// </summary>
    /// <param name="toolLifeEventType"></param>
    /// <returns></returns>
    IList<IEventToolLifeConfig> FindAllByType(EventToolLifeType toolLifeEventType);
  }
}
