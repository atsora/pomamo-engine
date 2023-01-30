// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// 
  /// </summary>
  public interface IDatabaseConnection
  {
    /// <summary>
    /// Open a IDAOSession
    /// </summary>
    /// <returns></returns>
    IDAOSession OpenSession ();

    /// <summary>
    /// Get the PostgreSQL server version number, for example: 90501
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request fails or if the result is not a valid integer</exception>
    int GetPostgreSQLVersionNum ();

    /// <summary>
    /// Check if an proxy object has been initialized / is not lazy
    /// </summary>
    /// <param name="proxy"></param>
    /// <returns></returns>
    bool IsInitialized (object proxy);
  }
}
