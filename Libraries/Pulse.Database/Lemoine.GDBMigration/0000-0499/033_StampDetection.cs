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
  /// Migration 033:
  /// <item>Add the stampdetection table</item>
  /// </summary>
  [Migration(33)]
  public class StampDetection: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (StampDetection).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.STAMP_DETECTION)) {
        AddStampDetectionTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.STAMP_DETECTION)) {
        RemoveStampDetectionTable ();
      }
    }

    /// <summary>
    /// Add the new stampdetection table
    /// </summary>
    private void AddStampDetectionTable ()
    {
      Database.AddTable (TableName.STAMP_DETECTION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.STAMP_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.STAMP_DETECTION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.STAMP_DETECTION, ColumnName.STAMP_ID,
                                   TableName.STAMP, ColumnName.STAMP_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.STAMP_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.STAMP_DETECTION,
                ColumnName.STAMP_ID);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.STAMP_DETECTION));      
    }
    
    /// <summary>
    /// Remove the stampdetection table
    /// </summary>
    private void RemoveStampDetectionTable ()
    {
      Database.RemoveTable (TableName.STAMP_DETECTION);
    }
  }
}
