// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Migrator.Framework;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 277: Add the table task
  /// </summary>
  [Migration (277)]
  public class AddTask : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddTask).FullName);

    // Keep the options until all customers were upgraded to version >= 19.0.0
    // And after version >= 19.0.0 was installed at new customers
    static readonly string USE_DEPRECATED_TASK_STATUS_KEY = "Migration.UseDeprecatedTaskStatus";
    static readonly bool USE_DEPRECATED_TASK_STATUS_DEFAULT = false;

    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddTaskStatus ();
      AddTaskStatusValues ();
      AddTaskFullTable ();
      AddTaskView ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_IMPLEMENTATION)) {
        Database.RemoveTable (TableName.MANUFACTURING_ORDER_IMPLEMENTATION);
        var sequenceName = string.Format ("{0}_{0}id_seq", TableName.MANUFACTURING_ORDER);
        RemoveSequence (sequenceName);
      }
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_STATUS)) {
        Database.RemoveTable (TableName.MANUFACTURING_ORDER_STATUS);
      }
      Database.ExecuteNonQuery ("""
        DELETE FROM translation
        WHERE translationkey LIKE 'ManufacturingOrderStatus%';
        """);
        
      // Keep the next lines until all customers were upgraded to version >= 19.0.0
      // And after version >= 19.0.0 was installed at new customers
      if (Database.TableExists (TableName.TASK_FULL)) {
        Database.RemoveTable (TableName.TASK_FULL);
        var sequenceName = string.Format ("{0}_{0}id_seq", TableName.TASK);
        RemoveSequence (sequenceName);
      }
      if (Database.TableExists (TableName.TASK_STATUS)) {
        Database.RemoveTable (TableName.TASK_STATUS);
      }
      Database.ExecuteNonQuery ("""
        DELETE FROM translation
        WHERE translationkey LIKE 'TaskStatus%';
        """);
    }

    void AddTaskStatus ()
    {
      var tableName =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_STATUS_KEY, USE_DEPRECATED_TASK_STATUS_DEFAULT)
        ? TableName.TASK_STATUS
        : TableName.MANUFACTURING_ORDER_STATUS;

      Database.AddTable (tableName,
                         new Column (tableName + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (tableName + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (tableName + "name", DbType.String, ColumnProperty.Unique),
                         new Column (tableName + "translationkey", DbType.String, ColumnProperty.Unique),
                         new Column (tableName + "color", DbType.String, 7, ColumnProperty.NotNull));
      MakeColumnCaseInsensitive (tableName,
                                 tableName + "name");
      AddConstraintColor (tableName,
                          tableName + "color");
      AddConstraintNameTranslationKey (tableName,
                                       tableName + "name",
                                       tableName + "translationkey");
    }

    void AddTaskStatusValues ()
    {
      var taskStatus =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_STATUS_KEY, USE_DEPRECATED_TASK_STATUS_DEFAULT)
        ? TableName.TASK_STATUS
        : TableName.MANUFACTURING_ORDER_STATUS;
      var prefix =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_STATUS_KEY, USE_DEPRECATED_TASK_STATUS_DEFAULT)
        ? "TaskStatus"
        : "ManufacturingOrderStatus";

      // 1: New (gray)
      Database.Insert (TableName.TRANSLATION,
                       new string[] { ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE },
                       new string[] { "", $"{prefix}New", "New" });
      Database.Insert (taskStatus,
                       new string[] { taskStatus + "id", taskStatus + "translationkey", taskStatus + "color" },
                       new string[] { "1", $"{prefix}New", "#808080" });
      // 2: Ready (light blue)
      Database.Insert (TableName.TRANSLATION,
                       new string[] { ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE },
                       new string[] { "", $"{prefix}Ready", "Ready" });
      Database.Insert (taskStatus,
                       new string[] { taskStatus + "id", taskStatus + "translationkey", taskStatus + "color" },
                       new string[] { "2", $"{prefix}Ready", "#ADD8E6" });
      // 3: Running (violet)
      Database.Insert (TableName.TRANSLATION,
                       new string[] { ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE },
                       new string[] { "", $"{prefix}Running", "Running" });
      Database.Insert (taskStatus,
                       new string[] { taskStatus + "id", taskStatus + "translationkey", taskStatus + "color" },
                       new string[] { "3", $"{prefix}Running", "#EE82EE" });
      // 4: Completed (green)
      Database.Insert (TableName.TRANSLATION,
                       new string[] { ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE },
                       new string[] { "", $"{prefix}Complete", "Complete" });
      Database.Insert (taskStatus,
                       new string[] { taskStatus + "id", taskStatus + "translationkey", taskStatus + "color" },
                       new string[] { "4", $"{prefix}Complete", "#008000" });
      // 5: Hold (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string[] { ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE },
                       new string[] { "", $"{prefix}Hold", "Hold" });
      Database.Insert (taskStatus,
                       new string[] { taskStatus + "id", taskStatus + "translationkey", taskStatus + "color" },
                       new string[] { "5", $"{prefix}Hold", "#FFFF00" });

      SetSequence (taskStatus,
                   taskStatus + "id",
                   100);
    }

    void AddTaskFullTable ()
    {
      var taskStatus =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_STATUS_KEY, USE_DEPRECATED_TASK_STATUS_DEFAULT)
        ? TableName.TASK_STATUS
        : TableName.MANUFACTURING_ORDER_STATUS;
      var taskView =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)
        ? TableName.TASK
        : TableName.MANUFACTURING_ORDER;
      var taskTable =
        Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)
        ? TableName.TASK_FULL
        : TableName.MANUFACTURING_ORDER_IMPLEMENTATION;

      Database.AddTable (taskTable,
                         new Column (taskView + "id", DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (taskView + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (taskView + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (taskView + "externalcode", DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (taskStatus + "id", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (taskView + "quantity", DbType.Int32, ColumnProperty.Null),
                         new Column (taskView + "setupduration", DbType.Int32, ColumnProperty.Null), // if null, taken from the operation
                         new Column (taskView + "cycleduration", DbType.Int32, ColumnProperty.Null), // if null, taken from the operation
                         new Column (taskView + "duedatetime", DbType.DateTime, ColumnProperty.Null),
                         new Column (taskView + "order", DbType.Double, ColumnProperty.Null),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.Null));
      string sequenceName = string.Format ("{0}_{0}id_seq", taskView);
      AddSequence (sequenceName);
      SetSequence (taskTable,
                   taskView + "id",
                   sequenceName);
      AddTimeStampTrigger (taskTable,
                           taskView + "timestamp");
      AddUniqueConstraint (taskTable,
                           taskView + "externalcode");
      Database.GenerateForeignKey (taskTable, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (taskTable, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (taskTable, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (taskTable, taskStatus + "id",
                                   taskStatus, taskStatus + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (taskTable, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddIndex (taskTable,
                ColumnName.MACHINE_ID,
                taskView + "order",
                taskView + "duedatetime");
      AddIndex (taskTable,
                ColumnName.OPERATION_ID);
      AddIndex (taskTable,
                ColumnName.COMPONENT_ID);
      AddIndex (taskTable,
                ColumnName.WORK_ORDER_ID);
    }

    void AddTaskView ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)
) {
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE VIEW {TableName.TASK} AS
          SELECT *
          FROM {TableName.TASK_FULL};
          """);
      }
      else {
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE VIEW {TableName.MANUFACTURING_ORDER} AS
          SELECT *
          FROM {TableName.MANUFACTURING_ORDER_IMPLEMENTATION};
          """);
      }
    }
  }
}
