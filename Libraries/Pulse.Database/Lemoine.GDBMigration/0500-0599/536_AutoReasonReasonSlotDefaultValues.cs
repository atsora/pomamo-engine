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
  /// Migration 536: add the default values for the new columns for auto-reason in reason slot
  /// 
  /// without default values
  /// </summary>
  [Migration (536)]
  public class AutoReasonReasonSlotDefaultValues : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonReasonSlotDefaultValues).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.REASON_SLOT, "reasonslotdefaultreason")) {
        Database.ExecuteNonQuery (@"UPDATE reasonslot
SET reasonslotreasonscore=100,
    reasonslotautoreasonnumber=0,
    reasonslotreasonsource=CASE WHEN reasonslotdefaultreason=TRUE THEN 1 ELSE 4 END
;");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
