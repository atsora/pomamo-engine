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
  /// Migration 502:
  /// </summary>
  [Migration(502)]
  public class AddEventMachine: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventMachine).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CreateAbstract ();
      CreateGeneric ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UnpartitionTable (TableName.EVENT_MACHINE_GENERIC);
      
      Database.RemoveTable (TableName.EVENT_MACHINE_GENERIC);
      Database.RemoveTable (TableName.EVENT_MACHINE);
    }
    
    void CreateAbstract ()
    {
      Inherit (TableName.EVENT_MACHINE, TableName.EVENT, ColumnName.EVENT_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.AddColumn (TableName.EVENT_MACHINE,
                          new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull));

      AddIndex (TableName.EVENT_MACHINE,
                ColumnName.MACHINE_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void CreateGeneric ()
    {
      Inherit (TableName.EVENT_MACHINE_GENERIC, TableName.EVENT_MACHINE, ColumnName.EVENT_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_GENERIC, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_GENERIC, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      PartitionTable (TableName.EVENT_MACHINE_GENERIC, TableName.MACHINE);
    }
  }
}
