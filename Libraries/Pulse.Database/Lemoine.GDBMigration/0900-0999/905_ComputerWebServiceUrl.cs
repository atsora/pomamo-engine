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
  /// Migration 905: add a column webserviceurl in the table computer 
  /// </summary>
  [Migration (905)]
  public class ComputerWebServiceUrl : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerWebServiceUrl).FullName);

    static readonly string COLUMN_NAME = TableName.COMPUTER + "webserviceurl";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.COMPUTER, COLUMN_NAME, DbType.String);
      MakeColumnText (TableName.COMPUTER, COLUMN_NAME);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.COMPUTER, COLUMN_NAME);
    }
  }
}
