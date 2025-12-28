// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add MachineCost column to machinemode table
  /// </summary>
  [Migration(1911)]
  public class AddMachineModeMachineCost: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModeMachineCost).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODE,
                          new Column ($"{TableName.MACHINE_MODE}machinecost", System.Data.DbType.Double));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODE, $"{TableName.MACHINE_MODE}machinecost");
    }
  }
}