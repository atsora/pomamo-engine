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
  /// Migration 219: Add the table automachinestatetemplate,
  /// where a new machine state template may be triggered by a current machine state template and a new machine mode
  /// </summary>
  [Migration(219)]
  public class AddAutoMachineStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAutoMachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.AUTO_MACHINE_STATE_TEMPLATE,
                         new Column (TableName.AUTO_MACHINE_STATE_TEMPLATE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.AUTO_MACHINE_STATE_TEMPLATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("current" + ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32),
                         new Column ("new" + ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.AUTO_MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.AUTO_MACHINE_STATE_TEMPLATE, "current" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.AUTO_MACHINE_STATE_TEMPLATE, "new" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.AUTO_MACHINE_STATE_TEMPLATE,
                           ColumnName.MACHINE_MODE_ID,
                           "current" + ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.AUTO_MACHINE_STATE_TEMPLATE);
    }
  }
}
