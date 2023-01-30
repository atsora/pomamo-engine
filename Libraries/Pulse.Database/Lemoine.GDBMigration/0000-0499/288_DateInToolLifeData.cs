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
  /// Migration 288: add time in the tool life information
  /// also add time and machine observation state in tool life event
  /// </summary>
  [Migration(288)]
  public class DateInToolLifeData: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DateInToolLifeData).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Add a column "timestamp" to the tables toollife and toolposition
      Database.AddColumn(TableName.TOOL_LIFE,
                         new Column(TableName.TOOL_LIFE + "timestamp",
                                    DbType.DateTime, ColumnProperty.NotNull,
                                    "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      AddTimeStampTrigger(TableName.TOOL_LIFE);
      Database.AddColumn(TableName.TOOL_POSITION,
                         new Column(TableName.TOOL_POSITION + "timestamp",
                                    DbType.DateTime, ColumnProperty.NotNull,
                                    "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      AddTimeStampTrigger(TableName.TOOL_POSITION);
      
      // Add a column "elapsedtime" in eventtoollife
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "elapsedtime", DbType.Int32);
      
      // Also add the column "machineobservationstateid" in eventtoollife
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32);
      Database.AddForeignKey("fk", TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                             TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                             Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Remove timestamp columns
      RemoveTimeStampTrigger(TableName.TOOL_LIFE);
      RemoveTimeStampTrigger(TableName.TOOL_POSITION);
      
      // Remove the column "elapsedtime" in eventtoollife
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "elapsedtime");
      
      // Remove the column "machineobservationstateid" from eventtoollife
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_OBSERVATION_STATE_ID);
    }
  }
}
