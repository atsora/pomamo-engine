// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Generic NHiberate DAO for the read-only classes, on which no update is possible
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class SaveOnlyNHibernateDAO<T, I, ID>
    : GenericNHibernateDAO<T, I, ID>
    where T: class, I
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SaveOnlyNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override I MakePersistent (I entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Save (entity);
      return entity;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<I> MakePersistentAsync (I entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .SaveAsync (entity);
      return entity;
    }
  }
}
