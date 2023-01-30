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
  /// Migration 097: Add a column in monitoredmachine to set what kind of data must be displayed in the operation bar
  ///                in db_monitor
  /// </summary>
  [Migration(97)]
  public class MonitoredMachineOperationBar: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineOperationBar).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE,
                          new Column (TableName.MONITORED_MACHINE + "operationbar", DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MONITORED_MACHINE,
                             TableName.MONITORED_MACHINE + "operationbar");
    }
  }
}
