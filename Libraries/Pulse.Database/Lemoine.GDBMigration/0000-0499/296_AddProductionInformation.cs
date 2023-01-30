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
  /// Migration 296:
  /// </summary>
  [Migration(296)]
  public class AddProductionInformation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddProductionInformation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      ProductionInformationShiftUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      ProductionInformationShiftDown ();
    }

    void ProductionInformationShiftUp ()
    {
      Database.AddTable (TableName.PRODUCTION_INFORMATION_SHIFT,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.DAY, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PRODUCTION_INFORMATION_SHIFT + "checked", DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PRODUCTION_INFORMATION_SHIFT + "scrapped", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (TableName.PRODUCTION_INFORMATION_SHIFT + "inprogress", DbType.Boolean, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION_SHIFT, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION_SHIFT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION_SHIFT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION_SHIFT, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION_SHIFT, ColumnName.SHIFT_ID,
                                   TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.PRODUCTION_INFORMATION_SHIFT);
    }
    
    void ProductionInformationShiftDown ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.PRODUCTION_INFORMATION_SHIFT));
      Database.RemoveTable (TableName.PRODUCTION_INFORMATION_SHIFT);
    }

    void ProductionInformationUp ()
    {
      Database.AddTable (TableName.PRODUCTION_INFORMATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PRODUCTION_INFORMATION + "datetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PRODUCTION_INFORMATION + "checked", DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PRODUCTION_INFORMATION + "scrapped", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (TableName.PRODUCTION_INFORMATION + "inprogress", DbType.Boolean, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.PRODUCTION_INFORMATION, ColumnName.SHIFT_ID,
                                   TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.SHIFT_MACHINE_ASSOCIATION);
    }
    
    void ProductionInformationDown ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.PRODUCTION_INFORMATION));
      Database.RemoveTable (TableName.PRODUCTION_INFORMATION);
    }
  }
}
