// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 269: deprecated
  /// </summary>
  [Migration (269)]
  public class SfkcfgsView : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SfkcfgsView).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // sfkcfgs overwrite with day_cutoff is not required any more
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
