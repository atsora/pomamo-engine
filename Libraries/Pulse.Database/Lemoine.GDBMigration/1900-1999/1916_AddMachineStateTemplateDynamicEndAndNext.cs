// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add DynamicEnd and NextMachineStateTemplateId columns to machinestatetemplate table
  /// </summary>
  [Migration (1916)]
  public class AddMachineStateTemplateDynamicEndAndNext : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStateTemplateDynamicEndAndNext).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn ($"{TableName.MACHINE_STATE_TEMPLATE}",
                          new Column ($"{TableName.MACHINE_STATE_TEMPLATE}dynamicend", System.Data.DbType.String));
      
      Database.AddColumn ($"{TableName.MACHINE_STATE_TEMPLATE}",
                          new Column ($"{TableName.MACHINE_STATE_TEMPLATE}nextid", System.Data.DbType.Int32));
      
      // Add foreign key constraint for NextMachineStateTemplateId
      Database.GenerateForeignKey ($"{TableName.MACHINE_STATE_TEMPLATE}", $"{TableName.MACHINE_STATE_TEMPLATE}nextid",
                                   $"{TableName.MACHINE_STATE_TEMPLATE}", $"{TableName.MACHINE_STATE_TEMPLATE}id",
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn ($"{TableName.MACHINE_STATE_TEMPLATE}", $"{TableName.MACHINE_STATE_TEMPLATE}nextid");
      Database.RemoveColumn ($"{TableName.MACHINE_STATE_TEMPLATE}", $"{TableName.MACHINE_STATE_TEMPLATE}dynamicend");
    }
  }
}