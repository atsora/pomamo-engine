// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1004: 
  /// </summary>
  [Migration (1004)]
  public class AddReasonDisplayPriority : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonDisplayPriority).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddDisplayPriority (TableName.REASON);
      AddDisplayPriority (TableName.REASON_GROUP);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDisplayPriority (TableName.REASON_GROUP);
      RemoveDisplayPriority (TableName.REASON);
    }

    void AddDisplayPriority (string tableName)
    {
      Database.AddColumn (tableName,
                          new Column (tableName + "displaypriority", DbType.Int32));
      AddIndex (tableName,
                tableName + "displaypriority");
    }

    void RemoveDisplayPriority (string tableName)
    {
      Database.RemoveColumn (tableName, tableName + "displaypriority");
    }
  }
}
