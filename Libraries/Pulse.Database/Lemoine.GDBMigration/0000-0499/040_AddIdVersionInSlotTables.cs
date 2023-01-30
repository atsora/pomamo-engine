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
  /// Migration 040: add an Id and Version columns in the Slot tables:
  /// <item>WorkOrderSlot</item>
  /// <item>OperationSlot</item>
  /// <item>ObservationStateSlot</item>
  /// <item>ReasonSlot</item>
  /// <item>UserSlot</item>
  /// </summary>
  [Migration(40)]
  public class AddIdVersionInSlotTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIdVersionInSlotTables).FullName);

    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpdateWorkOrderSlot ();
      UpdateOperationSlot ();
      UpdateObservationStateSlot ();
      UpdateReasonSlot ();
      UpdateUserSlot ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void UpdateWorkOrderSlot ()
    {
      RemoveIndex (TableName.WORK_ORDER_SLOT,
                   ColumnName.MACHINE_ID,
                   "workorderbegindatetime");
      RemoveUniqueConstraint (TableName.WORK_ORDER_SLOT,
                              ColumnName.MACHINE_ID,
                              "workorderbegindatetime");
      RemoveIndex (TableName.WORK_ORDER_SLOT,
                   ColumnName.MACHINE_ID,
                   "workorderbeginday");
      RemoveIndex (TableName.WORK_ORDER_SLOT,
                   ColumnName.WORK_ORDER_ID);
      Database.RenameTable (TableName.WORK_ORDER_SLOT,
                            TableName.WORK_ORDER_SLOT + MIG_SUFFIX);
      RemoveSequence ("workorderslot_workorderslotid_seq");
      
      Database.AddTable (TableName.WORK_ORDER_SLOT,
                         new Column (ColumnName.WORK_ORDER_SLOT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.WORK_ORDER_SLOT_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("WorkOrderBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("WorkOrderBeginDay", DbType.Date, ColumnProperty.NotNull),
                         new Column ("WorkOrderEndDateTime", DbType.DateTime),
                         new Column ("WorkOrderEndDay", DbType.Date));
      Database.AddCheckConstraint ("workorderslot_notempty",
                                   TableName.WORK_ORDER_SLOT,
                                   "workorderbegindatetime <> workorderenddatetime");
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.WORK_ORDER_SLOT,
                ColumnName.MACHINE_ID,
                "workorderbegindatetime");
      AddUniqueConstraint (TableName.WORK_ORDER_SLOT,
                           ColumnName.MACHINE_ID,
                           "workorderbegindatetime");
      AddIndex (TableName.WORK_ORDER_SLOT,
                ColumnName.MACHINE_ID,
                "workorderbeginday");
      AddIndex (TableName.WORK_ORDER_SLOT,
                ColumnName.WORK_ORDER_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(workorderid, machineid, workorderbegindatetime, workorderbeginday, workorderenddatetime, workorderendday)
SELECT workorderid, machineid, workorderbegindatetime, workorderbeginday, workorderenddatetime, workorderendday
FROM {0}{1}",
                                               TableName.WORK_ORDER_SLOT,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.WORK_ORDER_SLOT + MIG_SUFFIX);
    }

    void UpdateOperationSlot ()
    {
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   "operationslotbegindatetime");
      RemoveUniqueConstraint (TableName.OPERATION_SLOT,
                              ColumnName.MACHINE_ID,
                              "operationslotbegindatetime");
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.MACHINE_ID,
                   "operationslotbeginday");
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.OPERATION_ID);
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.COMPONENT_ID);
      RemoveIndex (TableName.OPERATION_SLOT,
                   ColumnName.WORK_ORDER_ID);
      Database.RenameTable (TableName.OPERATION_SLOT,
                            TableName.OPERATION_SLOT + MIG_SUFFIX);
      RemoveSequence ("operationslot_operationslotid_seq");
      
      Database.AddTable (TableName.OPERATION_SLOT,
                         new Column (ColumnName.OPERATION_SLOT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.OPERATION_SLOT_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("OperationSlotBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("OperationSlotBeginDay", DbType.Date, ColumnProperty.NotNull),
                         new Column ("OperationSlotEndDateTime", DbType.DateTime),
                         new Column ("OperationSlotEndDay", DbType.Date),
                         new Column ("OperationSlotDuration", DbType.Double),
                         new Column ("OperationSlotRunTime", DbType.Double),
                         new Column ("OperationSlotTotalCycles", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("OperationSlotPartialCycles", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("OperationSlotDryCycles", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("OperationSlotCurrentCycleBegin", DbType.DateTime),
                         new Column ("OperationSlotAverageCycleTime", DbType.Double));
      Database.AddCheckConstraint ("operationslot_notempty",
                                   TableName.OPERATION_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  "operationslotbegindatetime",
                                                  "operationslotenddatetime"));
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                "operationslotbegindatetime");
      AddUniqueConstraint (TableName.OPERATION_SLOT,
                           ColumnName.MACHINE_ID,
                           "operationslotbegindatetime");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                "operationslotbeginday");
      AddIndexCondition (TableName.OPERATION_SLOT,
                         "operationid IS NOT NULL",
                         ColumnName.OPERATION_ID);
      AddIndexCondition (TableName.OPERATION_SLOT,
                         "componentid IS NOT NULL",
                         ColumnName.COMPONENT_ID);
      AddIndexCondition (TableName.OPERATION_SLOT,
                         "workorderid IS NOT NULL",
                         ColumnName.WORK_ORDER_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(operationid, componentid, workorderid, machineid,
 operationslotbegindatetime, operationslotbeginday, operationslotenddatetime, operationslotendday,
 operationslotduration, operationslotruntime, operationslottotalcycles, operationslotpartialcycles, operationslotdrycycles,
 operationslotcurrentcyclebegin, operationslotaveragecycletime)
SELECT operationid, componentid, workorderid, machineid,
 operationslotbegindatetime, operationslotbeginday, operationslotenddatetime, operationslotendday,
 operationslotduration, operationslotruntime, operationslottotalcycles, operationslotpartialcycles, operationslotdrycycles,
 operationslotcurrentcyclebegin, operationslotaveragecycletime
FROM {0}{1}",
                                               TableName.OPERATION_SLOT,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.OPERATION_SLOT + MIG_SUFFIX);
    }
    
    void UpdateObservationStateSlot ()
    {
      string beginDateTime = "observationstateslotbegindatetime";
      string endDateTime = "observationstateslotenddatetime";
      string beginDay = "observationstateslotbeginday";
      string endDay = "observationstateslotendday";
      
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   beginDateTime);
      RemoveUniqueConstraint (TableName.OBSERVATION_STATE_SLOT,
                              ColumnName.MACHINE_ID,
                              beginDateTime);
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID);
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   beginDay);
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.USER_ID,
                   beginDay);
      Database.RenameTable (TableName.OBSERVATION_STATE_SLOT,
                            TableName.OBSERVATION_STATE_SLOT + MIG_SUFFIX);
      RemoveSequence ("observationstateslot_observationstateslotid_seq");
      
      Database.AddTable (TableName.OBSERVATION_STATE_SLOT,
                         new Column (ColumnName.OBSERVATION_STATE_SLOT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.OBSERVATION_STATE_SLOT_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (beginDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (beginDay, DbType.Date, ColumnProperty.NotNull),
                         new Column (endDateTime, DbType.DateTime),
                         new Column (endDay, DbType.Date),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.USER_ID, DbType.Int32));
      Database.AddCheckConstraint ("observationstateslot_notempty",
                                   TableName.OBSERVATION_STATE_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  beginDateTime, endDateTime));
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                beginDateTime);
      AddUniqueConstraint (TableName.OBSERVATION_STATE_SLOT,
                           ColumnName.MACHINE_ID,
                           beginDateTime);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                beginDay);
      AddIndexCondition (TableName.OBSERVATION_STATE_SLOT,
                         string.Format ("{0} IS NOT NULL", ColumnName.USER_ID),
                         ColumnName.USER_ID,
                         beginDay);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(machineid, observationstateslotbegindatetime, observationstateslotbeginday,
 observationstateslotenddatetime, observationstateslotendday,
 machineobservationstateid, userid)
