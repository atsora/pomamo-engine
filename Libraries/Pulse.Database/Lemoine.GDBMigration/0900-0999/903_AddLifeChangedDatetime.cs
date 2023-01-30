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
  /// Migration 903: add the column "toolpositionlifechangeddatetime" in the table "toolposition"
  /// </summary>
  [Migration (903)]
  public class AddLifeChangedDatetime : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddLifeChangedDatetime).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.TOOL_POSITION, new Column (TableName.TOOL_POSITION + "lifechangeddatetime", DbType.DateTime));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.TOOL_POSITION, TableName.TOOL_POSITION + "lifechangeddatetime");
    }
  }
}
