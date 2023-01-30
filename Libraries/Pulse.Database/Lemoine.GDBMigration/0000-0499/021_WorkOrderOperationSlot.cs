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
  /// Migration 021: Add the following analysis tables
  /// <item>WorkOrderSlot</item>
  /// <item>OperationSlot</item>
  /// </summary>
  [Migration(21)]
  public class WorkOrderOperationSlot: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderOperationSlot).FullName);
    
    static readonly string WORK_ORDER_SLOT_TABLE = "WorkOrderSlot";
    static readonly string OPERATION_SLOT_TABLE = "OperationSlot";
    static readonly string WORK_ORDER_TABLE = "WorkOrder";
    static readonly string WORK_ORDER_ID = "WorkOrderId";
    static readonly string COMPONENT_TABLE = "Component";
    static readonly string COMPONENT_ID = "ComponentId";
    static readonly string OPERATION_TABLE = "Operation";
    static readonly string OPERATION_ID = "OperationId";
    static readonly string MACHINE_TABLE = "Machine";
    static readonly string MACHINE_ID = "MachineId";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (WORK_ORDER_SLOT_TABLE)) {
        Database.AddTable (WORK_ORDER_SLOT_TABLE,
                           new Column (WORK_ORDER_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("WorkOrderBeginDateTime", DbType.DateTime, ColumnProperty.PrimaryKey),
                           new Column ("WorkOrderBeginDay", DbType.Date, ColumnProperty.NotNull),
                           new Column ("WorkOrderEndDateTime", DbType.DateTime),
                           new Column ("WorkOrderEndDay", DbType.Date));
        Database.AddCheckConstraint ("workorderslot_notempty",
                                     WORK_ORDER_SLOT_TABLE,
                                     "workorderbegindatetime <> workorderenddatetime");
        Database.AddUniqueConstraint ("workorderslot_unique_machine_datetime",
                                     WORK_ORDER_SLOT_TABLE,
                                     new string[] {"machineid", "workorderbegindatetime"});
        Database.GenerateForeignKey (WORK_ORDER_SLOT_TABLE, WORK_ORDER_ID,
                                     WORK_ORDER_TABLE, WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (WORK_ORDER_SLOT_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (@"CREATE INDEX workorderslot_machine_beginday
ON WorkOrderSlot
USING btree (machineid, workorderbeginday);");
        Database.ExecuteNonQuery (@"CREATE INDEX workorderslot_workorderid
ON WorkOrderSlot
USING btree (WorkOrderId);");
      }
      
      if (!Database.TableExists (OPERATION_SLOT_TABLE)) {
        Database.AddTable (OPERATION_SLOT_TABLE,
                           new Column (OPERATION_ID, DbType.Int32),
                           new Column (COMPONENT_ID, DbType.Int32),
                           new Column (WORK_ORDER_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("OperationSlotBeginDateTime", DbType.DateTime, ColumnProperty.PrimaryKey),
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
        Database.GenerateForeignKey (OPERATION_SLOT_TABLE, OPERATION_ID,
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (OPERATION_SLOT_TABLE, COMPONENT_ID,
                                     COMPONENT_TABLE, COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
        Database.GenerateForeignKey (OPERATION_SLOT_TABLE, WORK_ORDER_ID,
                                     WORK_ORDER_TABLE, WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
        Database.GenerateForeignKey (OPERATION_SLOT_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (@"CREATE INDEX operationslot_machine_beginday
ON OperationSlot
USING btree (MachineId, OperationSlotBeginDay);");
        Database.ExecuteNonQuery (@"CREATE INDEX operationslot_operationid
ON OperationSlot
USING btree (OperationId) 
WHERE OperationId IS NOT NULL;");
        Database.ExecuteNonQuery (@"CREATE INDEX operationslot_componentid
ON OperationSlot
USING btree (ComponentId) 
WHERE ComponentId IS NOT NULL;");
        Database.ExecuteNonQuery (@"CREATE INDEX operationslot_workorderid
ON OperationSlot
USING btree (WorkOrderId) 
WHERE WorkOrderId IS NOT NULL;");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (WORK_ORDER_SLOT_TABLE)) {
        Database.RemoveTable (WORK_ORDER_SLOT_TABLE);
      }
      if (Database.TableExists (OPERATION_SLOT_TABLE)) {
        Database.RemoveTable (OPERATION_SLOT_TABLE);
      }
    }
  }
}
