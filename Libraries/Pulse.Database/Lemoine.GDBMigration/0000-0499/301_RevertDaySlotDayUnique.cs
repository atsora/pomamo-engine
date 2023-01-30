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
  /// Migration 301:
  /// </summary>
  [Migration(301)]
  public class RevertDaySlotDayUnique: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RevertDaySlotDayUnique).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.DAY_SLOT,
                   ColumnName.DAY);
      AddUniqueConstraint (TableName.DAY_SLOT,
                           ColumnName.DAY);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.DAY_SLOT,
                              ColumnName.DAY);
      AddIndex (TableName.DAY_SLOT,
                ColumnName.DAY);
    }
  }
}
