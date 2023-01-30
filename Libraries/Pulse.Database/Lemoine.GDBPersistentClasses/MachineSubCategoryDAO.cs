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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineSubCategoryDAO">IMachineSubCategoryDAO</see>
  /// </summary>
  public class MachineSubCategoryDAO
    : VersionableNHibernateDAO<MachineSubCategory, IMachineSubCategory, int>
    , IMachineSubCategoryDAO
  {
    /// <summary>
    /// Find all IMachineSubCategory ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<IMachineSubCategory> FindAll ()
    {
      IList<IMachineSubCategory> subCategories = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineSubCategory> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<IMachineSubCategory> ();
      return subCategories;
    }

    /// <summary>
    /// Find all IMachineSubCategory ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IMachineSubCategory>> FindAllAsync ()
    {
      IList<IMachineSubCategory> subCategories = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineSubCategory> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<IMachineSubCategory> ();
      return subCategories;
    }

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachineSubCategory> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineSubCategory> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<IMachineSubCategory> ();
    }
  }
}
