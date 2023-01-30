// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for a Generic DAO
  /// for tables that are partitioned by line
  /// 
  /// By default, an entity can't be updated, only saved.
  /// If you want your entity to be updated, please use IGenericByLineUpdateDAO instead.
  /// </summary>
  public interface IGenericByLineDAO<T, ID>
    : IBaseGenericDAO<T, ID>
    where T: IPartitionedByLine
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    T FindById (ID id, ILine line);

    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lineId"></param>
    /// <returns></returns>
    T FindById (ID id, int lineId);
  }
}
