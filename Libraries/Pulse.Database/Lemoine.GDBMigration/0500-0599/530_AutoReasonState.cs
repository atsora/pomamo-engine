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
  /// Migration 530: add a new table autoreasonstate
  /// </summary>
  [Migration (530)]
  public class AddAutoReasonState : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddAutoReasonState).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.AUTO_REASON_STATE,
                         new Column (TableName.AUTO_REASON_STATE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.AUTO_REASON_STATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.AUTO_REASON_STATE + "key", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                         new Column (TableName.AUTO_REASON_STATE + "value", DbType.String, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.AUTO_REASON_STATE, ColumnName.MACHINE_ID, 
        TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      PartitionTable (TableName.AUTO_REASON_STATE, TableName.MONITORED_MACHINE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UnpartitionTable (TableName.AUTO_REASON_STATE);
      Database.RemoveTable (TableName.AUTO_REASON_STATE);
    }
  }
}
