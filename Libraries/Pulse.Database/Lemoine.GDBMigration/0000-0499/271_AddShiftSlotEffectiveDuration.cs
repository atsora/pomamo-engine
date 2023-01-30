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
  /// Migration 271: add an effective duration column in shift slot
  /// </summary>
  [Migration(271)]
  public class AddShiftSlotEffectiveDuration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftSlotEffectiveDuration).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (TableName.SHIFT_SLOT + "effectiveduration", DbType.Int32));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} 
SET {0}effectiveduration=EXTRACT(EPOCH FROM upper({0}range)-lower({0}range))
WHERE NOT lower_inf({0}range)
  AND NOT upper_inf({0}range)",
                                              TableName.SHIFT_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} 
SET {0}templateprocessed=FALSE
WHERE shifttemplateid IS NOT NULL
   OR {0}effectiveduration IS NULL",
                                              TableName.SHIFT_SLOT)); // To recompute the effective duration
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SHIFT_SLOT,
                            TableName.SHIFT_SLOT + "effectiveduration");
    }
  }
}
