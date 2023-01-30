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
  /// Migration 117: Add a column to the table sequence to flag is a sequence should only be considered
  /// when some auto activities is detected on the machine
  /// </summary>
  [Migration(117)]
  public class AddSequenceAutoOnly: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceAutoOnly).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SEQUENCE,
                          new Column (TableName.SEQUENCE + "autoonly", DbType.Boolean, ColumnProperty.NotNull, "TRUE"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SEQUENCE,
                             TableName.SEQUENCE + "autoonly");
    }
  }
}
