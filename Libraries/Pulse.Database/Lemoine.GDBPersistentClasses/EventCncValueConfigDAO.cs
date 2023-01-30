// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventCncValueConfigDAO">IEventCncValueConfigDAO</see>
  /// </summary>
  public class EventCncValueConfigDAO
    : VersionableNHibernateDAO<EventCncValueConfig, IEventCncValueConfig, int>
    , IEventCncValueConfigDAO
  {
    /// <summary>
    /// FindAll implementation
    /// 
    /// The result is sorted by:
    /// <item>MinDuration</item>
    /// </summary>
    /// <returns></returns>
    public override IList<IEventCncValueConfig> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventCncValueConfig> ()
        .AddOrder (Order.Asc ("MinDuration"))
        .List<IEventCncValueConfig> ();
    }

    /// <summary>
    /// FindAll implementation
    /// 
    /// The result is sorted by:
    /// <item>MinDuration</item>
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IEventCncValueConfig>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventCncValueConfig> ()
        .AddOrder (Order.Asc ("MinDuration"))
        .ListAsync<IEventCncValueConfig> ();
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.ModelDAO.IEventCncValueConfigDAO.FindAllForConfig">FindAllForConfig</see>
    /// 
    /// Note : use Fetch Eager Mode
    /// </summary>
    public IList<IEventCncValueConfig> FindAllForConfig(){
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventCncValueConfig> ()
        .Fetch (SelectMode.Fetch, "Level")
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .AddOrder (Order.Asc ("MinDuration"))
        .List<IEventCncValueConfig> ();
    }
    
    /// <summary>
    /// Find by name
    /// 
    /// Fetch early some of its properties
    /// 
    /// null if not found
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IEventCncValueConfig FindByName (string name)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventCncValueConfig> ()
        .Add (Restrictions.Eq ("Name", name))
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .Fetch (SelectMode.Fetch, "Field")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IEventCncValueConfig> ();
    }
    
    /// <summary>
    /// Get the used IEventLevel in EventCncValueConfig
    /// </summary>
    /// <returns></returns>
    public IList<IEventLevel> GetLevels ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("EventCncValueConfigLevels")
        .List<IEventLevel> ();
    }
  }
}
