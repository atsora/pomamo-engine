// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventToolLifeDAO">IEventToolLifeDAO</see>
  /// </summary>
  public class EventToolLifeDAO
    : SaveOnlyByMachineModuleNHibernateDAO<EventToolLife, IEventToolLife, int>
    , IEventToolLifeDAO
  {
    /// <summary>
    /// Find the last event occuring for a specific tool and a specific type of event
    /// </summary>
    /// <param name="machineModule">machine module handling the tool (not null)</param>
    /// <param name="eventToolLifeType">type of the event to find</param>
    /// <param name="toolId">tool having triggered the event</param>
    /// <returns></returns>
    public IEventToolLife FindLastDateTimeOfAnEvent (IMachineModule machineModule,
                                                    EventToolLifeType eventToolLifeType,
                                                    string toolId)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventToolLife> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("EventType", eventToolLifeType))
        .Add (Restrictions.Eq ("ToolId", toolId))
        .AddOrder (Order.Desc ("DateTime")) // Order by DateTime
        .SetMaxResults (1)
        .UniqueResult<IEventToolLife> ();
    }

    /// <summary>
    /// Get the last tool life event of a list of machine modules
    /// (pertaining for example to the same machine)
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <returns></returns>
    public IEventToolLife GetLast (int[] machineModuleIds)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventToolLife> ()
        .Add (Restrictions.In ("MachineModule.Id", machineModuleIds))
        .AddOrder (Order.Desc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IEventToolLife> ();
    }

    /// <summary>
    /// Find the next event occuring after startEventId
    /// for a specific type of tool life event and a list of machine modules
    /// (pertaining for example to the same machine)
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <param name="eventToolLifeTypes"></param>
    /// <param name="startEventId"></param>
    /// <returns></returns>
    public IEventToolLife FindNextEventByType (int[] machineModuleIds,
                                              EventToolLifeType[] eventToolLifeTypes,
                                              int startEventId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventToolLife> ()
        .Add (Restrictions.In ("MachineModule.Id", machineModuleIds))
        .Add (Restrictions.In ("EventType", eventToolLifeTypes))
        .Add (Restrictions.Gt ("Id", startEventId))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IEventToolLife> ();
    }

    /// <summary>
    /// Find a specific kind of events in a range, for a tool
    /// </summary>
    /// <param name="machineModuleIds"></param>
    /// <param name="eventToolLifeType"></param>
    /// <param name="dateTimeRange"></param>
    /// <param name="toolId"></param>
    /// <returns></returns>
    public IList<IEventToolLife> FindEventsByType (int[] machineModuleIds,
                                                   EventToolLifeType eventToolLifeType,
                                                   UtcDateTimeRange dateTimeRange,
                                                   string toolId)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventToolLife> ()
        .Add (Restrictions.In ("MachineModule.Id", machineModuleIds))
        .Add (Restrictions.Eq ("EventType", eventToolLifeType))
        .Add (Restrictions.Eq ("ToolId", toolId));

      if (dateTimeRange.Lower.HasValue) {
        if (dateTimeRange.LowerInclusive) {
          criteria = criteria.Add (Restrictions.Ge ("DateTime", dateTimeRange.Lower.Value));
        }
        else {
          criteria = criteria.Add (Restrictions.Gt ("DateTime", dateTimeRange.Lower.Value));
        }
      }

      if (dateTimeRange.Upper.HasValue) {
        if (dateTimeRange.UpperInclusive) {
          criteria = criteria.Add (Restrictions.Le ("DateTime", dateTimeRange.Upper.Value));
        }
        else {
          criteria = criteria.Add (Restrictions.Lt ("DateTime", dateTimeRange.Upper.Value));
        }
      }

      return criteria.List<IEventToolLife> ();
    }
  }
}
