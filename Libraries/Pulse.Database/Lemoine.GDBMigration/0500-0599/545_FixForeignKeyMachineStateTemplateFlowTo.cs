// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Migrator.Framework;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 545: fix a foreign key in machinestatetemplateflow
  /// </summary>
  [Migration (545)]
  public class FixForeignKeyMachineStateTemplateFlowTo : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixForeignKeyMachineStateTemplateFlowTo).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveForeignKey (TableName.MACHINE_STATE_TEMPLATE_FLOW, "fk_machinestatetemplateflow_machinestatetemplate");

      Database.AddForeignKey ("fk_machinestatetemplateflowfrom",
        TableName.MACHINE_STATE_TEMPLATE_FLOW, "from" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
        TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddForeignKey ("fk_machinestatetemplateflowto",
        TableName.MACHINE_STATE_TEMPLATE_FLOW, "to" + ColumnName.MACHINE_STATE_TEMPLATE_ID,
        TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}