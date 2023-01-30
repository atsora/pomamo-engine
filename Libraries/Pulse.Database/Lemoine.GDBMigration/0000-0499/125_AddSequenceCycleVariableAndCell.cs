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
  /// Migration 125: Add in database
  /// <item>two columns to store for a machine module a sequence and cycle variable</item>
  /// <item>a new table to store in which cell is a machine</item>
  /// </summary>
  [Migration(125)]
  public class AddSequenceCycleVariableAndCell: MachineHierarchyMigration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceCycleVariableAndCell).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddSequenceCycleVariable ();
      AddCell ();
      AddMachineCellUpdate ();
      AddCellToMachineFilter ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveCellFromMachineFilter ();
      RemoveMachineCellUpdate ();
      RemoveCell ();
      RemoveSequenceCycleVariable ();
    }
    
    void AddSequenceCycleVariable ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "sequencevariable", DbType.String, ColumnProperty.Null));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "cyclevariable", DbType.String, ColumnProperty.Null));
    }
    
    void RemoveSequenceCycleVariable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "cyclevariable");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "sequencevariable");
    }
    
    void AddCell ()
    {
      AddMachineHierarchyTable (TableName.CELL);
    }
    
    void RemoveCell ()
    {
      RemoveMachineHierarchyTable (TableName.CELL);
    }
    
    void AddMachineCellUpdate ()
    {
      Database.AddTable (TableName.MACHINE_CELL_UPDATE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("oldcellid", DbType.Int32),
                         new Column ("newcellid", DbType.Int32));
      Database.AddCheckConstraint ("machinecell_old_new",
                                   TableName.MACHINE_CELL_UPDATE,
                                   @"(oldcellid IS NOT NULL) OR (newcellid IS NOT NULL)");
      Database.GenerateForeignKey (TableName.MACHINE_CELL_UPDATE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CELL_UPDATE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CELL_UPDATE, "oldcellid",
                                   TableName.CELL, ColumnName.CELL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_CELL_UPDATE, "newcellid",
                                   TableName.CELL, ColumnName.CELL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.MACHINE_CELL_UPDATE);
    }
    
    void RemoveMachineCellUpdate ()
    {
      Database.RemoveTable (TableName.MACHINE_CELL_UPDATE);
    }
    
    void AddCellToMachineFilter ()
    {
      Database.AddColumn (TableName.MACHINE_FILTER,
                          new Column (ColumnName.CELL_ID, DbType.Int32, ColumnProperty.Null));

      // Foreign keys
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.CELL_ID,
                                   TableName.CELL, ColumnName.CELL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveCellFromMachineFilter ()
    {
      Database.RemoveColumn (TableName.MACHINE_FILTER,
                             ColumnName.CELL_ID);
                             
    }
  }
}
