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
  }
}
