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
  /// Migration 164: Fix some foreign keys in betweencycles table
  /// </summary>
  [Migration(164)]
  public class FixBetweenCyclesForeignKeys: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixBetweenCyclesForeignKeys).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveForeignKey (TableName.BETWEEN_CYCLES,
                                 "fk_" + TableName.BETWEEN_CYCLES + "_" + TableName.OPERATION_CYCLE);
      Database.AddForeignKey ("fk_" + TableName.BETWEEN_CYCLES + "_previouscycle",
                              TableName.BETWEEN_CYCLES, "previouscycleid",
                              TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "id",
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddForeignKey ("fk_" + TableName.BETWEEN_CYCLES + "_nextcycle",
                              TableName.BETWEEN_CYCLES, "nextcycleid",
                              TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "id",
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveForeignKey (TableName.BETWEEN_CYCLES,
                                 "fk_" + TableName.BETWEEN_CYCLES + "_previouscycle");
      Database.RemoveForeignKey (TableName.BETWEEN_CYCLES,
                                 "fk_" + TableName.BETWEEN_CYCLES + "_nextcycle");
      Database.GenerateForeignKey (TableName.BETWEEN_CYCLES, "nextcycleid",
                                   TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
  }
}
