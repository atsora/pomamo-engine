// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IBaseLog.
  /// </summary>
  public interface ILogDAO: IGenericDAO<IBaseLog, int>
  {
    /// <summary>
    /// Find the IBaseLog items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IList<IBaseLog> FindGreaterThan (int id);

    /// <summary>
    /// Find the IBaseLog items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns></returns>
    IList<IBaseLog> FindGreaterThan (int id, int maxResults);
  }
}
