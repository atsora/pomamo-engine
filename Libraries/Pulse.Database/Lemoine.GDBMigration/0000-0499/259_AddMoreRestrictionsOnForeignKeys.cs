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
  /// Migration 259: add more restrictions to some foreign keys
  /// </summary>
  [Migration(259)]
  public class AddMoreRestrictionsOnForeignKeys: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMoreRestrictionsOnForeignKeys).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.WORK_ORDER_ID);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.LINE_ID);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.COMPONENT_ID);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.SHIFT_ID);
      Database.GenerateForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void RemoveForeignKey (string table, string column)
    {
      Database.RemoveForeignKey (table, GetForeignKeyConstraintName (table, column));
    }
    
    string GetForeignKeyConstraintName (string table, string column)
    {
      return "fk_" + table + "_" + column;
    }
  }
}
