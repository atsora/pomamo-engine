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
  /// Migration 1101: Add the table sequence milestone 
  /// </summary>
  [Migration (1101)]
  public class SequenceMilestone : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SequenceMilestone).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SEQUENCE_MILESTONE,
                        new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column (TableName.SEQUENCE_MILESTONE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column (TableName.SEQUENCE_MILESTONE + "datetime", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                        new Column (ColumnName.SEQUENCE_ID, DbType.Int32),
                        new Column (TableName.SEQUENCE_MILESTONE + "seconds", DbType.Int32, ColumnProperty.NotNull),
                        new Column (TableName.SEQUENCE_MILESTONE + "completed", DbType.Boolean, ColumnProperty.NotNull, "FALSE")
                       );
      Database.GenerateForeignKey (TableName.SEQUENCE_MILESTONE, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.SEQUENCE_MILESTONE);
    }
  }
}
