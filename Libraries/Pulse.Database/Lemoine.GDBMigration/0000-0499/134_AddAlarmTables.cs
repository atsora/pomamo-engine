// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration.Properties
{
  /// <summary>
  /// Migration 134: add alarm and alarmclass tables
  /// </summary>
  [Migration(134)]
  public class AddAlarmTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAlarmTables).FullName);
    static readonly string ALARM_CLASS_NAME = "alarmclass" + "name";
    static readonly string ALARM_NUMBER = "alarm" + "number";
    static readonly string ALARM_MESSAGE = "alarm" + "message";
    static readonly string ALARM_CNCINFO = "alarm" + "cncinfo";
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable ("alarmclass",
                         new Column ("alarmclassid", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("alarmclass" + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ALARM_CLASS_NAME, DbType.String, ColumnProperty.NotNull));
      
      // add index on id
      AddIndex("alarmclass", "alarmclassid");

      // add constraint on uniqueness of alarm class name
      // also used as index
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

      // add index on id
      AddIndex("alarm", "alarmid");
      
      // add constraint on uniqueness of alarm w.r.t. class+number+message+cncinfo
      // also used as index
      AddUniqueConstraint("alarm",
                          ALARM_NUMBER, ALARM_MESSAGE, ALARM_CNCINFO, "alarmclassid");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable ("alarmclass");
      Database.RemoveTable ("alarm");
    }
  }
}
