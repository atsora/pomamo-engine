// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 316:
  /// </summary>
  [Migration(316)]
  public class AddDonePurgeModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDonePurgeModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name", TableName.ANALYSIS_STATUS + "category"},
                       new string[] {"13", "DonePurge", "Completed"});
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM analysisstatus WHERE analysisstatusid=13;");
    }
  }
}
