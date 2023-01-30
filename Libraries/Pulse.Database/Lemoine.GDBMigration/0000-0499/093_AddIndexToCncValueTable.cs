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
  /// Migration 093: Add an index on machine module and begin date/time
  /// </summary>
  [Migration(93)]
  public class AddIndexToCncValueTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexToCncValueTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                TableName.CNC_VALUE + "begindatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.CNC_VALUE,
                   ColumnName.MACHINE_MODULE_ID,
                   TableName.CNC_VALUE + "begindatetime");
    }
  }
}
