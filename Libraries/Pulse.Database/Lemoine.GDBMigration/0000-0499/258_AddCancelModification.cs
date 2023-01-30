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
  /// Migration 258:
  /// </summary>
  [Migration(258)]
  public class AddCancelModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCancelModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"12", "Cancel"});
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysisstatus WHERE analysisstatusid=12;");
    }
  }
}