SELECT machineid, observationstateslotbegindatetime, observationstateslotbeginday,
 observationstateslotenddatetime, observationstateslotendday,
 machineobservationstateid, userid
FROM {0}{1}",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.OBSERVATION_STATE_SLOT + MIG_SUFFIX);
    }
    
    void UpdateReasonSlot ()
    {
      string beginDateTime = "reasonslotbegindatetime";
      string endDateTime = "reasonslotenddatetime";
      string beginDay = "reasonslotbeginday";
      string endDay = "reasonslotendday";
      
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   beginDateTime);
      RemoveUniqueConstraint (TableName.REASON_SLOT,
                              ColumnName.MACHINE_ID,
                              beginDateTime);
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID);
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   beginDay);
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   endDateTime);
      RemoveUniqueConstraint (TableName.REASON_SLOT,
                              ColumnName.MACHINE_ID,
                              endDateTime);
      Database.RenameTable (TableName.REASON_SLOT,
                            TableName.REASON_SLOT + MIG_SUFFIX);
      RemoveSequence ("reasonslot_reasonslotid_seq");
      
      Database.AddTable (TableName.REASON_SLOT,
                         new Column (ColumnName.REASON_SLOT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.REASON_SLOT_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (beginDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (beginDay, DbType.Date, ColumnProperty.NotNull),
                         new Column (endDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (endDay, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ReasonSlotDefaultReason", DbType.Boolean, ColumnProperty.NotNull),
                         new Column ("ReasonSlotOverwriteRequired", DbType.Boolean, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_DETAILS, DbType.String));
      Database.AddCheckConstraint ("reasonslot_notempty",
                                   TableName.REASON_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  beginDateTime, endDateTime));
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                beginDateTime);
      AddUniqueConstraint (TableName.REASON_SLOT,
                           ColumnName.MACHINE_ID,
                           beginDateTime);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                beginDay);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                endDateTime);
      AddUniqueConstraint (TableName.REASON_SLOT,
                           ColumnName.MACHINE_ID,
                           endDateTime);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(machineid, reasonslotbegindatetime, reasonslotbeginday, reasonslotenddatetime, reasonslotendday,
 machinemodeid, reasonid, machineobservationstateid, reasonslotdefaultreason, reasonslotoverwriterequired, reasondetails)
