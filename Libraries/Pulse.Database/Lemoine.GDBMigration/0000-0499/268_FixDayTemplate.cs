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
  /// Migration 268:
  /// </summary>
  [Migration(268)]
  public class FixDayTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixDayTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.DAY_TEMPLATE_ITEM,
                                               ColumnName.DAY_TEMPLATE_ID));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
