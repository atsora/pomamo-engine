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
  /// Migration 177: Add the machinemoduleactivity table
  /// </summary>
  [Migration(177)]
  public class AddMachineModuleActivityTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModuleActivityTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable(TableName.MACHINE_MODULE_ACTIVITY,
                        new Column(TableName.MACHINE_MODULE_ACTIVITY + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.MACHINE_MODULE_ACTIVITY + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.MACHINE_MODULE_ACTIVITY + "begindatetime", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(TableName.MACHINE_MODULE_ACTIVITY + "enddatetime", DbType.DateTime, ColumnProperty.NotNull),
                        new Column(ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull));
      AddIndex(TableName.MACHINE_MODULE_ACTIVITY,
               ColumnName.MACHINE_MODULE_ID, TableName.MACHINE_MODULE_ACTIVITY + "enddatetime", TableName.MACHINE_MODULE_ACTIVITY + "begindatetime");
      AddUniqueConstraint(TableName.MACHINE_MODULE_ACTIVITY,
                          ColumnName.MACHINE_MODULE_ID, TableName.MACHINE_MODULE_ACTIVITY + "begindatetime");
      Database.AddCheckConstraint(string.Format("{0}_begindatetime_enddatetime", TableName.MACHINE_MODULE_ACTIVITY),
                                  TableName.MACHINE_MODULE_ACTIVITY,
                                  string.Format ("{0} < {1}",
                                                 TableName.MACHINE_MODULE_ACTIVITY + "begindatetime",
                                                 TableName.MACHINE_MODULE_ACTIVITY + "enddatetime"));
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_ACTIVITY, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.MACHINE_MODULE_ACTIVITY, ColumnName.MACHINE_MODE_ID,
                                  TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MACHINE_MODULE_ACTIVITY);
    }
  }
}
