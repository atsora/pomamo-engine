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
  /// Migration 149: Re-index some tables with time ranges to make the requests faster
  /// </summary>
  [Migration(149)]
  public class ReIndexTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReIndexTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // cncvalue
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   TableName.CNC_VALUE + "begindatetime",
                   TableName.CNC_VALUE + "enddatetime");
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                TableName.CNC_VALUE + "enddatetime",
                TableName.CNC_VALUE + "begindatetime");

      // reasonslot
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID);
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "beginday",
                   TableName.REASON_SLOT + "endday");
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "begindatetime",
                   TableName.REASON_SLOT + "enddatetime");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "endday",
                TableName.REASON_SLOT + "beginday");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "begindatetime");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "enddatetime",
                TableName.REASON_SLOT + "begindatetime");
      
      // fact
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "begindatetime",
                   TableName.FACT + "enddatetime");
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "enddatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "enddatetime",
                TableName.FACT + "begindatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "begindatetime");
      
      // operationslot
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "beginday",
                   TableName.OPERATION_SLOT + "endday");
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "begindatetime",
                   TableName.OPERATION_SLOT + "enddatetime");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "endday",
                TableName.OPERATION_SLOT + "beginday");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "enddatetime",
                TableName.OPERATION_SLOT + "begindatetime");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "begindatetime");
      
      // shiftslot
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "beginday",
                   TableName.SHIFT_SLOT + "endday");
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "begindatetime",
                   TableName.SHIFT_SLOT + "enddatetime");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "endday",
                TableName.SHIFT_SLOT + "beginday");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "enddatetime",
                TableName.SHIFT_SLOT + "begindatetime");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "begindatetime");
      
      // observationstateslot
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID);
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "beginday",
                   TableName.OBSERVATION_STATE_SLOT + "endday");
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "begindatetime",
                   TableName.OBSERVATION_STATE_SLOT + "enddatetime");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "endday",
                TableName.OBSERVATION_STATE_SLOT + "beginday");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      
      // userslot
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "beginday",
                   TableName.USER_SLOT + "endday");
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "begindatetime",
                   TableName.USER_SLOT + "enddatetime");
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "endday",
                TableName.USER_SLOT + "beginday");
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "enddatetime",
                TableName.USER_SLOT + "begindatetime");
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "begindatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // cncvalue
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   TableName.CNC_VALUE + "enddatetime",
                   TableName.CNC_VALUE + "begindatetime");
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                TableName.CNC_VALUE + "begindatetime",
                TableName.CNC_VALUE + "enddatetime");
      
      // reasonslot
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "endday",
                   TableName.REASON_SLOT + "beginday");
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "begindatetime");
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "enddatetime",
                   TableName.REASON_SLOT + "begindatetime");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "beginday",
                TableName.REASON_SLOT + "endday");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "begindatetime",
                TableName.REASON_SLOT + "enddatetime");
      
      // fact
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "enddatetime",
                   TableName.FACT + "begindatetime");
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "begindatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "begindatetime",
                TableName.FACT + "enddatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "enddatetime");
      
      // operationslot
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "endday",
                   TableName.OPERATION_SLOT + "beginday");
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "enddatetime",
                   TableName.OPERATION_SLOT + "begindatetime");
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "begindatetime");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "beginday",
                TableName.OPERATION_SLOT + "endday");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "begindatetime",
                TableName.OPERATION_SLOT + "enddatetime");
      
      // shiftslot
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "endday",
                   TableName.SHIFT_SLOT + "beginday");
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "enddatetime",
                   TableName.SHIFT_SLOT + "begindatetime");
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "begindatetime");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "beginday",
                TableName.SHIFT_SLOT + "endday");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "begindatetime",
                TableName.SHIFT_SLOT + "enddatetime");
      
      // observationstateslot
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "endday",
                   TableName.OBSERVATION_STATE_SLOT + "beginday");
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                   TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "beginday",
                TableName.OBSERVATION_STATE_SLOT + "endday");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "begindatetime",
                TableName.OBSERVATION_STATE_SLOT + "enddatetime");
      
      // userslot
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "endday",
                   TableName.USER_SLOT + "beginday");
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "enddatetime",
                   TableName.USER_SLOT + "begindatetime");
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "begindatetime");
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "beginday",
                TableName.USER_SLOT + "endday");
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "begindatetime",
                TableName.USER_SLOT + "enddatetime");
    }
  }
}
