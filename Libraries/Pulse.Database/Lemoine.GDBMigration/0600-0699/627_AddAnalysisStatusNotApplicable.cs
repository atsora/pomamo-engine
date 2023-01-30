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
  /// Migration 627: 
  /// </summary>
  [Migration (627)]
  public class AddAnalysisStatusNotApplicable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddAnalysisStatusNotApplicable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "14", "NotApplicable", "Completed" });
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "15", "AncestorNotApplicable", "Completed" });
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] { TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name",
                       TableName.ANALYSIS_STATUS + "category" },
                       new string[] { "16", "AncestorError", "Error" });
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysisstatus WHERE analysisstatusid>=14;");
    }
  }
}
