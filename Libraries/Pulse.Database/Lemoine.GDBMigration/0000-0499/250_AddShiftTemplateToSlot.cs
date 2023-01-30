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
  /// Migration 250:
  /// </summary>
  [Migration(250)]
  public class AddShiftTemplateToSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftTemplateToSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.SHIFT_SLOT, ColumnName.SHIFT_TEMPLATE_ID,
                                   TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SHIFT_SLOT,
                             ColumnName.SHIFT_TEMPLATE_ID);
    }
  }
}
