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
  /// Migration 1207: Add a reference to a stamping config from monitoredmachine
  /// </summary>
  [Migration (1207)]
  public class MonitoredMachineStampingConfig : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MonitoredMachineStampingConfig).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE, ColumnName.STAMPING_CONFIG_BY_NAME_ID, DbType.Int32, ColumnProperty.Null);
      Database.GenerateForeignKey (TableName.MONITORED_MACHINE, ColumnName.STAMPING_CONFIG_BY_NAME_ID,
        TableName.STAMPING_CONFIG_BY_NAME, ColumnName.STAMPING_CONFIG_BY_NAME_ID,
        Migrator.Framework.ForeignKeyConstraint.SetNull);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MONITORED_MACHINE, ColumnName.STAMPING_CONFIG_BY_NAME_ID);
    }
  }
}
