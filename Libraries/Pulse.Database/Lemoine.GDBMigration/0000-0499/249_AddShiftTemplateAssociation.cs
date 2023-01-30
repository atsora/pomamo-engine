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
  /// Migration 249: Shift template association
  /// </summary>
  [Migration(249)]
  public class ShiftTemplateAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SHIFT_TEMPLATE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_TEMPLATE_ASSOCIATION + "Begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_TEMPLATE_ASSOCIATION + "End", DbType.DateTime),
                         new Column (TableName.SHIFT_TEMPLATE_ASSOCIATION + "force", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_ASSOCIATION, ColumnName.SHIFT_TEMPLATE_ID,
                                   TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.SHIFT_TEMPLATE_ASSOCIATION);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.SHIFT_TEMPLATE_ASSOCIATION));
      Database.RemoveTable (TableName.SHIFT_TEMPLATE_ASSOCIATION);
    }
  }
}
