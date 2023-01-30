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
  /// Migration 319: in cncalarm
  /// * add index and nooverlap constraint
  /// * column "number" is now a string
  /// * add "NOT NULL"
  /// </summary>
  [Migration(319)]
  public class UpdateCncAlarmTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateCncAlarmTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Delete the table
      Database.RemoveTable(TableName.CNC_ALARM);
      
      // And add it again
      Database.AddTable(TableName.CNC_ALARM,
                        new Column(ColumnName.CNC_ALARM_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.CNC_ALARM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM + "cncinfo", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM + "type", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM + "number", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.CNC_ALARM + "message", DbType.String),
                        new Column(TableName.CNC_ALARM + "properties", DbType.Int32),
                        new Column(TableName.CNC_ALARM + "datetimerange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange(TableName.CNC_ALARM, TableName.CNC_ALARM + "datetimerange");
      MakeColumnJson(TableName.CNC_ALARM, TableName.CNC_ALARM + "properties");
      
      // Create an overlap constraint
      AddNoOverlapConstraint(TableName.CNC_ALARM,
                             TableName.CNC_ALARM + "datetimerange",
                             ColumnName.MACHINE_MODULE_ID,
                             TableName.CNC_ALARM + "cncinfo",
                             TableName.CNC_ALARM + "type",
                             TableName.CNC_ALARM + "number");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove the overlap constraint
      try {
        RemoveNoOverlapConstraint(TableName.CNC_ALARM);
      } catch (Exception e) {
        log.ErrorFormat("Couldn't remove the no overlap constraint: {0}", e);
      }
    }
  }
}
