// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Database or ODBC exception
  /// </summary>
  public class DatabaseException : RepositoryException
  {
    /// <summary>
    /// <see cref="RepositoryException"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public DatabaseException (string message, Exception innerException)
      : base (message, innerException)
    {
    }
  }
}
