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
  /// Migration 257:
  /// </summary>
  [Migration(257)]
  public class RestoreShiftMachineAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RestoreShiftMachineAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SHIFT_MACHINE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.DAY, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.SHIFT_MACHINE_ASSOCIATION + "Begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_MACHINE_ASSOCIATION + "End", DbType.DateTime));
      Database.GenerateForeignKey (TableName.SHIFT_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_MACHINE_ASSOCIATION, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.SHIFT_MACHINE_ASSOCIATION);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.SHIFT_MACHINE_ASSOCIATION));
      Database.RemoveTable (TableName.SHIFT_MACHINE_ASSOCIATION);
    }
  }
}
