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
  /// Migration 024: Add the following tables
  /// <item>ActivityDetection</item>
  /// <item>ActivityManual</item>
  /// <item>LinkOperation</item>
  /// <item>(removed) Temporary view FactTransition before migrating SfkFacts to Fact</item>
  /// </summary>
  [Migration(24)]
  public class ActivityAndLinkOperationTables: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ActivityAndLinkOperationTables).FullName);
    
    /// <summary>
    /// Add the activitydetection table
    /// </summary>
    private void AddActivityDetectionTable ()
    {
      Database.AddTable (TableName.ACTIVITY_DETECTION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_DETECTION, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.ACTIVITY_DETECTION));
    }
    
    /// <summary>
    /// Add the activitymanual table
    /// </summary>
    private void AddActivityManualTable ()
    {
      Database.AddTable (TableName.ACTIVITY_MANUAL,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ActivityBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("ActivityEndDateTime", DbType.DateTime));
      Database.AddCheckConstraint ("activitymanual_notempty",
                                   TableName.ACTIVITY_MANUAL,
                                   "activitybegindatetime <> activityenddatetime");
      Database.GenerateForeignKey (TableName.ACTIVITY_MANUAL, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_MANUAL, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ACTIVITY_MANUAL, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.ACTIVITY_MANUAL));
    }
    
    /// <summary>
    /// Add the linkoperation table
    /// </summary>
    private void AddLinkOperationTable ()
    {
      Database.AddTable (TableName.LINK_OPERATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("LinkOperationBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("LinkOperationEndDateTime", DbType.DateTime),
                         new Column ("LinkOperationDirection", DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.LINK_OPERATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.LINK_OPERATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.LINK_OPERATION));
    }

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.ACTIVITY_DETECTION)) {
        AddActivityDetectionTable ();
      }
      if (!Database.TableExists (TableName.ACTIVITY_MANUAL)) {
        AddActivityManualTable ();
      }
      if (!Database.TableExists (TableName.LINK_OPERATION)) {
        AddLinkOperationTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists ("FactTransition")) {
        Database.ExecuteNonQuery (@"DROP VIEW FactTransition");
      }
      if (Database.TableExists (TableName.LINK_OPERATION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='LinkOperation'");
        Database.RemoveTable (TableName.LINK_OPERATION);
      }
      if (Database.TableExists ("activity")) { // obsolete table, replaced by the two below
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='Activity'");
        Database.RemoveTable ("activity");
      }
      if (Database.TableExists (TableName.ACTIVITY_MANUAL)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ActivityManual'");
        Database.RemoveTable (TableName.ACTIVITY_MANUAL);
      }
      if (Database.TableExists (TableName.ACTIVITY_DETECTION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ActivityDetection'");
        Database.RemoveTable (TableName.ACTIVITY_DETECTION);
      }
    }
  }
}
