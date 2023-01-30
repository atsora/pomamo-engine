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
  /// Migration 636: AddTimeoutCancelled 
  /// </summary>
  [Migration (636)]
  public class AddTimeoutCanceled : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddTimeoutCanceled).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
UPDATE analysisstatus
SET analysisstatuscategory='InProgress'
WHERE analysisstatusid=7;
");
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "17", "TimeoutCancelled", "Error" });
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "18", "ParentInError", "Error" });
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "19", "ChildInError", "Error" });
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysisstatus WHERE analysisstatusid>=17;");
      Database.ExecuteNonQuery (@"
UPDATE analysisstatus
SET analysisstatuscategory='Error'
WHERE analysisstatusid=7;
");
    }

  }
}
