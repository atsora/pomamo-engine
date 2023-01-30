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
  /// Implementation of <see cref="Lemoine.ModelDAO.ICompanyDAO">ICompanyDAO</see>
  /// </summary>
  public class CompanyDAO
    : VersionableNHibernateDAO<Company, ICompany, int>
    , ICompanyDAO
  {
    /// <summary>
    /// Find all ICompany ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<ICompany> FindAll ()
    {
      IList<ICompany> companies = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Company> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<ICompany> ();
      return companies;
    }

    /// <summary>
    /// Find all ICompany ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<ICompany>> FindAllAsync ()
    {
      IList<ICompany> companies = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Company> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<ICompany> ();
      return companies;
    }

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<ICompany> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Company> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<ICompany> ();
    }
  }
}
