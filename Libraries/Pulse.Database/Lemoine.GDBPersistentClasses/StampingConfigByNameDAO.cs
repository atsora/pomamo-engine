// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IStampingConfigByNameDAODAO">IStampingConfigByNameDAODAO</see>
  /// </summary>
  public class StampingConfigByNameDAO
    : VersionableNHibernateDAO<StampingConfigByName, IStampingConfigByName, int>
    , IStampingConfigByNameDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public StampingConfigByNameDAO ()
      : base ()
    {
    }

    /// <summary>
    /// <see cref="IStampingConfigByNameDAO"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IStampingConfigByName FindByName (string name)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<StampingConfigByName> ()
        .Add (Restrictions.Eq ("Name", name))
        .UniqueResult<IStampingConfigByName> ();
    }

    /// <summary>
    /// <see cref="IStampingConfigByNameDAO"/>
    /// </summary>
    /// <returns></returns>
    public IList<IStampingConfigByName> FindAllForConfig ()
    {
      var stampingConfigs = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<StampingConfigByName> ()
        .Fetch (SelectMode.Fetch, "Config")
        .AddOrder (Order.Asc ("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IStampingConfigByName> ();
      foreach (var stampingConfig in stampingConfigs) {
        ModelDAOHelper.DAOFactory.Initialize (stampingConfig.Config);
      }
      return stampingConfigs;
    }
  }
}
