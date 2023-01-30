// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO for entities that may be updated
  /// for tables that are partitioned by User
  /// </summary>
  public interface IGenericByUserUpdateDAO<T, ID>
    : IBaseGenericUpdateDAO<T, ID>, IGenericByUserDAO<T, ID>
    where T: Lemoine.Model.IDataWithVersion, Lemoine.Model.IPartitionedByUser
  {
    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, IUser user);

    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id, int userId);
  }
}
