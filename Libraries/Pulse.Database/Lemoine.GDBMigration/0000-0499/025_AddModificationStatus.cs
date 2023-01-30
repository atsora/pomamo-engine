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
  /// Migration 025: Split the modification table in:
  /// <item>modification table that is not updated (only inserted)</item>
  /// <item>modificationstatus table that is only updated by the Lem_Analysis service</item>
  /// </summary>
  [Migration(25)]
  public class AddModificationStatus: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationStatus).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {      
      if (!Database.TableExists (TableName.MODIFICATION_STATUS)) {
        Database.RemoveConstraint (TableName.MODIFICATION,
                                   "modification_analysisstatusids");
        Database.ExecuteNonQuery ("DROP INDEX IF EXISTS modification_pendinganalysis_idx");
        Database.AddTable (TableName.MODIFICATION_STATUS,
                           new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("AnalysisStatusId", DbType.Int32, ColumnProperty.NotNull, 0),
                           new Column ("AnalysisAppliedDateTime", DbType.DateTime, ColumnProperty.Null));
        Database.GenerateForeignKey (TableName.MODIFICATION_STATUS, ColumnName.MODIFICATION_ID,
                                     TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
                                     TableName.MODIFICATION_STATUS,
                                     "analysisstatusid IN (0, 1, 3, 4, 5, 6)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete
        Database.AddCheckConstraint ("modificationstatus_analysisstatusids_applieddatetime",
                                     TableName.MODIFICATION_STATUS,
                                     "(analysisstatusid <> 0) OR (analysisapplieddatetime IS NULL)");
        Database.ExecuteNonQuery ("CREATE INDEX modificationstatus_analysisstatusid_idx " +
                                  "ON modificationstatus (analysisstatusid)");
        Database.ExecuteNonQuery (@"INSERT INTO modificationstatus
SELECT modificationid, analysisstatusid, analysisapplieddatetime 
FROM modification");
        Database.RemoveColumn (TableName.MODIFICATION,
                               "analysisstatusid");
        Database.RemoveColumn (TableName.MODIFICATION,
                               "analysisapplieddatetime");
        Database.ExecuteNonQuery ("CREATE INDEX modification_datetime_idx " +
                                  "ON modification (modificationdatetime);");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.MODIFICATION_STATUS)) {
        Database.AddColumn (TableName.MODIFICATION, "AnalysisStatusId", DbType.Int32, 1, ColumnProperty.NotNull, 1);
        Database.AddColumn (TableName.MODIFICATION, "AnalysisAppliedDateTime", DbType.DateTime, ColumnProperty.Null);
        Database.ExecuteNonQuery (@"UPDATE modification
SET analysisstatusid=modificationstatus.analysisstatusid,
    analysisapplieddatetime=modificationstatus.analysisapplieddatetime
FROM modificationstatus
WHERE modificationstatus.modificationid = modification.modificationid
  AND modificationstatus.analysisstatusid <> 0");
        Database.RemoveTable (TableName.MODIFICATION_STATUS);
        Database.ExecuteNonQuery ("DROP INDEX IF EXISTS modification_datetime_idx");
        Database.AddCheckConstraint ("modification_analysisstatusids",
                                     TableName.MODIFICATION,
                                     @"(analysisstatusid = 1)
 OR (analysisstatusid = 3) OR (analysisstatusid = 4)
 OR (analysisstatusid = 5) OR (analysisstatusid = 6)"); // 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete
        Database.ExecuteNonQuery ("CREATE INDEX modification_pendinganalysis_idx " +
                                  "ON modification (modificationdatetime) " +
                                  "WHERE analysisstatusid=1;");
      }
    }
  }
}
