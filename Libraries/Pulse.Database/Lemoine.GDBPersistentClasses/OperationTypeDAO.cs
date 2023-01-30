// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;


namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationTypeDAO">IOperationTypeDAO</see>
  /// </summary>
  public class OperationTypeDAO
    : VersionableNHibernateDAO<OperationType, IOperationType, int>
    , IOperationTypeDAO
  {
    /// <summary>
    /// Get All OperationType ordered by name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IOperationType> FindAllOrderByName()
    {
      return NHibernateHelper.GetCurrentSession().CreateCriteria<OperationType>()
        .AddOrder(Order.Asc("Name"))
        .AddOrder(Order.Asc("Code"))
        .SetCacheable (true)
        .List<IOperationType>();
    }

  }
}
