// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ILogDAO">ILogDAO</see>
  /// </summary>
  public class LogDAO
    : ReadOnlyNHibernateDAO<Log, IBaseLog, int>
    , ILogDAO
  {
    /// <summary>
    /// Find the IBaseLog items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IList<IBaseLog> FindGreaterThan (int id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Log> ()
        .Add (Restrictions.Gt ("Id", id))
        .AddOrder (Order.Asc ("Id"))
        .List<IBaseLog> ();
    }

    /// <summary>
    /// Find the IBaseLog items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns></returns>
    public IList<IBaseLog> FindGreaterThan (int id, int maxResults)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Log> ()
        .Add (Restrictions.Gt ("Id", id))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxResults)
        .List<IBaseLog> ();
    }
  }
}
