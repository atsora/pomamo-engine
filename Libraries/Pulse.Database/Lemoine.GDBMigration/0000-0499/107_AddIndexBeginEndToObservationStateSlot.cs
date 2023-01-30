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
  /// Migration 107: Add some new indexes to the observationstateslot table:
  /// <item>(machineid, beginday, endday)</item>
  /// <item>(machineid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(107)]
  public class AddIndexBeginEndToObservationStateSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToObservationStateSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "beginday",
                TableName.OBSERVATION_STATE_SLOT + "endday");
      
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "begindatetime",
                TableName.OBSERVATION_STATE_SLOT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "beginday",
                   TableName.OBSERVATION_STATE_SLOT + "endday");
      
      RemoveIndex (TableName.OBSERVATION_STATE_SLOT,
                   ColumnName.MACHINE_ID,
                   TableName.OBSERVATION_STATE_SLOT + "begindatetime",
                   TableName.OBSERVATION_STATE_SLOT + "enddatetime");
    }
  }
}
