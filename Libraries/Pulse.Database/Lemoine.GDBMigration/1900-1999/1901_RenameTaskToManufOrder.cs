// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1901: rename table task to manuforder for manufacturing order
  /// </summary>
  [Migration (1901)]
  public class RenameTaskToManufOrder : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RenameTaskToManufOrder).FullName);

    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.TASK_FULL)) {
        Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS task CASCADE");

        // Remove unique constraints and foreign keys
        RemoveUniqueConstraint (TableName.TASK_FULL, "taskexternalcode");
        foreach (var suffix in new string[] { "component", "machine", "operation", "taskstatus", "workorder" }) {
          Database.RemoveForeignKey (TableName.TASK_FULL, $"fk_taskfull_{suffix}");
        }
        RemoveTimeStampTrigger (TableName.TASK_FULL);
        Database.ExecuteNonQuery ("""
          DROP FUNCTION IF EXISTS public.taskfullid(taskfull)
          """);

        // Rename taskfull into manuforder1
        Database.RenameTable (TableName.TASK_FULL, TableName.MANUFACTURING_ORDER_IMPLEMENTATION); // Local implementation of manufacturingorder
        foreach (var suffix in new string[] { "id", "version", "timestamp", "externalcode", "statusid", "quantity", "setupduration", "cycleduration", "duedatetime", "order" }) {
          Database.RenameColumn (TableName.MANUFACTURING_ORDER_IMPLEMENTATION,
            $"{TableName.TASK}{suffix}",
            $"{TableName.MANUFACTURING_ORDER}{suffix}");
        }

        AddUniqueConstraint (TableName.MANUFACTURING_ORDER_IMPLEMENTATION,
          $"{TableName.MANUFACTURING_ORDER}externalcode");

        // Indexes
        foreach (var column in new string[] { "componentid", "operationid", "workorderid" }) {
          Database.ExecuteNonQuery ($"""
          ALTER INDEX taskfull_{column}_idx
          RENAME TO manuforder1_{column}_idx;
          """);
        }
        Database.ExecuteNonQuery ($"""
          ALTER INDEX taskfull_pkey
          RENAME TO manuforder1_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX taskfull_machineid_taskorder_taskduedatetime_idx
          RENAME TO manuforder1_machineid_manuforderorder_manuforderduedatetime_idx;
          """);

        // Foreign keys
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, ColumnName.OPERATION_ID,
                                     TableName.OPERATION, ColumnName.OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, ColumnName.COMPONENT_ID,
                                     TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, ColumnName.WORK_ORDER_ID,
                                     TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, TableName.MANUFACTURING_ORDER_STATUS + "id",
                                     TableName.MANUFACTURING_ORDER_STATUS, TableName.MANUFACTURING_ORDER_STATUS + "id",
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);

        // Rename sequence
        Database.ExecuteNonQuery ($"""
          ALTER SEQUENCE task_taskid_seq
          RENAME TO manuforder_manuforderid_seq;
          """);

        // Rename functions: taskfull_timestamp_update, taskfulldisplay, taskfullid
        AddTimeStampTrigger (TableName.MANUFACTURING_ORDER_IMPLEMENTATION,
                             TableName.MANUFACTURING_ORDER + "timestamp");
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE FUNCTION public.{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}id({TableName.MANUFACTURING_ORDER_IMPLEMENTATION})
           RETURNS integer
           LANGUAGE sql
           IMMUTABLE
          AS $function$SELECT $1.{ColumnName.MANUFACTURING_ORDER_ID}$function$
          ;
          """);

        // Display
        Database.ExecuteNonQuery ($"""
          UPDATE display
          SET displaytable='{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}'
          WHERE displaytable='{TableName.TASK_FULL}';
          """);

        // View
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE VIEW {TableName.MANUFACTURING_ORDER} AS
          SELECT *, {TableName.MANUFACTURING_ORDER_IMPLEMENTATION}.{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}display AS {TableName.MANUFACTURING_ORDER}display
          FROM {TableName.MANUFACTURING_ORDER_IMPLEMENTATION};
          """);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        // View / display
        Database.ExecuteNonQuery ($"""
          DROP VIEW IF EXISTS {TableName.MANUFACTURING_ORDER};
          """);

        // Remove unique constraints and foreign keys
        RemoveUniqueConstraint (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, "taskexternalcode");
        foreach (var suffix in new string[] { "component", "machine", "operation", "taskstatus", "workorder" }) {
          Database.RemoveForeignKey (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, $"fk_{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}_{suffix}");
        }
        RemoveTimeStampTrigger (TableName.MANUFACTURING_ORDER_IMPLEMENTATION);
        Database.ExecuteNonQuery ($"""
          DROP FUNCTION IF EXISTS public.{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}id({TableName.MANUFACTURING_ORDER_IMPLEMENTATION})
          """);

        // Table
        // Rename manuforder1 into taskfull
        Database.RenameTable (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, TableName.TASK_FULL); // Local implementation of manufacturingorder
        foreach (var suffix in new string[] { "id", "version", "timestamp", "externalcode", "statusid", "quantity", "setupduration", "cycleduration", "duedatetime", "order" }) {
          Database.RenameColumn (TableName.TASK_FULL,
            $"{TableName.MANUFACTURING_ORDER}{suffix}",
            $"{TableName.TASK}{suffix}");
        }

        AddUniqueConstraint (TableName.TASK_FULL,
          $"{TableName.TASK_FULL}externalcode");

        // Indexes
        foreach (var column in new string[] { "componentid", "operationid", "workorderid" }) {
          Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforder1_{column}_idx
          RENAME TO taskfull_{column}_idx;
          """);
        }
        Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforder1_pkey
          RENAME TO taskfull_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforder1_machineid_taskorder_taskduedatetime_idx
          RENAME TO taskfull_machineid_manuforderorder_manuforderduedatetime_idx;
          """);

        // Foreign keys
        Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.OPERATION_ID,
                                     TableName.OPERATION, ColumnName.OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.COMPONENT_ID,
                                     TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.WORK_ORDER_ID,
                                     TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.TASK_FULL, TableName.MANUFACTURING_ORDER_STATUS + "id",
                                     TableName.MANUFACTURING_ORDER_STATUS, TableName.MANUFACTURING_ORDER_STATUS + "id",
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);

        // Rename sequence
        Database.ExecuteNonQuery ($"""
          ALTER SEQUENCE manuforder_manuforderid_seq
          RENAME TO task_taskid_seq;
          """);

        // Rename functions: taskfull_timestamp_update, taskfulldisplay, taskfullid
        AddTimeStampTrigger (TableName.TASK_FULL,
                             TableName.TASK + "timestamp");
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE FUNCTION public.{TableName.TASK_FULL}id({TableName.TASK_FULL})
           RETURNS integer
           LANGUAGE sql
           IMMUTABLE
          AS $function$SELECT $1.{ColumnName.TASK_ID}$function$
          ;
          """);

        // Display
        Database.ExecuteNonQuery ($"""
          UPDATE display
          SET displaytable='{TableName.TASK_FULL}'
          WHERE displaytable='{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}';
          """);

        // View
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE VIEW {TableName.TASK} AS
          SELECT *, {TableName.TASK_FULL}.{TableName.TASK_FULL}display AS ${TableName.TASK}display
          FROM {TableName.TASK_FULL};
          """);
      }
    }

  }
}
