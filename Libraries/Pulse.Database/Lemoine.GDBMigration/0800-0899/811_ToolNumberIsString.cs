// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 811: tool number is considered as a string, not a number anymore
  /// </summary>
  [Migration (811)]
  public class ToolNumberIsString : MigrationExt
  {
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.TOOL_POSITION +
        " ALTER COLUMN toolpositiontoolnumber TYPE TEXT ");
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.EVENT_TOOL_LIFE +
        " ALTER COLUMN eventtoollifetoolnumber TYPE TEXT ");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.TOOL_POSITION +
        " ALTER COLUMN toolpositiontoolnumber TYPE INT USING toolpositiontoolnumber::integer");
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.EVENT_TOOL_LIFE +
        " ALTER COLUMN eventtoollifetoolnumber TYPE INT USING eventtoollifetoolnumber::integer");
    }
  }
}
