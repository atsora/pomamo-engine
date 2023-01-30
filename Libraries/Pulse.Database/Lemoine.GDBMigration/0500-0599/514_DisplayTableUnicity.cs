// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 514: Fix the unicity constraint in display table
  /// </summary>
  [Migration (514)]
  public class DisplayTableUnicity: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DisplayTableUnicity).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Remove this index, because it duplicates the unique constraint reasonselection_machinemodeid_machineobservationstateid__unique
      RemoveIndex (TableName.REASON_SELECTION, ColumnName.MACHINE_MODE_ID, ColumnName.MACHINE_OBSERVATION_STATE_ID);

      // Remove this index, because it duplicates the unique index machinemodedefaultreason_machinemodeid_machineobservationst_idx
      RemoveIndex (TableName.MACHINE_MODE_DEFAULT_REASON, ColumnName.MACHINE_MODE_ID, ColumnName.MACHINE_OBSERVATION_STATE_ID);

      RemoveUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table", TableName.DISPLAY + "variant");
      RemoveIndex (TableName.DISPLAY, TableName.DISPLAY + "table");
      AddNamedUniqueIndex ("display_unique",
        TableName.DISPLAY,
        TableName.DISPLAY + "table",
        "COALESCE(displayvariant,'')");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("display_unique", TableName.DISPLAY);
      AddUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table", TableName.DISPLAY + "variant");
      AddIndex (TableName.DISPLAY, TableName.DISPLAY + "table");

      AddIndex (TableName.MACHINE_MODE_DEFAULT_REASON, ColumnName.MACHINE_MODE_ID, ColumnName.MACHINE_OBSERVATION_STATE_ID);

      AddIndex (TableName.REASON_SELECTION, ColumnName.MACHINE_MODE_ID, ColumnName.MACHINE_OBSERVATION_STATE_ID);
    }
  }
}
