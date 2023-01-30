// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO for entities that may be updated
  /// that is independent whether the table is partitioned or not
  /// </summary>
  public interface IBaseGenericUpdateDAO<T, ID>: IBaseGenericDAO<T, ID>
    where T: Lemoine.Model.IDataWithVersion
  {
    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// 
    /// Do not use it in a serializable transaction
    /// 
    /// Please note this is pretty inefficient when the machine is partitioned
    /// (the foreign key is not used for the moment)
    /// </summary>
    /// <param name="entity"></param>
    void UpgradeLock (T entity);
    
    /// <summary>
    /// Reload the object (for example after an update operation fails or because it was changed somewhere else))
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    T Reload (T entity);
  }
}
