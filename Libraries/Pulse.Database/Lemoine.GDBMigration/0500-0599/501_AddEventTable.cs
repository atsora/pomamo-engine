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
  /// Migration 501:
  /// </summary>
  [Migration(501)]
  public class AddEventTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CreateVirtual ();
      CreateOnly ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS eventonly");
      Database.RemoveTable (TableName.EVENT);
      
      // Note: view event has never been used, it is not useful to create it again
    }
    
    void CreateVirtual ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS event;");

      Database.AddTable (TableName.EVENT,
                         new Column (ColumnName.EVENT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_TYPE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_DATA, DbType.Int32, ColumnProperty.NotNull));
      MakeColumnJson(TableName.EVENT, ColumnName.EVENT_DATA);
      
      SetSequence (TableName.EVENT,
                   ColumnName.EVENT_ID,
                   SequenceName.EVENT_ID);
      Database.GenerateForeignKey (TableName.EVENT, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void CreateOnly ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW eventonly AS
SELECT * FROM ONLY event");
    }
  }
}
