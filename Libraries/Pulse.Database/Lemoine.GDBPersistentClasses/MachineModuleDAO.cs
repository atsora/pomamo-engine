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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModuleDAO">IMachineModuleDAO</see>
  /// </summary>
  public class MachineModuleDAO
    : VersionableNHibernateDAO<MachineModule, IMachineModule, int>
    , IMachineModuleDAO
  {
    /// <summary>
    /// Get the machine module from its id, with an eager fetch of the monitored machine and all its machine modules
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IMachineModule FindByIdWithMonitoredMachine (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModule> ()
        .Add (Restrictions.IdEq (id))
        .Fetch (SelectMode.Fetch, "MonitoredMachine")
        .Fetch (SelectMode.Fetch, "MonitoredMachine.MachineModules")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IMachineModule> ();
    }
    
    /// <summary>
    /// Find all IMachineModule to configure them.
    /// 
    /// That means get also:
    /// <item>its Cnc Acquisition</item>
    /// <item>its Monitored Machine</item>
    /// </summary>
    /// <returns></returns>
    public IList<IMachineModule> FindAllForConfig ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModule> ()
        .Fetch (SelectMode.Fetch, "MonitoredMachine")
        .Fetch (SelectMode.Fetch, "MonitoredMachine.MonitoringType")
        .Fetch (SelectMode.Fetch, "CncAcquisition")
        .AddOrder (Order.Asc ("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachineModule> ();
    }

    /// <summary>
    /// Find all IMachineModule with an eager fetch of the monitored machine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachineModule> FindAllWithMonitoredMachine ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModule> ()
        .Fetch (SelectMode.Fetch, "MonitoredMachine")
        .Fetch (SelectMode.Fetch, "MonitoredMachine.MonitoringType")
        .AddOrder (Order.Asc ("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachineModule> ();
    }
  }
}
