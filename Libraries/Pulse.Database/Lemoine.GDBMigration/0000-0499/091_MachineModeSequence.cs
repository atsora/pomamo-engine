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
  /// Migration 091: make the machine mode sequence start at 100 for custom machine modes
  /// </summary>
  [Migration(91)]
  public class MachineModeSequence: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeSequence).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      SetSequence (TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID, 100);
      SetMinSequence (TableName.REASON_GROUP, ColumnName.REASON_GROUP_ID, 100);
      SetMinSequence (TableName.REASON, ColumnName.REASON_ID, 100);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
