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
  /// Implementation of <see cref="Lemoine.ModelDAO.ICellDAO">ICellDAO</see>
  /// </summary>
  public class CellDAO
    : VersionableNHibernateDAO<Cell, ICell, int>
    , ICellDAO
  {
    /// <summary>
    /// Find all ICell ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override IList<ICell> FindAll ()
    {
      IList<ICell> cells = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Cell> ()
        .AddOrder(Order.Asc("DisplayPriority"))
        .AddOrder(Order.Asc("Code"))
        .AddOrder(Order.Asc("Name"))
        .SetCacheable (true)
        .List<ICell> ();
      return cells;
    }

    /// <summary>
    /// Find all ICell ordered by displaypriority / code / name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<ICell>> FindAllAsync ()
    {
      IList<ICell> cells = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Cell> ()
        .AddOrder (Order.Asc ("DisplayPriority"))
        .AddOrder (Order.Asc ("Code"))
        .AddOrder (Order.Asc ("Name"))
        .SetCacheable (true)
        .ListAsync<ICell> ();
      return cells;
    }

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<ICell> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Cell> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<ICell> ();
    }
  }
}
