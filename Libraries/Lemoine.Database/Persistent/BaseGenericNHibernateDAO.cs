// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Threading.Tasks;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Base Generic NHibernate DAO
  /// that is independent whether the machine is partitioned or not
  /// </summary>
  public abstract class BaseGenericNHibernateDAO<T, I, ID>
    : IBaseGenericDAO<I, ID>
    where T: class, I
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BaseGenericNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// Does the current session contain the persistent instance?
    /// </summary>
    /// <param name="persistent"></param>
    /// <returns></returns>
    public virtual bool IsAttachedToSession (I persistent) => NHibernateHelper.GetCurrentSession ().Contains (persistent);

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public virtual IList<I> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .List<I> ();
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IList<I>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<T> ()
        .ListAsync<I> ();
    }

    /// <summary>
    /// MakePersistent implementation
    /// 
    /// To be implemented by the super-class
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public abstract I MakePersistent (I entity);

    /// <summary>
    /// MakePersistent implementation
    /// 
    /// To be implemented by the super-class
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public abstract Task<I> MakePersistentAsync (I entity);

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="entity"></param>
    public virtual void MakeTransient (I entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Delete (entity);
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="entity"></param>
    public virtual async System.Threading.Tasks.Task MakeTransientAsync (I entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .DeleteAsync (entity);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    public virtual void Lock (I entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Lock (entity, NHibernate.LockMode.None);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    public virtual async System.Threading.Tasks.Task LockAsync (I entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .LockAsync (entity, NHibernate.LockMode.None);
    }
  }
}
