// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Database.Migration
{
  /// <summary>
  /// Interface to implement to apply the database migrations before fully initializing the connection
  /// </summary>
  public interface IMigrationHelper
  {
    /// <summary>
    /// Connection string
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Migrate the database
    /// </summary>
    void Migrate ();
  }
}
