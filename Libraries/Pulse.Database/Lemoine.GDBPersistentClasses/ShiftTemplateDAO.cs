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
using NHibernate.Transform;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IShiftTemplateDAO">IShiftTemplateDAO</see>
  /// </summary>
  public class ShiftTemplateDAO
    : VersionableNHibernateDAO<ShiftTemplate, IShiftTemplate, int>
    , IShiftTemplateDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateDAO).FullName);

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IShiftTemplateDAO.FindAllForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns></returns>
    public IList<IShiftTemplate> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<ShiftTemplate>()
        .Fetch (SelectMode.Fetch, "Items")
        .Fetch (SelectMode.Fetch, "Items.Shift")
        .Fetch (SelectMode.Fetch, "Breaks")
        .SetResultTransformer(new DistinctRootEntityResultTransformer()) // Remove duplicate root entity
        .AddOrder(Order.Asc("Id"))
        .List<IShiftTemplate>();
    }
  }
}
