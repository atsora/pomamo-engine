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
  /// Migration 901: add the column "operationarchivedatetime" to the table "operation"
  /// </summary>
  [Migration (901)]
  public class AddOperationArchiveDatetime : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddUserTableProperties).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION, new Column ("operationarchivedatetime", DbType.DateTime));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION, "operationarchivedatetime");
    }
  }
}
