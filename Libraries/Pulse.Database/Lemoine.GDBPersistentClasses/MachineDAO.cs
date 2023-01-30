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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineDAO">IMachineDAO</see>
  /// </summary>
  public class MachineDAO
    : VersionableNHibernateDAO<Machine, IMachine, int>
    , IMachineDAO
  {
    /// <summary>
    /// Find all IMachine ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> FindAll ()
    {
      IList<IMachine> machines = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
      MachineCache.Add (machines);
      return machines;
    }

    /// <summary>
    /// Find all IMachine ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IMachine>> FindAllAsync ()
    {
      IList<IMachine> machines = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<IMachine> ();
      MachineCache.Add (machines);
      return machines;
    }

    /// <summary>
    /// Find all machines trying to use the cache first
    /// 
    /// Note that the machines may be detached
    /// </summary>
    /// <param name="useCache"></param>
    /// <returns></returns>
    public virtual IList<IMachine> FindAll (bool useCache)
    {
      if (useCache) {
        IList<IMachine> machines;
        if (MachineCache.TryGetMachines (out machines)) {
          return machines;
        }
      }
      
      return FindAll ();
    }

    /// <summary>
    /// Find all IMachine for XML serialization
    /// </summary>
    /// <returns></returns>
    public IList<IMachine> FindAllForXmlSerialization ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Fetch (SelectMode.Fetch, "MonitoringType")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachine> ();
    }

    /// <summary>
    /// Find all IMachine, but get also all its children
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachine> FindAllWithChildren ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Fetch (SelectMode.Fetch, "Company")
        .Fetch (SelectMode.Fetch, "Department")
        .Fetch (SelectMode.Fetch, "Cell")
        .Fetch (SelectMode.Fetch, "MachineCategory")
        .Fetch (SelectMode.Fetch, "MachineSubCategory")
        .Fetch (SelectMode.Fetch, "MonitoringType")
        .Fetch (SelectMode.Fetch, "StampingConfigByName")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachine> ();
    }
    
    /// <summary>
    /// Find an IMachine using its name <paramref name="machineName"/>
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machineName"></param>
    /// <returns></returns>
    public IMachine FindByName(string machineName) {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<Machine> ()
        .Add (Restrictions.Eq ("Name", machineName))
        .SetCacheable (true)
        .UniqueResult<IMachine> ();
    }
    
    /// <summary>
    /// Find all IMachine ordered by name
    /// </summary>
    /// <returns></returns>
    public IList<IMachine> FindAllOrderByName ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }
    
    /// <summary>
    /// Find all IMachine that are not obsolete
    /// </summary>
    /// <returns></returns>
    public IList<IMachine> FindAllNotObsolete()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<Machine>()
        .Add(Restrictions.Not(Restrictions.Eq("MonitoringType.Id", (int)MachineMonitoringTypeId.Obsolete)))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable(true)
        .List<IMachine>();
    }

    /// <summary>
    /// Find all IMachine with given company id
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public IList<IMachine> FindAllInCompany (int companyId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Add (Restrictions.Eq ("Company.Id", companyId))
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }

    /// <summary>
    /// Find all IMachine with given department id
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    public IList<IMachine> FindAllInDepartment (int departmentId){
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Add(Restrictions.Eq("Department.Id",departmentId))
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }
    
    /// <summary>
    /// Find all IMachine with given machine category id
    /// </summary>
    /// <param name="machineCategoryId"></param>
    /// <returns></returns>
    public IList<IMachine> FindAllInMachineCategory (int machineCategoryId){
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Add(Restrictions.Eq("Category.Id",machineCategoryId))
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }

    /// <summary>
    /// Find all IMachine with given machine sub-category id
    /// </summary>
    /// <param name="machineSubCategoryId"></param>
    /// <returns></returns>
    public IList<IMachine> FindAllInMachineSubCategory (int machineSubCategoryId) {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Add(Restrictions.Eq("SubCategory.Id",machineSubCategoryId))
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }

    /// <summary>
    /// Find all IMachine with given cell id
    /// </summary>
    /// <param name="cellId"></param>
    /// <returns></returns>
    public IList<IMachine> FindAllInCell (int cellId){
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Machine> ()
        .Add(Restrictions.Eq("Cell.Id",cellId))
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachine> ();
    }
  }
}
