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
  /// Migration 1102: add a new column to the machine module table 
  /// </summary>
  [Migration (1102)]
  public class MachineModuleMilestoneVariable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineModuleMilestoneVariable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "milestonevariable", DbType.String, ColumnProperty.Null));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "milestonevariable");
    }
  }
}
