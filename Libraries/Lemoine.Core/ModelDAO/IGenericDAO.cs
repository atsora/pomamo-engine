// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO for not-partitioned tables
  /// 
  /// By default, an entity can't be updated, only saved.
  /// If you want your entity to be updated, please use IGenericUpdateDAO instead.
  /// </summary>
  public interface IGenericDAO<T, ID>
    : IBaseGenericDAO<T, ID>
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T FindById (ID id);

    /// <summary>
    /// Find an item T by Id asynchronously
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<T> FindByIdAsync (ID id);
  }
}
