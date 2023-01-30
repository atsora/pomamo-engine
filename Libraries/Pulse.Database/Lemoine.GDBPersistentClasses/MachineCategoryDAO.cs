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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineCategoryDAO">IMachineCategoryDAO</see>
  /// </summary>
  public class MachineCategoryDAO
    : VersionableNHibernateDAO<MachineCategory, IMachineCategory, int>
    , IMachineCategoryDAO
  {
    /// <summary>
    /// Find all IMachineCategory ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IMachineCategory> FindAll ()
    {
      IList<IMachineCategory> categories = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineCategory> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachineCategory> ();
      return categories;
    }

    /// <summary>
    /// Find all IMachineCategory ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IMachineCategory>> FindAllAsync ()
    {
      IList<IMachineCategory> categories = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineCategory> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<IMachineCategory> ();
      return categories;
    }

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachineCategory> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineCategory> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<IMachineCategory> ();
    }
  }
}
