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
  /// Migration 282: Add the productionanalysisstatus table
  /// </summary>
  [Migration(282)]
  public class AddProductionAnalysisStatus: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddProductionAnalysisStatus).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.PRODUCTION_ANALYSIS_STATUS,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (TableName.PRODUCTION_ANALYSIS_STATUS + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("productionanalysisdatetime", DbType.DateTime));
      Database.GenerateForeignKey (TableName.PRODUCTION_ANALYSIS_STATUS, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {1}
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                               TableName.PRODUCTION_ANALYSIS_STATUS,
                                               "productionanalysisdatetime"));
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {1}=now() AT TIME ZONE 'UTC';",
                                               TableName.PRODUCTION_ANALYSIS_STATUS,
                                               "productionanalysisdatetime"));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {1} SET NOT NULL",
                                               TableName.PRODUCTION_ANALYSIS_STATUS,
                                               "productionanalysisdatetime"));
      
      PartitionTable (TableName.PRODUCTION_ANALYSIS_STATUS, TableName.MACHINE);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.PRODUCTION_ANALYSIS_STATUS);
    }
  }
}
