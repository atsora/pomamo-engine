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
  /// Migration 045:
  /// <item>Add a reasondetails column in machinestatus table</item>
  /// </summary>
  [Migration(45)]
  public class AddReasonDetailsInMachineStatus: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReasonDetailsInMachineStatus).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS,
                          new Column (ColumnName.REASON_DETAILS, DbType.String));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS,
                             ColumnName.REASON_DETAILS);
    }
  }
}
