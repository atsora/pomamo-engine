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
  /// Migration 328:
  /// </summary>
  [Migration(328)]
  public class AddMachineStateTemplateProduction: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateProduction).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_CATEGORY,
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_CATEGORY + "name", DbType.String, ColumnProperty.NotNull));
      Database.Insert (TableName.MACHINE_STATE_TEMPLATE_CATEGORY,
                       new string[] {ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID, TableName.MACHINE_STATE_TEMPLATE_CATEGORY + "name"},
                       new string[] {"1", "Production"});
      Database.Insert (TableName.MACHINE_STATE_TEMPLATE_CATEGORY,
                       new string[] {ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID, TableName.MACHINE_STATE_TEMPLATE_CATEGORY + "name"},
                       new string[] {"2", "Set-up"});

      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID,
                                   TableName.MACHINE_STATE_TEMPLATE_CATEGORY, ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      
      SetSequence (TableName.MACHINE_STATE_TEMPLATE_CATEGORY, ColumnName.MACHINE_STATE_TEMPLATE_CATEGORY_ID,
                   100);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             TableName.MACHINE_STATE_TEMPLATE + "categoryid");
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_CATEGORY);
    }
  }
}
