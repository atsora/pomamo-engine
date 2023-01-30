// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// IDataWithIdExtensions
  /// </summary>
  public static class IDataWithIdExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IDataWithIdExtensions).FullName);

    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <typeparam name="ID"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized<ID> (this IDataWithId<ID> data)
    {
      if (ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (data)) {
        return data.ToString ();
      }
      else {
        return $"[{data.GetType ().Name} {data.Id}]";
      }
    }
  }
}
