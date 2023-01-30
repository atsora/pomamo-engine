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
  /// Migration 232: add the notion of a line, linked to machines and output components
  /// </summary>
  [Migration(232)]
  public class LineTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LineTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add table line
      CreateTableLine();
      
      // Add table lineMachine
      CreateTableLineMachine();
      
      // Add table lineComponent
      CreateTableLineComponent();
      
      // Alter table OperationSourceWorkPiece
      AlterTableOperationWorkPiece();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RestoreTableOperationWorkPiece();
      Database.RemoveTable(TableName.LINE_COMPONENT);
      Database.RemoveTable(TableName.LINE_MACHINE);
      Database.RemoveTable(TableName.LINE_OLD);
      
      // Remove the display property for the table "line"
      Database.Delete(TableName.DISPLAY, "displaytable", TableName.LINE_OLD);
    }
    
    void CreateTableLine()
    {
      Database.AddTable (TableName.LINE_OLD,
                         new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.LINE_OLD + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.LINE_OLD + "code", DbType.String),
                         new Column (TableName.LINE_OLD + "name", DbType.String));
      AddNamedUniqueConstraint(TableName.LINE_OLD + "_unique", TableName.LINE_OLD,
                            TableName.LINE_OLD + "code");
      
      // Add a display property
      Database.Insert(TableName.DISPLAY,
                      new string[] {"displaytable", "displaypattern"},
                      new string[] {TableName.LINE_OLD, "<%Name%>"});
    }
    
    void CreateTableLineMachine()
    {
      Database.AddTable (TableName.LINE_MACHINE,
                         new Column (ColumnName.LINE_MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.LINE_MACHINE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.LINE_MACHINE + "status", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.LINE_MACHINE, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.LINE_MACHINE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.LINE_MACHINE, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.AddCheckConstraint(TableName.LINE_MACHINE + "_" + "status",
                                  TableName.LINE_MACHINE,
                                  TableName.LINE_MACHINE + "status" + " > 0");
      AddNamedUniqueConstraint(TableName.LINE_MACHINE + "_unique", TableName.LINE_MACHINE,
                            ColumnName.LINE_ID,
                            ColumnName.MACHINE_ID,
                            ColumnName.OPERATION_ID);
    }
    
    void CreateTableLineComponent()
    {
      Database.AddTable (TableName.LINE_COMPONENT,
                         new Column (ColumnName.LINE_COMPONENT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.LINE_COMPONENT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.LINE_COMPONENT, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.LINE_COMPONENT, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint(TableName.LINE_COMPONENT + "_unique", TableName.LINE_COMPONENT,
                            ColumnName.LINE_ID,
                            ColumnName.COMPONENT_ID);
    }
    
    void AlterTableOperationWorkPiece()
    {
      // Replace composite key with an id as primary key
      Database.RemoveConstraint(TableName.OPERATION_SOURCE_WORKPIECE, "pk_operationsourceworkpiece");
      Database.AddColumn(TableName.OPERATION_SOURCE_WORKPIECE,
                         new Column(ColumnName.OPERATION_SOURCE_WORKPIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity));
      
      // Add version
      Database.AddColumn(TableName.OPERATION_SOURCE_WORKPIECE,
                         new Column(TableName.OPERATION_SOURCE_WORKPIECE + "version", DbType.Int32, ColumnProperty.NotNull, 1));
      
      // Add unicity constraint
      AddNamedUniqueConstraint(TableName.OPERATION_SOURCE_WORKPIECE + "_unique", TableName.OPERATION_SOURCE_WORKPIECE,
                            ColumnName.OPERATION_ID,
                            ColumnName.INTERMEDIATE_WORK_PIECE_ID);
      
      // Add column denominator
      Database.AddColumn(TableName.OPERATION_SOURCE_WORKPIECE,
                         new Column(TableName.OPERATION_SOURCE_WORKPIECE + "quantitydenominator",
                                    DbType.Int32, ColumnProperty.NotNull, 1));
      Database.AddCheckConstraint(TableName.OPERATION_SOURCE_WORKPIECE + "_" + "quantitydenominator",
                                  TableName.OPERATION_SOURCE_WORKPIECE,
                                  TableName.OPERATION_SOURCE_WORKPIECE + "quantitydenominator" + " > 0");
    }
    
    void RestoreTableOperationWorkPiece()
    {
      // Remove denominator
      Database.RemoveColumn(TableName.OPERATION_SOURCE_WORKPIECE,
                            TableName.OPERATION_SOURCE_WORKPIECE + "quantitydenominator");
      
      // Remove unicity constraint
      Database.RemoveConstraint(TableName.OPERATION_SOURCE_WORKPIECE, "operationsourceworkpiece_unique");
      
      // Remove version
      Database.RemoveColumn(TableName.OPERATION_SOURCE_WORKPIECE, TableName.OPERATION_SOURCE_WORKPIECE + "version");
      
      // Replace primary key with composite key
      Database.RemoveColumn(TableName.OPERATION_SOURCE_WORKPIECE, ColumnName.OPERATION_SOURCE_WORKPIECE_ID);
      Database.AddPrimaryKey("pk_operationsourceworkpiece", TableName.OPERATION_SOURCE_WORKPIECE,
                             ColumnName.OPERATION_ID, ColumnName.INTERMEDIATE_WORK_PIECE_ID);
    }
  }
}
