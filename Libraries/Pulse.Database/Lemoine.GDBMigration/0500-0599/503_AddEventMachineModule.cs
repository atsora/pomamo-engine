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
  /// Migration 503:
  /// </summary>
  [Migration(503)]
  public class AddEventMachineModule: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventMachineModule).FullName);
    
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
      UnpartitionTable (TableName.EVENT_MACHINE_MODULE_GENERIC);
      
      Database.RemoveTable (TableName.EVENT_MACHINE_MODULE_GENERIC);
      Database.RemoveTable (TableName.EVENT_MACHINE_MODULE);
    }
    
    void CreateAbstract ()
    {
      Inherit (TableName.EVENT_MACHINE_MODULE, TableName.EVENT_MACHINE, ColumnName.EVENT_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.AddColumn (TableName.EVENT_MACHINE_MODULE,
                          new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull));

      AddIndex (TableName.EVENT_MACHINE_MODULE,
                ColumnName.MACHINE_MODULE_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void CreateGeneric ()
    {
      Inherit (TableName.EVENT_MACHINE_MODULE_GENERIC, TableName.EVENT_MACHINE_MODULE, ColumnName.EVENT_ID);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE_GENERIC, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE_GENERIC, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey (TableName.EVENT_MACHINE_MODULE_GENERIC, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      PartitionTable (TableName.EVENT_MACHINE_MODULE_GENERIC, TableName.MACHINE_MODULE);
    }
  }
}
