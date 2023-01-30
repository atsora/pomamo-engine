// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Generic NHibernate DAO
  /// for tables that are not partitioned
  /// </summary>
  public abstract class GenericNHibernateDAO<T, I, ID>
    : BaseGenericNHibernateDAO<T, I, ID>, IGenericDAO<I, ID>
    where T: class, I
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GenericNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public I FindById (ID id)
    {
      return NHibernateHelper.GetCurrentSession ()
        .Get<T> (id);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<I> FindByIdAsync (ID id)
    {
      return await NHibernateHelper.GetCurrentSession ()
        .GetAsync<T> (id);
    }
  }
}
