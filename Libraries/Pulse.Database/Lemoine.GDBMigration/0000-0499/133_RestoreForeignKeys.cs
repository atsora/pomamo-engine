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
  /// Migration 133: Restore some foreign keys that disappeared
  /// </summary>
  [Migration(133)]
  public class RestoreForeignKeys: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RestoreForeignKeys).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Clean some tables first
      CleanTables ();
      
      // machineactivitysummary.machineid
      // machineactivitysummary.machinemodeid
      // machineactivitysummary.machineobservationstateid
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // machinecellupdate.newcellid
      Database.AddForeignKey ("fk_machinecellupdate_newcell",
                              TableName.MACHINE_CELL_UPDATE, "newcellid",
                              TableName.CELL, ColumnName.CELL_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      // machinecompanyupdate.newcompanyid
      Database.AddForeignKey ("fk_machinecompanyupdate_newcompany",
                              TableName.MACHINE_COMPANY_UPDATE, "newcompanyid",
                              TableName.COMPANY, ColumnName.COMPANY_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      // machinedepartmentupdate.newdepartmentid
      Database.AddForeignKey ("fk_machinedepartmentupdate_newdepartment",
                              TableName.MACHINE_DEPARTMENT_UPDATE, "newdepartmentid",
                              TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      // machinestatus.machinemodeid (vs cncmachinemodeid)
      Database.RemoveForeignKey (TableName.MACHINE_STATUS,
                                 "fk_machinestatus_machinemode");
      Database.AddForeignKey ("fk_machinestatus_cncmachinemode",
                              TableName.MACHINE_STATUS, "cncmachinemodeid",
                              TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddForeignKey ("fk_machinestatus_machinemode",
                              TableName.MACHINE_STATUS, ColumnName.MACHINE_MODE_ID,
                              TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      // projectcomponentupdate.newprojectid
      Database.AddForeignKey ("fk_projectcomponentupdate_newdproject",
                              TableName.PROJECT_COMPONENT_UPDATE, "newprojectid",
                              TableName.PROJECT, ColumnName.PROJECT_ID,
                              Migrator.Framework.ForeignKeyConstraint.Restrict);
      
      // observationstateslot.machineid
      // observationstateslot.machineobservationstateid
      // observationstateslot.userid
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // operationcycledeliverablepiece.deliverablepieceid
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                                   ColumnName.DELIVERABLE_PIECE_ID,
                                   TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // operationslot.operationid
      // operationslot.componentid
      // operationslot.workorderid
      // operationslot.machineid
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
      
      // reasonslot.machineid
      // reasonslot.machinemodeid
      // reasonslot.machineobservationstateid
      // reasonslot.reasonid
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

      // reasonsummary.machineid
      // reasonsummary.machineobservationstateid
      // reasonsummary.reasonid
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // stampingvalue.fieldid
      Database.GenerateForeignKey (TableName.STAMPING_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);


      // userslot.userid
      Database.GenerateForeignKey (TableName.USER_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // workorderslot.wororderid
      // workorderslot.machineid
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void CleanTables ()
    {
      // operationslot
      Database.ExecuteNonQuery (@"DELETE FROM operationslot
WHERE operationid IS NOT NULL AND workorderid IS NULL AND componentid IS NULL
  AND NOT EXISTS (SELECT 1 FROM operation WHERE operation.operationid=operationslot.operationid)");
      Database.ExecuteNonQuery (@"UPDATE operationslot
SET operationid=NULL
WHERE operationid IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM operation WHERE operation.operationid=operationslot.operationid)");
      Database.ExecuteNonQuery (@"UPDATE operationslot
SET componentid=NULL
WHERE componentid IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM component WHERE component.componentid=operationslot.componentid)");
      Database.ExecuteNonQuery (@"UPDATE operationslot
SET workorderid=NULL
WHERE workorderid IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM workorder WHERE workorder.workorderid=operationslot.workorderid)");
      
      // workorderslot
      Database.ExecuteNonQuery (@"DELETE FROM workorderslot
WHERE workorderid IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM workorder WHERE workorder.workorderid=workorderslot.workorderid)");
      
      // machineactivitysummary and reasonsummary
      Database.ExecuteNonQuery (@"DELETE FROM machineactivitysummary
WHERE NOT EXISTS (SELECT 1 FROM monitoredmachine WHERE machineactivitysummary.machineid=monitoredmachine.machineid)");
      Database.ExecuteNonQuery (@"DELETE FROM reasonsummary
WHERE NOT EXISTS (SELECT 1 FROM monitoredmachine WHERE reasonsummary.machineid=monitoredmachine.machineid)");

      // observationstateslot
      Database.ExecuteNonQuery (@"UPDATE observationstateslot
SET userid=NULL
WHERE userid NOT IN (SELECT userid FROM usertable)");
      Database.ExecuteNonQuery (@"DELETE FROM observationstateslot
WHERE machineid NOT IN (SELECT machineid FROM machine)");

      // reasonslot
      Database.ExecuteNonQuery (@"DELETE FROM observationstateslot
WHERE machineid NOT IN (SELECT machineid FROM machine)");
    }
  }
}
