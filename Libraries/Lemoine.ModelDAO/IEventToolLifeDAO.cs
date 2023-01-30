// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventToolLife.
  /// </summary>
  public interface IEventToolLifeDAO : IGenericByMachineModuleDAO<IEventToolLife, int>
  {
    /// <summary>
    /// Find the last event occuring for a tool
    /// </summary>
    /// <param name="machineModule">machine module handling the tool (not null)</param>
    /// <param name="eventToolLifeType">type of the event to find</param>
    /// <param name="toolId">tool having triggered the event</param>
    /// <returns></returns>
    IEventToolLife FindLastDateTimeOfAnEvent(IMachineModule machineModule,
                                             EventToolLifeType eventToolLifeType,
                                             string toolId);

    /// <summary>
    /// Get the last tool life event of a list of machine modules
    /// (pertaining for example to the same machine)
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <returns></returns>
    IEventToolLife GetLast(int[] machineModuleIds);

    /// <summary>
    /// Find the next event occuring after startEventId
    /// for specific types of tool life event and a list of machine modules
    /// (pertaining for example to the same machine)
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <param name="eventToolLifeTypes"></param>
    /// <param name="startEventId"></param>
    /// <returns></returns>
    IEventToolLife FindNextEventByType(int[] machineModuleIds,
                                       EventToolLifeType[] eventToolLifeTypes,
                                       int startEventId);

    /// <summary>
    /// Find a specific kind of events in a range, for a tool
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <param name="eventToolLifeType"></param>
    /// <param name="dateTimeRange"></param>
    /// <param name="toolId"></param>
    /// <returns></returns>
    IList<IEventToolLife> FindEventsByType (int[] machineModuleIds,
                                            EventToolLifeType eventToolLifeType,
                                            UtcDateTimeRange dateTimeRange,
                                            string toolId);
  }
}
