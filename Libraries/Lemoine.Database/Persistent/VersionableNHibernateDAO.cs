// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Generic NHiberate DAO for the IVersionable classes
  /// for tables that are not partitioned
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class VersionableNHibernateDAO<T, I, ID>
    : BaseVersionableNHibernateDAO<T, I, ID>, IGenericUpdateDAO<I, ID>
    where T: class, I, Lemoine.Collections.IDataWithId<ID>
    where I: Lemoine.Model.IDataWithVersion
  {
    static readonly ILog log = LogManager.GetLogger(typeof (VersionableNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual I FindById (ID id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .Get<T> (id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async virtual System.Threading.Tasks.Task<I> FindByIdAsync (ID id)
    {
      return await NHibernateHelper.GetCurrentSession ()
        .GetAsync<T> (id);
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual I FindByIdAndLock (ID id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .Get<T> (id, NHibernate.LockMode.Upgrade);
    }
  }
}
