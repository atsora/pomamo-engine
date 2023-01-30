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
  /// Migration 104: Some indexes are not used, remove them because they may slow down a little the system
  /// </summary>
  [Migration(104)]
  public class RemoveUnusedIndexes: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveUnusedIndexes).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // - unused

      // workorderslot_machineid_workorderbegindatetime_idx
      RemoveIndex (TableName.WORK_ORDER_SLOT,
                   ColumnName.MACHINE_ID,
                   "workorderbegindatetime");

      // operationslot_machineid_operationslotbegindatetime_idx
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OPERATION_SLOT + "begindatetime");
      
      // observationstateslot_machineid_observationstateslotbegindat_idx
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      // observationstateslot_machineid_observationstateslotbeginday_idx
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "beginday");
      // observationstateslot_userid_observationstateslotbeginday_idx
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.USER_ID,
                   TableName.OBSERVATION_STATE_SLOT + "beginday");

      // reasonslot_machineid_reasonslotbegindatetime_idx
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "begindatetime");
      // reasonslot_machineid_reasonslotenddatetime_idx
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.REASON_SLOT + "enddatetime");

      // userslot_userid_userslotbegindatetime_idx
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "begindatetime");
      // userslot_userid_userslotbeginday_idx
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   TableName.USER_SLOT + "beginday");

      // reasonsummary_reasonid_reasonsummaryday_idx
      RemoveIndex (TableName.REASON_SUMMARY,
                   ColumnName.REASON_ID,
                   TableName.REASON_SUMMARY + "day");

      // fact_machineid_idx
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID);
      // fact_machineid_factbegindatetime_idx
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "begindatetime");
      
      // cncvalue_machinemoduleid_fieldid_idx
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   ColumnName.FIELD_ID);
      // cncvalue_machinemoduleid_fieldid_cncvaluebegindatetime_idx
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   ColumnName.FIELD_ID,
                   TableName.CNC_VALUE + "begindatetime");
      
      // shiftslot_machineid_shiftslotbegindatetime_idx
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "begindatetime");
      // shiftslot_machineid_shiftslotenddatetime_idx
      RemoveIndex (TableName.SHIFT_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.SHIFT_SLOT + "enddatetime");
      
      
      // - duplicate
      
      // detectionanalysislog_datetime_idx
      RemoveIndex (TableName.DETECTION_ANALYSIS_LOG, "datetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // - unused

      // workorderslot_machineid_workorderbegindatetime_idx
      AddIndex (TableName.WORK_ORDER_SLOT,
                ColumnName.MACHINE_ID,
                "workorderbegindatetime");

      // operationslot_machineid_operationslotbegindatetime_idx
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "begindatetime");
      
      // observationstateslot_machineid_observationstateslotbegindat_idx
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      // observationstateslot_machineid_observationstateslotbeginday_idx
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "beginday");
      // observationstateslot_userid_observationstateslotbeginday_idx
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.USER_ID,
                TableName.OBSERVATION_STATE_SLOT + "beginday");

      // reasonslot_machineid_reasonslotbegindatetime_idx
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "begindatetime");
      // reasonslot_machineid_reasonslotenddatetime_idx
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "enddatetime");

      // userslot_userid_userslotbegindatetime_idx
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "begindatetime");
      // userslot_userid_userslotbeginday_idx
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SLOT + "beginday");

      // reasonsummary_reasonid_reasonsummaryday_idx
      AddIndex (TableName.REASON_SUMMARY,
                ColumnName.REASON_ID,
                TableName.REASON_SUMMARY + "day");

      // fact_machineid_idx
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID);
      // fact_machineid_factbegindatetime_idx
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "begindatetime");
      
      // cncvalue_machinemoduleid_fieldid_idx
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID);
      // cncvalue_machinemoduleid_fieldid_cncvaluebegindatetime_idx
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID,
                TableName.CNC_VALUE + "begindatetime");
      
      // shiftslot_machineid_shiftslotbegindatetime_idx
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "begindatetime");
      // shiftslot_machineid_shiftslotenddatetime_idx
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "enddatetime");
      
      
      // - duplicate
      
      // detectionanalysislog_datetime_idx
      /*
      Database.ExecuteNonQuery (@"
CREATE INDEX detectionanalysislog_datetime_idx
ON analysislog (datetime);");*/
    }
  }
}
