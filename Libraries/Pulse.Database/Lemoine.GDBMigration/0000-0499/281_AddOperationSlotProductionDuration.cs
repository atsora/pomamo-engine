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
  /// Migration 281:
  /// </summary>
  [Migration(281)]
  public class AddOperationSlotProductionDuration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationSlotProductionDuration).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "productionduration", DbType.Int32));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "productionduration");
    }
  }
}
