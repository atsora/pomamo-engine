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
  /// Migration 527:
  /// </summary>
  [Migration(527)]
  public class AddMachineCncVariable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineCncVariable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.MACHINE_CNC_VARIABLE,
                         new Column (TableName.MACHINE_CNC_VARIABLE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_CNC_VARIABLE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32),
                         new Column(TableName.CNC_VARIABLE + "key", DbType.String, ColumnProperty.NotNull),
                         new Column(TableName.CNC_VARIABLE + "value", DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32),
                         new Column (ColumnName.SEQUENCE_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MACHINE_CNC_VARIABLE, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CNC_VARIABLE, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CNC_VARIABLE, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CNC_VARIABLE, ColumnName.SEQUENCE_ID,
                                   TableName.SEQUENCE, ColumnName.SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_CNC_VARIABLE, TableName.CNC_VARIABLE + "key", TableName.CNC_VARIABLE + "value");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MACHINE_CNC_VARIABLE);
    }
  }
}
