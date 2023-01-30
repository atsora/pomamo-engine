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
  /// Migration 317: simplify alarm tables
  /// </summary>
  [Migration(317)]
  public class UpdateAlarmTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateAlarmTables).FullName);
    static readonly string ALARM_CLASS_NAME = "alarmclass" + "name";
    static readonly string ALARM_NUMBER = "alarm" + "number";
    static readonly string ALARM_MESSAGE = "alarm" + "message";
    static readonly string ALARM_CNCINFO = "alarm" + "cncinfo";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      DeleteOldTables();
      CreateNewTable();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      DeleteNewTable();
      RestoreOldTables();
    }
    
    void DeleteOldTables()
    {
      // Migration 170: down
      Database.RemoveTable ("alarmslotalarm");
      Database.RemoveTable ("alarmslot");
      
      // Migration 134: down
      Database.RemoveTable ("alarmclass");
      Database.RemoveTable ("alarm");
    }
    
    void CreateNewTable()
    {
      Database.AddTable(TableName.CNC_ALARM,
                        new Column(ColumnName.CNC_ALARM_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.CNC_ALARM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM + "cncinfo", DbType.String),
                        new Column(TableName.CNC_ALARM + "type", DbType.String),
                        new Column(TableName.CNC_ALARM + "number", DbType.Int32),
                        new Column(TableName.CNC_ALARM + "message", DbType.String),
                        new Column(TableName.CNC_ALARM + "properties", DbType.Int32),
                        new Column(TableName.CNC_ALARM + "datetimerange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange(TableName.CNC_ALARM, TableName.CNC_ALARM + "datetimerange");
      MakeColumnJson(TableName.CNC_ALARM, TableName.CNC_ALARM + "properties");
    }
    
    void RestoreOldTables()
    {
      // Migration 134: up
      Database.AddTable ("alarmclass",
                         new Column ("alarmclassid", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("alarmclass" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ALARM_CLASS_NAME, DbType.String, ColumnProperty.NotNull));
      AddIndex("alarmclass", "alarmclassid");
      AddUniqueConstraint("alarmclass", ALARM_CLASS_NAME);
      Database.AddTable ("alarm",
                         new Column ("alarmid", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("alarm" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ALARM_NUMBER, DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (ALARM_MESSAGE, DbType.String),
                         new Column (ALARM_CNCINFO, DbType.String),
                         new Column ("alarmclassid", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.GenerateForeignKey ("alarm", "alarmclassid",
                                   "alarmclass", "alarmclassid",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex("alarm", "alarmid");
      AddUniqueConstraint("alarm", ALARM_NUMBER, ALARM_MESSAGE, ALARM_CNCINFO, "alarmclassid");
      
      // Migration 170: up
      Database.AddTable ("alarmslot",
                         new Column ("alarmslot" + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("alarmslot" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("alarmslot" + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("alarmslot" + "end", DbType.DateTime, ColumnProperty.Null));
      Database.GenerateForeignKey ("alarmslot", ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint ("alarmslot", ColumnName.MACHINE_MODULE_ID, "alarmslot" + "begin"); // Also used as an index
      AddUniqueConstraint ("alarmslot", ColumnName.MACHINE_MODULE_ID, "alarmslot" + "end"); // Also used as an index
      Database.AddTable ("alarmslotalarm",
                         new Column("alarmslotalarm" + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column("alarmslotalarm" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("alarmslot" + "id", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("alarm" + "id", DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey ("alarmslotalarm", "alarmslot" + "id",
                                   "alarmslot", "alarmslot" + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey ("alarmslotalarm", "alarm" + "id",
                                   "alarm", "alarm" + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint ("alarmslotalarm_SecondaryKey",
                             "alarmslotalarm",
                             new string [] { "alarmslot" + "id",
                               "alarm" + "id"});
      AddIndex ("alarmslotalarm", "alarmslot" + "id");
      AddIndex ("alarmslotalarm", "alarm" +"id");
      
      // Migration 175: up
      Database.AddColumn ("alarmslotalarm",
                          new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.Null));
      Database.ExecuteNonQuery (@"UPDATE alarmslotalarm
SET machinemoduleid=alarmslot.machinemoduleid
FROM alarmslot
WHERE alarmslotalarm.alarmslotid=alarmslot.alarmslotid");
      Database.ExecuteNonQuery (@"ALTER TABLE alarmslotalarm ALTER COLUMN machinemoduleid SET NOT NULL");
      Database.GenerateForeignKey ("alarmslotalarm", ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void DeleteNewTable()
    {
      Database.RemoveTable(TableName.CNC_ALARM);
    }
  }
}
