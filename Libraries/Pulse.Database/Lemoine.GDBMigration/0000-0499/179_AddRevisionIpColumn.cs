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
  /// Migration 179: Add revisionip column to revision table (should be long enough to contain ipv6 addresses)
  /// </summary>
  [Migration(179)]
  public class AddRevisionIpColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddRevisionIpColumn).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn(TableName.REVISION,
                         new Column (TableName.REVISION + "ip", DbType.String));
      Database.ExecuteNonQuery ("ALTER TABLE " + TableName.REVISION + " " +
                                "ALTER COLUMN " + TableName.REVISION + "ip" + " " +
                                "SET DATA TYPE character varying(39);"); // max 39 chars in ipv6 address

    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REVISION,
                             TableName.REVISION + "ip");
    }
  }
}
