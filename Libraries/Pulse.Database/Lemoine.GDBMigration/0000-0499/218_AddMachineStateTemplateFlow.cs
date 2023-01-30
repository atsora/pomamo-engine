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
  /// Migration 218: add the table machine state template flow
  /// </summary>
  [Migration(218)]
  public class AddMachineStateTemplateFlow: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateFlow).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_FLOW,
                         new Column (TableName.MACHINE_STATE_TEMPLATE_FLOW + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_FLOW + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("from" + ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("to" + ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_FLOW, "from" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_FLOW, "to" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.MACHINE_STATE_TEMPLATE_FLOW,
                           "from" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
                           "to" + ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_FLOW);
    }
  }
}
