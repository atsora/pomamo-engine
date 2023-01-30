// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 619: 
  /// </summary>
  [Migration (619)]
  public class AddCncAlarmLowerUpperIndexes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncAlarmLowerUpperIndexes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndex ("cncalarm_lower", TableName.CNC_ALARM, ColumnName.MACHINE_MODULE_ID, "lower(cncalarmdatetimerange)");
      AddNamedIndex ("cncalarm_upperlower", TableName.CNC_ALARM, ColumnName.MACHINE_MODULE_ID, "upper(cncalarmdatetimerange)", "lower(cncalarmdatetimerange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("cncalarm_lower", TableName.CNC_ALARM);
      RemoveNamedIndex ("cncalarm_upperlower", TableName.CNC_ALARM);
    }
  }
}
