// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventCncValueDAO">IEventCncValueDAO</see>
  /// </summary>
  public class EventCncValueDAO
    : SaveOnlyNHibernateDAO<EventCncValue, IEventCncValue, int>
    , IEventCncValueDAO
  {
    /// <summary>
    /// Find all the EventCncValue corresponding to a specified config
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public IList<IEventCncValue> FindWithConfig (IEventCncValueConfig config)
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<EventCncValue>()
        .Add (Restrictions.Eq ("Config", config))
        .List<IEventCncValue>();
    }

    /// <summary>
    /// <see cref="IEventCncValueDAO"/>
    /// </summary>
    /// <param name="machineModuleIds">not null</param>
    /// <param name="range">not null</param>
    /// <returns>not null</returns>
    public async System.Threading.Tasks.Task<IList<IEventCncValue>> FindInRangeAsync (int[] machineModuleIds, UtcDateTimeRange range)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventCncValue> ()
        .Add (Restrictions.In ("MachineModule.Id", machineModuleIds));
      if (range.Lower.HasValue) {
        criteria = criteria.Add (range.LowerInclusive
          ? Restrictions.Ge ("DateTime", range.Lower.Value)
          : Restrictions.Gt ("DateTime", range.Lower.Value));
      }
      if (range.Upper.HasValue) {
        criteria = criteria.Add (range.UpperInclusive
          ? Restrictions.Le ("DateTime", range.Upper.Value)
          : Restrictions.Lt ("DateTime", range.Upper.Value));
      }
      return await criteria
        .AddOrder (Order.Asc ("DateTime"))
        .ListAsync<IEventCncValue> ();
    }
  }
}
