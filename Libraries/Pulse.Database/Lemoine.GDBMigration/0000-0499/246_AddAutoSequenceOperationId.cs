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
  /// Migration 246:
  /// </summary>
  [Migration(246)]
  public class AddAutoSequenceOperationId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAutoSequenceOperationId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.AUTO_SEQUENCE,
                          new Column (ColumnName.OPERATION_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.AUTO_SEQUENCE, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.AUTO_SEQUENCE,
                                               ColumnName.SEQUENCE_ID));
      AddOneNotNullConstraint (TableName.AUTO_SEQUENCE, ColumnName.SEQUENCE_ID, ColumnName.OPERATION_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.AUTO_SEQUENCE,
                             ColumnName.OPERATION_ID);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.AUTO_SEQUENCE,
                                               ColumnName.SEQUENCE_ID));
    }
  }
}
