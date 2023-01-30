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
  /// Migration 277: Add the table task
  /// </summary>
  [Migration(277)]
  public class AddTask: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddTask).FullName);
    
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
      Database.RemoveTable (TableName.TASK_FULL);
      string sequenceName = string.Format ("{0}_{0}id_seq", TableName.TASK);
      RemoveSequence (sequenceName);
      Database.RemoveTable (TableName.TASK_STATUS);
      Database.ExecuteNonQuery (@"DELETE FROM translation
WHERE translationkey LIKE 'TaskStatus%'");
    }
    
    void AddTaskStatus ()
    {
      Database.AddTable (TableName.TASK_STATUS,
                         new Column (TableName.TASK_STATUS + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.TASK_STATUS + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.TASK_STATUS + "name", DbType.String, ColumnProperty.Unique),
                         new Column (TableName.TASK_STATUS + "translationkey", DbType.String, ColumnProperty.Unique),
                         new Column (TableName.TASK_STATUS + "color", DbType.String, 7, ColumnProperty.NotNull));
      MakeColumnCaseInsensitive (TableName.TASK_STATUS,
                                 TableName.TASK_STATUS + "name");
      AddConstraintColor (TableName.TASK_STATUS,
                          TableName.TASK_STATUS + "color");
      AddConstraintNameTranslationKey (TableName.TASK_STATUS,
                                       TableName.TASK_STATUS + "name",
                                       TableName.TASK_STATUS + "translationkey");
    }
    
    void AddTaskStatusValues ()
    {
      // 1: New (gray)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "TaskStatusNew", "New"});
      Database.Insert (TableName.TASK_STATUS,
                       new string [] {TableName.TASK_STATUS + "id", TableName.TASK_STATUS + "translationkey", TableName.TASK_STATUS + "color"},
                       new string [] {"1", "TaskStatusNew", "#808080"});
      // 2: Ready (light blue)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "TaskStatusReady", "Ready"});
      Database.Insert (TableName.TASK_STATUS,
                       new string [] {TableName.TASK_STATUS + "id", TableName.TASK_STATUS + "translationkey", TableName.TASK_STATUS + "color"},
                       new string [] {"2", "TaskStatusReady", "#ADD8E6"});
      // 3: Running (violet)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "TaskStatusRunning", "Running"});
      Database.Insert (TableName.TASK_STATUS,
                       new string [] {TableName.TASK_STATUS + "id", TableName.TASK_STATUS + "translationkey", TableName.TASK_STATUS + "color"},
                       new string [] {"3", "TaskStatusRunning", "#EE82EE"});
      // 4: Completed (green)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "TaskStatusComplete", "Complete"});
      Database.Insert (TableName.TASK_STATUS,
                       new string [] {TableName.TASK_STATUS + "id", TableName.TASK_STATUS + "translationkey", TableName.TASK_STATUS + "color"},
                       new string [] {"4", "TaskStatusComplete", "#008000"});
      // 5: Hold (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "TaskStatusHold", "Hold"});
      Database.Insert (TableName.TASK_STATUS,
                       new string [] {TableName.TASK_STATUS + "id", TableName.TASK_STATUS + "translationkey", TableName.TASK_STATUS + "color"},
                       new string [] {"5", "TaskStatusHold", "#FFFF00"});

      SetSequence (TableName.TASK_STATUS,
                   TableName.TASK_STATUS + "id",
                   100);
    }
    
    void AddTaskFullTable ()
    {
      Database.AddTable (TableName.TASK_FULL,
                         new Column (TableName.TASK + "id", DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (TableName.TASK + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.TASK + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (TableName.TASK + "externalcode", DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.TASK_STATUS + "id", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.TASK + "quantity", DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.TASK + "setupduration", DbType.Int32, ColumnProperty.Null), // if null, taken from the operation
                         new Column (TableName.TASK + "cycleduration", DbType.Int32, ColumnProperty.Null), // if null, taken from the operation
                         new Column (TableName.TASK + "duedatetime", DbType.DateTime, ColumnProperty.Null),
                         new Column (TableName.TASK + "order", DbType.Double, ColumnProperty.Null),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.Null));
      string sequenceName = string.Format ("{0}_{0}id_seq", TableName.TASK);
      AddSequence (sequenceName);
      SetSequence (TableName.TASK_FULL,
                   TableName.TASK + "id",
                   sequenceName);
      AddTimeStampTrigger (TableName.TASK_FULL,
                           TableName.TASK + "timestamp");
      AddUniqueConstraint (TableName.TASK_FULL,
                           TableName.TASK + "externalcode");
      Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.TASK_FULL, TableName.TASK_STATUS + "id",
                                   TableName.TASK_STATUS, TableName.TASK_STATUS + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.TASK_FULL, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddIndex (TableName.TASK_FULL,
                ColumnName.MACHINE_ID,
                TableName.TASK + "order",
                TableName.TASK + "duedatetime");
      AddIndex (TableName.TASK_FULL,
                ColumnName.OPERATION_ID);
      AddIndex (TableName.TASK_FULL,
                ColumnName.COMPONENT_ID);
      AddIndex (TableName.TASK_FULL,
                ColumnName.WORK_ORDER_ID);
    }
    
    void AddTaskView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW task AS
SELECT *
FROM taskfull");
    }
  }
}
