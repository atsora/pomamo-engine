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
  /// Migration 171: Add a mobile number column to the user table
  /// </summary>
  [Migration(171)]
  public class AddMobileNumber: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMobileNumber).FullName);
    
    static readonly string MOBILE_NUMBER_COLUMN = "usermobilenumber";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.USER,
                          new Column (MOBILE_NUMBER_COLUMN, DbType.String, 30, ColumnProperty.Null));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.USER,
                             MOBILE_NUMBER_COLUMN);
    }
  }
}
