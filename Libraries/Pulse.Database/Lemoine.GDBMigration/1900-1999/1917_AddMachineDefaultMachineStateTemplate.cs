// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add DefaultMachineStateTemplate column to machine table
  /// </summary>
  [Migration (1917)]
  public class AddMachineDefaultMachineStateTemplate : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineDefaultMachineStateTemplate).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn ($"{TableName.MACHINE}",
                          new Column ($"{TableName.MACHINE}defaultmachinestatetemplateid", System.Data.DbType.Int32));
      
      // Add foreign key constraint for DefaultMachineStateTemplate
      Database.GenerateForeignKey ($"{TableName.MACHINE}", $"{TableName.MACHINE}defaultmachinestatetemplateid",
                                   $"{TableName.MACHINE_STATE_TEMPLATE}", $"{TableName.MACHINE_STATE_TEMPLATE}id",
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn ($"{TableName.MACHINE}", $"{TableName.MACHINE}defaultmachinestatetemplateid");
    }
  }
}