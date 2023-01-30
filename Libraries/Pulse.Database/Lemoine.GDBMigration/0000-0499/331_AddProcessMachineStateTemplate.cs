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
  /// Migration 331:
  /// </summary>
  [Migration(331)]
  public class AddProcessMachineStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddProcessMachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.PROCESS_MACHINE_STATE_TEMPLATE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.PROCESS_MACHINE_STATE_TEMPLATE + "range", DbType.Int32, ColumnProperty.NotNull));
      MakeColumnTsRange (TableName.PROCESS_MACHINE_STATE_TEMPLATE, TableName.PROCESS_MACHINE_STATE_TEMPLATE + "range");
      Database.GenerateForeignKey (TableName.PROCESS_MACHINE_STATE_TEMPLATE, ColumnName.MODIFICATION_ID,
                                   TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.PROCESS_MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetMachineModificationTable (TableName.PROCESS_MACHINE_STATE_TEMPLATE);
      AddIndex (TableName.PROCESS_MACHINE_STATE_TEMPLATE,
                ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM machinemodification
WHERE modificationreferencedtable='{0}'",
                                               TableName.PROCESS_MACHINE_STATE_TEMPLATE));
      Database.RemoveTable (TableName.PROCESS_MACHINE_STATE_TEMPLATE);
    }
  }
}
