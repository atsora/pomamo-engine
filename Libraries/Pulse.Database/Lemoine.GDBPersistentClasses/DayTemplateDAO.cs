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
using NHibernate.Transform;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IDayTemplateDAO">IDayTemplateDAO</see>
  /// </summary>
  public class DayTemplateDAO
    : VersionableNHibernateDAO<DayTemplate, IDayTemplate, int>
    , IDayTemplateDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateDAO).FullName);

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IShiftTemplateDAO.FindAllForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns></returns>
    public IList<IDayTemplate> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<IDayTemplate>()
        .Fetch (SelectMode.Fetch, "Items")
        .SetResultTransformer(new DistinctRootEntityResultTransformer()) // Remove duplicate root entity
        .AddOrder(Order.Asc("Id"))
        .List<IDayTemplate>();
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override IDayTemplate MakePersistent (IDayTemplate entity)
    {
      IDayTemplate result = base.MakePersistent (entity);
      foreach (var item in entity.Items) {
        NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdate (item);
      }
      return result;
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override async Task<IDayTemplate> MakePersistentAsync (IDayTemplate entity)
    {
      IDayTemplate result = await base.MakePersistentAsync (entity);
      foreach (var item in entity.Items) {
        await NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdateAsync (item);
      }
      return result;
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient(IDayTemplate entity)
    {
      foreach (var item in entity.Items) {
        NHibernateHelper.GetCurrentSession ()
          .Delete (item);
      }
      entity.Items.Clear ();
      base.MakeTransient(entity);
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IDayTemplate entity)
    {
      foreach (var item in entity.Items) {
        await NHibernateHelper.GetCurrentSession ()
          .DeleteAsync (item);
      }
      entity.Items.Clear ();
      await base.MakeTransientAsync (entity);
    }
  }
}
