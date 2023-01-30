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
  /// Migration 287:
  /// </summary>
  [Migration(287)]
  public class AddReasonSummaryShift: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReasonSummaryShift).FullName);
    
    static readonly string DAY_COLUMN = TableName.REASON_SUMMARY + "day";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveUniqueConstraint(TableName.REASON_SUMMARY, ColumnName.MACHINE_ID, DAY_COLUMN,
                             ColumnName.MACHINE_OBSERVATION_STATE_ID, ColumnName.REASON_ID);
      
      Database.AddColumn (TableName.REASON_SUMMARY,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict); // Because of the unicity key
      
      AddUniqueConstraint (TableName.REASON_SUMMARY,
                           ColumnName.MACHINE_ID,
                           DAY_COLUMN,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.REASON_ID,
                           ColumnName.SHIFT_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint(TableName.REASON_SUMMARY,
                             ColumnName.MACHINE_ID,
                             DAY_COLUMN,
                             ColumnName.MACHINE_OBSERVATION_STATE_ID,
                             ColumnName.REASON_ID,
                             ColumnName.SHIFT_ID);

      Database.RemoveColumn (TableName.REASON_SUMMARY,
                             ColumnName.SHIFT_ID);
      
      // Note: do not restore the old unicity constraint,
      // just because of the new data, it might be impossible to do it
      /*
      AddUniqueConstraint (TableName.REASON_SUMMARY,
                           ColumnName.MACHINE_ID,
                           DAY_COLUMN,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.REASON_ID);
       */
    }
  }
}
