// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1909: Machine state template color (optional)
  /// </summary>
  [Migration (1909)]
  public class MachineStateTemplateColor : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateColor).FullName);

    static readonly string MACHINE_STATE_TEMPLATE_COLOR_COLUMN = TableName.MACHINE_STATE_TEMPLATE + "color";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
        new Column (MACHINE_STATE_TEMPLATE_COLOR_COLUMN, DbType.String));
      AddConstraintColor (TableName.MACHINE_STATE_TEMPLATE, MACHINE_STATE_TEMPLATE_COLOR_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE, MACHINE_STATE_TEMPLATE_COLOR_COLUMN);
    }
  }
}
