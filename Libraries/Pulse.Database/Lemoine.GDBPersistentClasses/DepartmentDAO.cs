// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDepartmentDAO">IDepartmentDAO</see>
  /// </summary>
  public class DepartmentDAO
    : VersionableNHibernateDAO<Department, IDepartment, int>
    , IDepartmentDAO
  {
    /// <summary>
    /// Find all IDepartment ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IDepartment> FindAll ()
    {
      IList<IDepartment> departments = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Department> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IDepartment> ();
      return departments;
    }

    /// <summary>
    /// Find all IDepartment ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IDepartment>> FindAllAsync ()
    {
      IList<IDepartment> departments = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Department> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<IDepartment> ();
      return departments;
    }

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IDepartment> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Department> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<IDepartment> ();
    }
  }
}
