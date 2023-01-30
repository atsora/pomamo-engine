// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Base Interface for a Generic DAO
  /// that is independent whether the table is partitioned or not
  /// 
  /// By default, an entity can't be updated, only saved.
  /// If you want your entity to be updated, please use IGenericUpdateDAO instead.
  /// </summary>
  public interface IBaseGenericDAO<T, ID>
  {
    /// <summary>
    /// Find all the entities
    /// </summary>
    /// <returns></returns>
    IList<T> FindAll ();
    
    /// <summary>
    /// Make an entity persistent
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    T MakePersistent (T entity);
    
    /// <summary>
    /// Make an entity transient
    /// </summary>
    /// <param name="entity"></param>
    void MakeTransient (T entity);
    
    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    void Lock (T entity);

    /// <summary>
    /// Find all the entities
    /// </summary>
    /// <returns></returns>
    Task<IList<T>> FindAllAsync ();

    /// <summary>
    /// Make an entity persistent
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<T> MakePersistentAsync (T entity);

    /// <summary>
    /// Make an entity transient
    /// </summary>
    /// <param name="entity"></param>
    Task MakeTransientAsync (T entity);

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="entity"></param>
    Task LockAsync (T entity);
  }
}
