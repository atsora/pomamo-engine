// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Core.ModelDAO
{
  /// <summary>
  /// Implementation of <see cref="IDatabaseConnectionStatus"/> that returns that the database is always up
  /// </summary>
  public class DatabaseConnectionStatusValid: IDatabaseConnectionStatus
  {
    readonly ILog log = LogManager.GetLogger (typeof (DatabaseConnectionStatusValid).FullName);

    /// <summary>
    /// <see cref="IDatabaseConnectionStatus"/>
    /// </summary>
    public bool IsDatabaseConnectionUp => true;
  }
}
