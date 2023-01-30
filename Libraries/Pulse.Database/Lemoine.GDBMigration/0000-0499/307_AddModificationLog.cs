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
  /// Migration 307:
  /// </summary>
  [Migration(307)]
  public class AddModificationLog: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationLog).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameTable (TableName.ANALYSIS_LOG,
                            TableName.OLD_ANALYSIS_LOG);
      
      Database.AddTable (TableName.GLOBAL_MODIFICATION_LOG,
                         new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("Level", DbType.String, ColumnProperty.NotNull),
                         new Column ("Message", DbType.String, ColumnProperty.NotNull, 1023),
                         new Column ("Module", DbType.String),
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.NotNull));
      ConvertToLogTable (TableName.GLOBAL_MODIFICATION_LOG);
      Database.GenerateForeignKey (TableName.GLOBAL_MODIFICATION_LOG, ColumnName.MODIFICATION_ID,
                                   TableName.GLOBAL_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.AddTable (TableName.MACHINE_MODIFICATION_LOG,
                         new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("Level", DbType.String, ColumnProperty.NotNull),
                         new Column ("Message", DbType.String, ColumnProperty.NotNull, 1023),
                         new Column ("Module", DbType.String),
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull));
      ConvertToLogTable (TableName.MACHINE_MODIFICATION_LOG);
      Database.GenerateForeignKey (TableName.MACHINE_MODIFICATION_LOG, ColumnName.MODIFICATION_ID,
                                   TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_MODIFICATION_LOG, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW analysislog AS
SELECT logid, datetime, level, message, module, modificationid
FROM globalmodificationlog
UNION
SELECT logid, datetime, level, message, module, modificationid
FROM machinemodificationlog
UNION
SELECT logid, datetime, level, message, module, modificationid
FROM oldanalysislog
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS analysislog");
      Database.RemoveTable (TableName.GLOBAL_MODIFICATION_LOG);
      Database.RemoveTable (TableName.MACHINE_MODIFICATION_LOG);
      Database.RenameTable (TableName.OLD_ANALYSIS_LOG,
                            TableName.ANALYSIS_LOG);
    }
    
  }
}
