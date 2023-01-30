// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 602:
  /// </summary>
  [Migration(602)]
  public class OperationCycleStatusNotNul: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleStatusNotNul).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
UPDATE operationcycle
SET operationcyclestatus=0
WHERE operationcyclestatus IS NULL");
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_CYCLE,
                                               "status"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} DROP NOT NULL",
                                               TableName.OPERATION_CYCLE,
                                               "status"));
      Database.ExecuteNonQuery (@"
UPDATE operationcycle
SET operationcyclestatus=NULL
WHERE operationcyclestatus=0");
    }
  }
}
