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
  /// Migration 298:
  /// </summary>
  [Migration(298)]
  public class DaySlotDayNotUnique: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DaySlotDayNotUnique).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveUniqueConstraint (TableName.DAY_SLOT,
                              ColumnName.DAY);
      AddIndex (TableName.DAY_SLOT,
                ColumnName.DAY);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.DAY_SLOT,
                   ColumnName.DAY);
      AddUniqueConstraint (TableName.DAY_SLOT,
                           ColumnName.DAY);
    }
  }
}
