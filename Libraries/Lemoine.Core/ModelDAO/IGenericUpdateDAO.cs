// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO for entities that may be updated
  /// for not-partitioned tables
  /// </summary>
  public interface IGenericUpdateDAO<T, ID>
    : IBaseGenericUpdateDAO<T, ID>, IGenericDAO<T, ID>
    where T: Lemoine.Model.IDataWithVersion
  {
    /// <summary>
    /// Find an item T by Id and lock it for an upgrade
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T FindByIdAndLock (ID id);
  }
}
