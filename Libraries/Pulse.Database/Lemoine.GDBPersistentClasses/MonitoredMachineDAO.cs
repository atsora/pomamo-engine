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
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="IMonitoredMachineDAO">IMonitoredMachineDAO</see>
  /// </summary>
  public class MonitoredMachineDAO
    : VersionableNHibernateDAO<MonitoredMachine, IMonitoredMachine, int>
    , IMonitoredMachineDAO
  {
    /// <summary>
    /// Find by Id with an eager fetch of the monitoring type
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns></returns>
    public IMonitoredMachine FindByIdForXmlSerialization (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.IdEq (id))
        .Fetch (SelectMode.Fetch, "MonitoringType")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find by Id with an eager fetch of the machine modules
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns></returns>
    public IMonitoredMachine FindByIdWithMachineModules (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.IdEq (id))
        .Fetch (SelectMode.Fetch, "MachineModules")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find by Id with an eager fetch of:
    /// <item>the main machine module</item>
    /// <item>the performance field</item>
    /// <item>the unit of the performance field</item>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IMonitoredMachine FindByIdWithMainMachineModulePerformanceFieldUnit (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.IdEq (id))
        .Fetch (SelectMode.Fetch, "MainMachineModule")
        .Fetch (SelectMode.Fetch, "PerformanceField")
        .Fetch (SelectMode.Fetch, "PerformanceField.Unit")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IMonitoredMachine> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .SetCacheable (true)
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IMonitoredMachine>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .SetCacheable (true)
        .ListAsync<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine for XML serialization
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllForXmlSerialization ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Fetch (SelectMode.Fetch, "MonitoringType")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine, but get also all its MachineModule child elements
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllWithMachineModule ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Fetch (SelectMode.Fetch, "MachineModules")
        .Fetch (SelectMode.Fetch, "MonitoringType")
        // Note: without the following line, some rows are duplicated.Add Is it a bug ?
        .SetResultTransformer (NHibernate.Transform.Transformers.DistinctRootEntity)
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine, but get also all its Cnc Acquisition child elements
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllWithCncAcquisition ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Fetch (SelectMode.Fetch, "MonitoringType")
        .Fetch (SelectMode.Fetch, "MainMachineModule")
        .Fetch (SelectMode.Fetch, "MainMachineModule.CncAcquisition")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine to configure them.
    /// 
    /// That means get also:
    /// <item>all its Cnc Acquisition child elements</item>
    /// <item>its performance field</item>
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllForConfig ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Fetch (SelectMode.Fetch, "MonitoringType")
        .Fetch (SelectMode.Fetch, "MainMachineModule")
        .Fetch (SelectMode.Fetch, "MainMachineModule.CncAcquisition")
        .Fetch (SelectMode.Fetch, "PerformanceField")
        .Fetch (SelectMode.Fetch, "StampingConfigByName")
        .AddOrder (Order.Asc ("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find the IMonitoredMachine corresponding to a machine name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machineName"></param>
    /// <returns></returns>
    public IMonitoredMachine FindByName (string machineName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Add (Restrictions.Eq ("Name", machineName))
        .SetCacheable (true)
        .UniqueResult<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find the IMonitoredMachine corresponding to a machine
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IMonitoredMachine FindByMachine (IMachine machine)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.IdEq (machine.Id))
        .SetCacheable (true)
        .UniqueResult<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine ordered by name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllOrderByName ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine in a given department with a not-null company
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllByDepartment (int departmentId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Add (Restrictions.Eq ("Department.Id", departmentId))
        .Add (Restrictions.IsNotNull ("Company"))
        .SetCacheable (true)
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all IMonitoredMachine in a given category with a not-null company
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindAllByCategory (int categoryId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Add (Restrictions.Eq ("Category.Id", categoryId))
        .Add (Restrictions.IsNotNull ("Company"))
        .SetCacheable (true)
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// <see cref="IMonitoredMachineDAO"/>
    /// </summary>
    /// <param name="stampingConfigByName"></param>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindByStampingConfig (IStampingConfigByName stampingConfigByName)
    {
      Debug.Assert (null != stampingConfigByName);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ()
        .Add (Restrictions.Eq ("StampingConfigByName.Id", stampingConfigByName.Id))
        .SetCacheable (true)
        .List<IMonitoredMachine> ();
    }
  }
}
