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
  /// Migration 153: Add the new betweencycles table
  /// </summary>
  [Migration(153)]
  public class AddBetweenCycles: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddBetweenCycles).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.BETWEEN_CYCLES,
                         new Column (TableName.BETWEEN_CYCLES + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.BETWEEN_CYCLES + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.BETWEEN_CYCLES + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.BETWEEN_CYCLES + "end", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("previouscycleid", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("nextcycleid", DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.BETWEEN_CYCLES + "offsetduration", DbType.Double, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.BETWEEN_CYCLES, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.BETWEEN_CYCLES, "previouscycleid",
                                   TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.BETWEEN_CYCLES, "nextcycleid",
                                   TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // begin < end
      Database.AddCheckConstraint (TableName.BETWEEN_CYCLES + "period",
                                   TableName.BETWEEN_CYCLES,
                                   string.Format ("{0} < {1}",
                                                  TableName.BETWEEN_CYCLES + "begin",
                                                  TableName.BETWEEN_CYCLES + "end"));
      // Index / unique constraints
      AddUniqueConstraint (TableName.BETWEEN_CYCLES,
                           ColumnName.MACHINE_ID,
                           TableName.BETWEEN_CYCLES + "begin");
      AddIndex (TableName.BETWEEN_CYCLES,
                ColumnName.MACHINE_ID,
                TableName.BETWEEN_CYCLES + "end",
                TableName.BETWEEN_CYCLES + "begin");
      AddUniqueConstraint (TableName.BETWEEN_CYCLES,
                           "previouscycleid");
      AddUniqueConstraint (TableName.BETWEEN_CYCLES,
                           "nextcycleid");
      AddIndex (TableName.BETWEEN_CYCLES,
                TableName.BETWEEN_CYCLES + "offsetduration");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.BETWEEN_CYCLES);
    }
  }
}
