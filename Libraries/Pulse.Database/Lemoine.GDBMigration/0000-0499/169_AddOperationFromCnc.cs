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
  /// Migration 169: add an operationfromcnc column to the monitored machine table,
  /// which is a boolean indicating whether the operation information comes from the CNC or not.
  /// For the migration, the value is set to true.
  /// </summary>
  [Migration(169)]
  public class AddOperationFromCnc: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationFromCnc).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE,
                          new Column("operationfromcnc",
                                     DbType.Boolean,
                                     ColumnProperty.NotNull,
                                     true));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE monitoredmachine " +
                                "DROP COLUMN operationfromcnc;");

    }
  }
}
