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
  /// Migration 313:
  /// </summary>
  [Migration(313)]
  public class RenameMachineModificationMachineId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RenameMachineModificationMachineId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameColumn (TableName.MACHINE_MODIFICATION,
                             ColumnName.MACHINE_ID,
                             TableName.MACHINE_MODIFICATION + ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RenameColumn (TableName.MACHINE_MODIFICATION,
                             TableName.MACHINE_MODIFICATION + ColumnName.MACHINE_ID,
                             ColumnName.MACHINE_ID);
    }
  }
}