SELECT machineid, reasonslotbegindatetime, reasonslotbeginday, reasonslotenddatetime, reasonslotendday,
 machinemodeid, reasonid, machineobservationstateid, reasonslotdefaultreason, reasonslotoverwriterequired, reasondetails
FROM {0}{1}",
                                               TableName.REASON_SLOT,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.REASON_SLOT + MIG_SUFFIX);
      
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW reasonslotboolean AS
 SELECT reasonslot.reasonslotid AS reasonslotid,
    reasonslot.reasonslotversion AS reasonslotversion,
    reasonslot.machineid AS machineid,
    reasonslot.reasonslotbegindatetime AS reasonslotbegindatetime,
    reasonslot.reasonslotbeginday AS reasonslotbeginday,
    reasonslot.reasonslotenddatetime AS reasonslotenddatetime,
    reasonslot.reasonslotendday AS reasonslotendday,
    reasonslot.machinemodeid AS machinemodeid,
    reasonslot.reasonid AS reasonid,
    reasonslot.machineobservationstateid AS machineobservationstateid,
        CASE
            WHEN reasonslot.reasonslotdefaultreason = false THEN 0
            WHEN reasonslot.reasonslotdefaultreason = true  THEN 1
        END AS reasonslotdefaultreason,
        CASE
            WHEN reasonslot.reasonslotoverwriterequired = false THEN 0
            WHEN reasonslot.reasonslotoverwriterequired = true  THEN 1
        END AS reasonslotoverwriterequired,
    reasonslot.reasondetails AS reasondetails
 FROM reasonslot;");
    }

    void UpdateUserSlot ()
    {
      string beginDateTime = "userslotbegindatetime";
      string endDateTime = "userslotenddatetime";
      string beginDay = "userslotbeginday";
      string endDay = "userslotendday";
      
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   beginDateTime);
      RemoveUniqueConstraint (TableName.USER_SLOT,
                              ColumnName.USER_ID,
                              beginDateTime);
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID);
      RemoveIndex (TableName.USER_SLOT,
                   ColumnName.USER_ID,
                   beginDay);
      Database.RenameTable (TableName.USER_SLOT,
                            TableName.USER_SLOT + MIG_SUFFIX);
      RemoveSequence ("userslot_userslotid_seq");
      
      Database.AddTable (TableName.USER_SLOT,
                         new Column (ColumnName.USER_SLOT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.USER_SLOT_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (beginDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (beginDay, DbType.Date, ColumnProperty.NotNull),
                         new Column (endDateTime, DbType.DateTime),
                         new Column (endDay, DbType.Date));
      Database.AddCheckConstraint ("userslot_notempty",
                                   TableName.USER_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  beginDateTime, endDateTime));
      Database.GenerateForeignKey (TableName.USER_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                beginDateTime);
      AddUniqueConstraint (TableName.USER_SLOT,
                           ColumnName.USER_ID,
                           beginDateTime);
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID);
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                beginDay);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(userid, userslotbegindatetime, userslotbeginday, userslotenddatetime, userslotendday)
SELECT userid, userslotbegindatetime, userslotbeginday, userslotenddatetime, userslotendday
FROM {0}{1}",
                                               TableName.USER_SLOT,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.USER_SLOT + MIG_SUFFIX);
    }
  }
}
