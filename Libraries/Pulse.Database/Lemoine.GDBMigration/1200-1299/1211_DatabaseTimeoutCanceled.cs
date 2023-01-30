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
  /// Migration 1211: DatabaseTimeoutCanceled 
  /// </summary>
  [Migration (1211)]
  public class DatabaseTimeoutCanceled : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DatabaseTimeoutCanceled).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
UPDATE analysisstatus
SET analysisstatusname='TimeoutCanceled'
WHERE analysisstatusid=17;
");
      Database.ExecuteNonQuery (@"
UPDATE analysisstatus
SET analysisstatuscategory='InProgress'
WHERE analysisstatusid=20;
");
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "21", "DatabaseTimeoutCanceled", "Error" });
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysisstatus WHERE analysisstatusid>=21;");
      Database.ExecuteNonQuery (@"
UPDATE analysisstatus
SET analysisstatuscategory='Error'
WHERE analysisstatusid=20;
");
    }

  }
}
