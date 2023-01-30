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
  /// Migration 160: add the start cycle variable (new column in machinemodule table)
  /// </summary>
  [Migration(160)]
  public class AddMachineModuleStartCycleVariable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModuleStartCycleVariable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "startcyclevariable", DbType.String, ColumnProperty.Null));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "startcyclevariable");
    }
  }
}
