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
  /// Migration 280:
  /// </summary>
  [Migration(280)]
  public class AddDayNullShiftMachineAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDayNullShiftMachineAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.SHIFT_MACHINE_ASSOCIATION,
                                               ColumnName.DAY));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.SHIFT_MACHINE_ASSOCIATION,
                                               ColumnName.DAY));
    }
  }
}
