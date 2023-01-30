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
  /// Migration 1008: 
  /// </summary>
  [Migration (1008)]
  public class CreateProductionRateSummary : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CreateProductionStateSummary).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.PRODUCTION_RATE_SUMMARY,
                         new Column ($"{TableName.PRODUCTION_RATE_SUMMARY}id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ($"{TableName.PRODUCTION_RATE_SUMMARY}version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.DAY, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column ("productionratesummaryduration", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("productionratesummaryrate", DbType.Double, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.PRODUCTION_RATE_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_RATE_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.PRODUCTION_RATE_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict); // Because of the unicity key

      AddUniqueConstraint (TableName.PRODUCTION_RATE_SUMMARY,
                           ColumnName.MACHINE_ID,
                           ColumnName.DAY,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.SHIFT_ID);

      PartitionTable (TableName.PRODUCTION_RATE_SUMMARY, TableName.MONITORED_MACHINE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UnpartitionTable (TableName.PRODUCTION_RATE_SUMMARY);
      Database.RemoveTable (TableName.PRODUCTION_RATE_SUMMARY);
    }
  }
}
