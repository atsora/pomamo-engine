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
  /// Migration 105: Add some new indexes to the fact table:
  /// <item>(machineid, begindatetime, enddatetime)</item>
  /// </summary>
  [Migration(105)]
  public class AddIndexBeginEndToFact: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexBeginEndToFact).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "begindatetime",
                TableName.FACT + "enddatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "begindatetime",
                   TableName.FACT + "enddatetime");
    }
  }
}
