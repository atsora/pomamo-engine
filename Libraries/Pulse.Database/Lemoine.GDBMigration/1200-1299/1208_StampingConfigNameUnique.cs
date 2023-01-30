// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1208: stampingconfigname is unique 
  /// </summary>
  [Migration (1208)]
  public class StampingConfigNameUnique : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (StampingConfigNameUnique).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddUniqueConstraint (TableName.STAMPING_CONFIG_BY_NAME, "stampingconfigname");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
