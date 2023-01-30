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
  /// Migration 1107: add a disconnection time for a user 
  /// </summary>
  [Migration (1107)]
  public class UserDisconnectionTime : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UserDisconnectionTime).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.USER, new Column ("userdisconnection", DbType.Int32));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.USER, "userdisconnection");
    }
  }
}
