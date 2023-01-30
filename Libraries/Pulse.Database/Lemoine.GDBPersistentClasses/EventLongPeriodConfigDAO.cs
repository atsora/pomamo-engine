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
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventLongPeriodConfigDAO">IEventLongPeriodConfigDAO</see>
  /// </summary>
  public class EventLongPeriodConfigDAO
    : VersionableNHibernateDAO<EventLongPeriodConfig, IEventLongPeriodConfig, int>
    , IEventLongPeriodConfigDAO
  {
    /// <summary>
    /// FindAll implementation
    /// 
    /// The result is sorted by:
    /// <item>not null monitored machines first</item>
    /// <item>not null machine modes first</item>
    /// <item>not null machine observation states first</item>
    /// <item>trigger duration</item>
    /// 
    /// This query is cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IEventLongPeriodConfig> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventLongPeriodConfig> ()
        .AddOrder (Order.Asc ("MonitoredMachine.Id"))
        .AddOrder (Order.Asc ("MachineMode.Id"))
        .AddOrder (Order.Asc ("MachineObservationState.Id"))
        .AddOrder (Order.Asc ("TriggerDuration"))
        .SetCacheable (true)
        .List<IEventLongPeriodConfig> ();
    }

    /// <summary>
    /// FindAll implementation
    /// 
    /// The result is sorted by:
    /// <item>not null monitored machines first</item>
    /// <item>not null machine modes first</item>
    /// <item>not null machine observation states first</item>
    /// <item>trigger duration</item>
    /// 
    /// This query is cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IEventLongPeriodConfig>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventLongPeriodConfig> ()
        .AddOrder (Order.Asc ("MonitoredMachine.Id"))
        .AddOrder (Order.Asc ("MachineMode.Id"))
        .AddOrder (Order.Asc ("MachineObservationState.Id"))
        .AddOrder (Order.Asc ("TriggerDuration"))
        .SetCacheable (true)
        .ListAsync<IEventLongPeriodConfig> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IEventLongPeriodConfigDAO.FindAllForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns>IList&lt;IEventLongPeriodConfig&gt;</returns>
    public IList<IEventLongPeriodConfig> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventLongPeriodConfig> ()
        .Fetch (SelectMode.Fetch, "Level")
        .Fetch (SelectMode.Fetch, "MonitoredMachine")
        .Fetch (SelectMode.Fetch, "MonitoredMachine.MonitoringType")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "MachineObservationState")
        .AddOrder (Order.Asc ("MonitoredMachine.Id"))
        .AddOrder (Order.Asc ("MachineMode.Id"))
        .AddOrder (Order.Asc ("MachineObservationState.Id"))
        .AddOrder (Order.Asc ("TriggerDuration"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IEventLongPeriodConfig> ();
    }
    
    /// <summary>
    /// Get the used IEventLevel in EventLongPeriodConfig
    /// </summary>
    /// <returns></returns>
    public IList<IEventLevel> GetLevels ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("EventLongPeriodConfigLevels")
        .List<IEventLevel> ();
    }
    
    /// <summary>
    /// Return the EventLongPeriodConfig corresponding to the key
    /// MonitoredMachine - MachineMode - MachineObservationState - TriggerDuration
    /// </summary>
    /// <param name="monitoredMachine">null means all</param>
    /// <param name="machineMode">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IList<IEventLongPeriodConfig> FindByKey(IMonitoredMachine monitoredMachine,
                                                   IMachineMode machineMode,
                                                   IMachineObservationState mos,
                                                   TimeSpan duration)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession().CreateCriteria<EventLongPeriodConfig>();
      
      if (monitoredMachine != null) {
        criteria = criteria.Add(Restrictions.Eq("MonitoredMachine", monitoredMachine));
      }

      if (machineMode != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineMode", machineMode));
      }

      if (mos != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineObservationState", mos));
      }

      criteria = criteria.Add(Restrictions.Eq("TriggerDuration", duration));
      
      return criteria.List<IEventLongPeriodConfig>();
    }
    
    /// <summary>
    /// Return the EventLongPeriodConfig corresponding to the key
    /// MonitoredMachine - MachineMode - MachineObservationState - EventLevel
    /// </summary>
    /// <param name="monitoredMachine">null means all</param>
    /// <param name="machineMode">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="level"></param>
    /// <returns></returns>
    public IList<IEventLongPeriodConfig> FindByKey(IMonitoredMachine monitoredMachine,
                                                   IMachineMode machineMode,
                                                   IMachineObservationState mos,
                                                   IEventLevel level)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession().CreateCriteria<EventLongPeriodConfig>();
      
      if (monitoredMachine != null) {
        criteria = criteria.Add(Restrictions.Eq("MonitoredMachine", monitoredMachine));
      }

      if (machineMode != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineMode", machineMode));
      }

      if (mos != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineObservationState", mos));
      }

      criteria = criteria.Add(Restrictions.Eq("Level", level));
      
      return criteria.List<IEventLongPeriodConfig>();
    }
  }
}
