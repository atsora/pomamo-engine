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
  /// Migration 170:
  /// </summary>
  [Migration(170)]
  public class AddAlarmSlotTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAlarmSlotTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add table alarmslot
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
      
      // add table alarmslotalarm for linking an alarm slot to a set of alarms
      Database.AddTable ("alarmslotalarm",
                         new Column("alarmslotalarm" + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column("alarmslotalarm" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("alarmslot" + "id", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("alarm" + "id", DbType.Int32, ColumnProperty.NotNull));
      
      Database.GenerateForeignKey ("alarmslotalarm",
                                   "alarmslot" + "id",
                                   "alarmslot",
                                   "alarmslot" + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey ("alarmslotalarm",
                                   "alarm" + "id",
                                   "alarm",
                                   "alarm" + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      AddNamedUniqueConstraint ("alarmslotalarm_SecondaryKey",
                             "alarmslotalarm",
                             new string [] { "alarmslot" + "id",
                               "alarm" + "id"});

      AddIndex ("alarmslotalarm",
                "alarmslot" + "id");
      
      AddIndex ("alarmslotalarm",
                "alarm" +"id");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable ("alarmslotalarm");
      Database.RemoveTable ("alarmslot");
    }
  }
}
