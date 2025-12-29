// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add MachineCost column to machine table
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
      Database.AddColumn (TableName.MACHINE,
                          new Column ($"{TableName.MACHINE}costoff", System.Data.DbType.Double));
      Database.AddColumn (TableName.MACHINE,
                          new Column ($"{TableName.MACHINE}costinactive", System.Data.DbType.Double));
      Database.AddColumn (TableName.MACHINE,
                          new Column ($"{TableName.MACHINE}costactive", System.Data.DbType.Double));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE, $"{TableName.MACHINE}costactive");
      Database.RemoveColumn (TableName.MACHINE, $"{TableName.MACHINE}costinactive");
      Database.RemoveColumn (TableName.MACHINE, $"{TableName.MACHINE}costoff");
    }
  }
}