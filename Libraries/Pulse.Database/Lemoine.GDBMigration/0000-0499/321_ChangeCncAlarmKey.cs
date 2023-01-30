// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 321: remove the no overlap constraint and add an index
  /// The no overlap constraint would have involved the jsonb column: impossible
  /// </summary>
  [Migration(321)]
  public class ChangeCncAlarmKey: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ChangeCncAlarmKey).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveNoOverlapConstraint(TableName.CNC_ALARM);
      AddIndex(TableName.CNC_ALARM,
               ColumnName.MACHINE_MODULE_ID,
               TableName.CNC_ALARM + "type",
               TableName.CNC_ALARM + "number");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex(TableName.CNC_ALARM,
                  ColumnName.MACHINE_MODULE_ID,
                  TableName.CNC_ALARM + "type",
                  TableName.CNC_ALARM + "number");
      try {
        AddNoOverlapConstraint(TableName.CNC_ALARM,
                               TableName.CNC_ALARM + "datetimerange",
                               ColumnName.MACHINE_MODULE_ID,
                               TableName.CNC_ALARM + "cncinfo",
                               TableName.CNC_ALARM + "type",
                               TableName.CNC_ALARM + "number");
      } catch (Exception e) {
        log.ErrorFormat("Couldn't add the no overlap constraint: {0}", e);
      }
    }
  }
}
