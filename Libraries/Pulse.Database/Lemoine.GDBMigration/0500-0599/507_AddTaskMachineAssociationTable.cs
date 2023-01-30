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
  /// Migration 507:
  /// </summary>
  [Migration(507)]
  public class AddTaskMachineAssociationTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddTaskMachineAssociationTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.TASK_MACHINE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.TASK_MACHINE_ASSOCIATION + "range", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.TASK_ID, DbType.Int32),
                         new Column (TableName.TASK_MACHINE_ASSOCIATION + "option", DbType.Int32));
      MakeColumnTsRange (TableName.TASK_MACHINE_ASSOCIATION, TableName.TASK_MACHINE_ASSOCIATION + "range");
      Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetMachineModificationTable (TableName.TASK_MACHINE_ASSOCIATION);
      AddIndex (TableName.TASK_MACHINE_ASSOCIATION,
                ColumnName.MACHINE_ID);

    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM machinemodification
WHERE modificationreferencedtable='{0}'",
                                               TableName.TASK_MACHINE_ASSOCIATION));
      Database.RemoveTable (TableName.TASK_MACHINE_ASSOCIATION);
    }
  }
}
