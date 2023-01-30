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
  /// Migration 023: Add the notion of operation cycle with these new tables:
  /// <item>modification table OperationCycleInformation</item>
  /// <item>log table analysislog</item>
  /// </summary>
  [Migration(23)]
  public class OperationCycleInformation: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleInformation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.OPERATION_CYCLE_INFORMATION)) {
        AddOperationCycleInformation ();
      }
      
      // Add an index to get the done OperationCycleInformation modifications
      // that correspond to a given date/time
      Database.ExecuteNonQuery (@"CREATE INDEX modification_doneanalysis_idx
ON modification (modificationreferencedtable, analysisapplieddatetime)
WHERE analysisstatusid=3
  AND modificationreferencedtable IN ('OperationCycleInformation');");
      
      if (!Database.TableExists (TableName.ANALYSIS_LOG)) {
        AddAnalysisLog ();
      }

      UpdateConstraintModificationAnalysisStatusIds ();
    }
    
    /// <summary>
    /// Add the modification table OperationCycleInformation
    /// </summary>
    private void AddOperationCycleInformation ()
    {
      Database.AddTable (TableName.OPERATION_CYCLE_INFORMATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32),
                         new Column ("OperationCycleApplicationDateTime", DbType.DateTime),
                         new Column ("OperationCycleBegin", DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column ("OperationCycleEnd", DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column ("DryCycleNumber", DbType.Int32),
                         new Column ("DryCycleNumberOffset", DbType.Int32));
      Database.AddCheckConstraint ("operationcycleinformation_begin_end_datetime",
                                   TableName.OPERATION_CYCLE_INFORMATION,
                                   "(operationcyclebegin = FALSE AND operationcycleend = FALSE) OR (operationcycleapplicationdatetime IS NOT NULL)");
      Database.AddCheckConstraint ("operationcycleinformation_begin_end",
                                   TableName.OPERATION_CYCLE_INFORMATION,
                                   "NOT ( (operationcyclebegin = TRUE) AND (operationcycleend = TRUE))");
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.OPERATION_CYCLE_INFORMATION));
    }
    
    /// <summary>
    /// Add the log table analysislog
    /// </summary>
    private void AddAnalysisLog ()
    {
      if (!Database.TableExists (TableName.ANALYSIS_LOG)) {
        Database.AddTable (TableName.ANALYSIS_LOG,
                           new Column (ColumnName.LOG_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("Level", DbType.String, ColumnProperty.NotNull),
                           new Column ("Message", DbType.String, ColumnProperty.NotNull),
                           new Column ("Module", DbType.String),
                           new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.NotNull));
        Database.ExecuteNonQuery ("ALTER TABLE analysislog " +
                                  "ALTER COLUMN datetime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery (@"ALTER TABLE analysislog
ALTER COLUMN logid
SET DEFAULT nextval('log_logid_seq'::regclass)");
        Database.ExecuteNonQuery (@"CREATE INDEX analysislog_datetime_idx " +
                                  "ON analysislog (datetime);");
      }
    }
    
    /// <summary>
    /// Update the constraint modification_analysisstatusids
    /// to add the Error status
    /// </summary>
    private void UpdateConstraintModificationAnalysisStatusIds ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE modification
DROP CONSTRAINT modification_analysisstatusids;");
      Database.ExecuteNonQuery (@"ALTER TABLE modification
ADD CONSTRAINT modification_analysisstatusids
CHECK (analysisstatusid = 1 OR analysisstatusid = 3 OR analysisstatusid = 4 OR analysisstatusid = 5 OR analysisstatusid = 6);"); // 1: Pending, 3: Done, 4: Error, 5: Obsolete
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.OPERATION_CYCLE_INFORMATION)) {
        Database.RemoveTable (TableName.OPERATION_CYCLE_INFORMATION);
      }

      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS modification_doneanalysis_idx");

      if (Database.TableExists (TableName.ANALYSIS_LOG)) {
        Database.RemoveTable (TableName.ANALYSIS_LOG);
      }
    }
  }
}
