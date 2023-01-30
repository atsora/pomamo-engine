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
  /// Migration 332:
  /// </summary>
  [Migration(332)]
  public class RemoveForeignKeysBeforeRestrictingThem: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveForeignKeysBeforeRestrictingThem).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        // Now part of Lemoine.Plugin.IntermediateWorkPieceSummary
        return;
      }

      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, TableName.WORK_ORDER);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, TableName.LINE_OLD);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, TableName.COMPONENT);
      RemoveForeignKey (TableName.INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, TableName.SHIFT);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void RemoveForeignKey (string table, string foreignTable)
    {
      Database.RemoveForeignKey (table, GetForeignKeyConstraintName (table, foreignTable));
    }
    
    string GetForeignKeyConstraintName (string table, string foreignTable)
    {
      return "fk_" + table + "_" + foreignTable;
    }
  }
}
