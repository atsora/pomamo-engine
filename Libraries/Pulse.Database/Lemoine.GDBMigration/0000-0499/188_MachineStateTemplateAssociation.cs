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
  /// Migration 188: Machine state template association
  /// </summary>
  [Migration(188)]
  public class MachineStateTemplateAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "Begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "End", DbType.DateTime));
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      SetModificationTable (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION));
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION);
    }
  }
}
