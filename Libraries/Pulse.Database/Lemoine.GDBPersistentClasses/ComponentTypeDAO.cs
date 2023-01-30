// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using NHibernate.Criterion;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ComponentTypeDAO.
  /// </summary>
  public class ComponentTypeDAO
    : VersionableNHibernateDAO<ComponentType, IComponentType, int>
    , IComponentTypeDAO
  {

    static readonly ILog log = LogManager.GetLogger (typeof (ComponentTypeDAO).FullName);

    /// <summary>
    /// Get all ComponentType ordered by name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IComponentType> FindAllOrderByName ()
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<ComponentType> ()
        .AddOrder (Order.Asc ("Name"))
        .AddOrder (Order.Asc ("Code"))
        .SetCacheable (true)
        .List<IComponentType> ();
    }

    /// <summary>
    /// <see cref="IComponentTypeDAO"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IComponentType FindByName (string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ComponentType> ()
        .Add (Restrictions.Eq ("Name", name))
        .UniqueResult<IComponentType> ();
    }

    /// <summary>
    /// <see cref="IComponentTypeDAO"/>
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public IComponentType FindByCode (string code)
    {
      Debug.Assert (!string.IsNullOrEmpty (code));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ComponentType> ()
        .Add (Restrictions.Eq ("Code", code))
        .UniqueResult<IComponentType> ();
    }
  }
}
