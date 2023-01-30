// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Interface for classes that may return if the database connection is up or not
  /// 
  /// This is required for example before returning some extensions
  /// </summary>
  public interface IDatabaseConnectionStatus
  {
    /// <summary>
    /// Is the database connection up ?
    /// </summary>
    bool IsDatabaseConnectionUp { get; }
  }
}
