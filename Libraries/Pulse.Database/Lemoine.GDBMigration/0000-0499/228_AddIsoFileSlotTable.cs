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
  /// Migration 228: add the table isofileslot
  /// </summary>
  [Migration(228)]
  public class AddIsoFileSlotTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIsoFileSlotTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.ISO_FILE_SLOT,
                         new Column (TableName.ISO_FILE_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.ISO_FILE_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.ISO_FILE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.ISO_FILE_SLOT + "begindatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.ISO_FILE_SLOT + "enddatetime", DbType.DateTime, ColumnProperty.Null),
                         new Column (TableName.ISO_FILE_SLOT + "beginday", DbType.Date, ColumnProperty.NotNull),
                         new Column (TableName.ISO_FILE_SLOT + "endday", DbType.Date, ColumnProperty.Null)
                        );
      Database.GenerateForeignKey (TableName.ISO_FILE_SLOT, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ISO_FILE_SLOT, ColumnName.ISO_FILE_ID,
                                   TableName.ISO_FILE, ColumnName.ISO_FILE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddCheckConstraint (string.Format ("{0}_beginend", TableName.ISO_FILE_SLOT),
                                   TableName.ISO_FILE_SLOT,
                                   string.Format ("({1} IS NULL) OR ({0} < {1})",
                                                  TableName.ISO_FILE_SLOT + "begindatetime",
                                                  TableName.ISO_FILE_SLOT + "enddatetime"));
      AddUniqueConstraint (TableName.ISO_FILE_SLOT,
                           ColumnName.MACHINE_MODULE_ID,
                           TableName.ISO_FILE_SLOT + "begindatetime");
      AddUniqueConstraint (TableName.ISO_FILE_SLOT,
                           ColumnName.MACHINE_MODULE_ID,
                           TableName.ISO_FILE_SLOT + "enddatetime");
      AddIndex (TableName.ISO_FILE_SLOT,
                ColumnName.MACHINE_MODULE_ID,
                TableName.ISO_FILE_SLOT + "beginday");
      AddIndex (TableName.ISO_FILE_SLOT,
                ColumnName.MACHINE_MODULE_ID,
                TableName.ISO_FILE_SLOT + "enddatetime",
                TableName.ISO_FILE_SLOT + "begindatetime");
      AddIndex (TableName.ISO_FILE_SLOT,
                ColumnName.MACHINE_MODULE_ID,
                TableName.ISO_FILE_SLOT + "endday",
                TableName.ISO_FILE_SLOT + "beginday");
      
      PartitionTable (TableName.ISO_FILE_SLOT, TableName.MACHINE_MODULE);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UnpartitionTable (TableName.ISO_FILE_SLOT);
      Database.RemoveTable (TableName.ISO_FILE_SLOT);
    }
  }
}
