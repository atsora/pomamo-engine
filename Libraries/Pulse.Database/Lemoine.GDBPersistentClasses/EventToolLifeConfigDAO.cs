// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventToolLifeConfigDAO">IEventToolLifeConfigDAO</see>
  /// </summary>
  public class EventToolLifeConfigDAO
    : VersionableNHibernateDAO<EventToolLifeConfig, IEventToolLifeConfig, int>
    , IEventToolLifeConfigDAO
  {
    /// <summary>
    /// FindAllForConfig implementation with EagerFetch
    /// </summary>
    /// <returns></returns>
    public IList<IEventToolLifeConfig> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<EventToolLifeConfig>()
        .AddOrder(Order.Asc("MachineFilter.Id"))
        .AddOrder(Order.Asc("MachineObservationState.Id"))
        .AddOrder(Order.Asc("Type"))
        .List<IEventToolLifeConfig>();
    }
    
    /// <summary>
    /// Get the used IEventLevel in EventToolLifeConfig
    /// </summary>
    /// <returns></returns>
    public IList<IEventLevel> GetLevels()
    {
      return NHibernateHelper.GetCurrentSession()
        .GetNamedQuery("EventToolLifeConfigLevels")
        .List<IEventLevel>();
    }
    
    /// <summary>
    /// Return the EventToolLifeConfig corresponding to the key
    /// MachineFilter - MachineObservationState - EventToolLifeType
    /// </summary>
    /// <param name="machineFilter">null means all</param>
    /// <param name="mos">null means all</param>
    /// <param name="toolLifeEventType"></param>
    /// <returns></returns>
    public IList<IEventToolLifeConfig> FindByKey(IMachineFilter machineFilter,
                                                 IMachineObservationState mos,
                                                 EventToolLifeType toolLifeEventType)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession().CreateCriteria<EventToolLifeConfig>();
      
      if (machineFilter != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineFilter", machineFilter));
      }

      if (mos != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineObservationState", mos));
      }

      criteria = criteria.Add(Restrictions.Eq("Type", toolLifeEventType));
      
      return criteria.List<IEventToolLifeConfig>();
    }
    
    /// <summary>
    /// Return the EventToolLifeConfig corresponding to the key
    /// MachineFilter - MachineObservationState - EventToolLifeType
    /// </summary>
    /// <param name="machineFilter">null means all</param>
    /// <param name="mos">null means all</param>
    /// <returns></returns>
    public IList<IEventToolLifeConfig> FindByKey(IMachineFilter machineFilter,
                                          IMachineObservationState mos)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession().CreateCriteria<EventToolLifeConfig>();
      
      if (machineFilter != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineFilter", machineFilter));
      }

      if (mos != null) {
        criteria = criteria.Add(Restrictions.Eq("MachineObservationState", mos));
      }

      return criteria.List<IEventToolLifeConfig>();
    }
    
    /// <summary>
    /// Return all EventToolLifeConfig corresponding to an event type
    /// </summary>
    /// <param name="toolLifeEventType"></param>
    /// <returns></returns>
    public IList<IEventToolLifeConfig> FindAllByType(EventToolLifeType toolLifeEventType)
    {
      return NHibernateHelper.GetCurrentSession().CreateCriteria<EventToolLifeConfig>()
        .Add(Restrictions.Eq("Type", toolLifeEventType))
        .List<IEventToolLifeConfig>();
    }
  }
}
