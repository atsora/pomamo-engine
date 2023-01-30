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
  /// Migration 312:
  /// </summary>
  [Migration(312)]
  public class RenameMachineModificationStatusMachineId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RenameMachineModificationStatusMachineId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameColumn (TableName.MACHINE_MODIFICATION_STATUS,
                             ColumnName.MACHINE_ID,
                             TableName.MACHINE_MODIFICATION_STATUS + ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RenameColumn (TableName.MACHINE_MODIFICATION_STATUS,
                             TableName.MACHINE_MODIFICATION_STATUS + ColumnName.MACHINE_ID,
                             ColumnName.MACHINE_ID);
    }
  }
}
