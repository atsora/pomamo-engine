// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add Dynamic column to machinestatetemplateassociation table
  /// </summary>
  [Migration(1915)]
  public class AddMachineStateTemplateAssociationDynamic : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStateTemplateAssociationDynamic).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                          new Column ($"{TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION}dynamic", System.Data.DbType.String));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, $"{TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION}dynamic");
    }
  }
}