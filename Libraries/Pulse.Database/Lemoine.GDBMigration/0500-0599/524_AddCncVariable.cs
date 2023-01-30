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
  /// Migration 524: Add cncvariable table
  /// </summary>
  [Migration(524)]
  public class AddCncVariable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCncVariable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      CreateTable();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      DeleteTable();
    }
    
    void CreateTable()
    {
      Database.AddTable(TableName.CNC_VARIABLE,
                        new Column(TableName.CNC_VARIABLE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.CNC_VARIABLE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.CNC_VARIABLE + "key", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_VARIABLE + "value", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_VARIABLE + "datetimerange", DbType.Int32, ColumnProperty.NotNull));
      MakeColumnTsRange(TableName.CNC_VARIABLE, TableName.CNC_VARIABLE + "datetimerange");
      
      // constraints / foreign keys
      AddConstraintRangeNotEmpty (TableName.CNC_VARIABLE, TableName.CNC_VARIABLE + "datetimerange");
      Database.GenerateForeignKey (TableName.CNC_VARIABLE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // indexes
      AddNoOverlapConstraint (TableName.CNC_VARIABLE, TableName.CNC_VARIABLE + "datetimerange",
                              ColumnName.MACHINE_MODULE_ID, TableName.CNC_VARIABLE + "key");
      AddNamedIndex (TableName.CNC_VARIABLE + "_lower", TableName.CNC_VARIABLE, ColumnName.MACHINE_MODULE_ID, "lower(cncvariabledatetimerange)");
      
      // partitioning
      PartitionTable (TableName.CNC_VARIABLE, TableName.MACHINE_MODULE);
    }
    
    void DeleteTable()
    {
      UnpartitionTable (TableName.CNC_VARIABLE);
      Database.RemoveTable(TableName.CNC_VARIABLE);
    }
  }
}
