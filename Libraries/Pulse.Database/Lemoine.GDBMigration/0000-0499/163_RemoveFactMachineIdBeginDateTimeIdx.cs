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
  /// Migration 163: Remove the index fact_machineid_factbegindatetime_idx
  /// because it is a duplicate of fact_machineid_factbegindatetime_unique
  /// </summary>
  [Migration(163)]
  public class RemoveFactMachineIdeBeginDateTimeIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveFactMachineIdeBeginDateTimeIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.FACT,
                   ColumnName.MACHINE_ID,
                   TableName.FACT + "begindatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                TableName.FACT + "begindatetime");
    }
  }
}
