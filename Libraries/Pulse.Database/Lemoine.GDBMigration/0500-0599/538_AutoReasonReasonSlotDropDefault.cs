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
  /// Migration 529: remove the deprecated reasonslotdefault
  /// </summary>
  [Migration (538)]
  public class AutoReasonReasonSlotDropDefault : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonReasonSlotDropDefault).FullName);

    static readonly string REASON_SLOT_DEFAULT_REASON = "reasonslotdefaultreason";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON)) {
        RemoveReasonSlotDefaultReason ();
      }
    }

    void RemoveReasonSlotDefaultReason ()
    {
      RemoveColumnCascade (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON);
      AddVirtualColumn (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON, "boolean", "SELECT $1.reasonslotreasonsource <= 1"); // (see migration 547 as well)
    }


    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RestoreReasonSlotDefaultReason ();
    }

    void RestoreReasonSlotDefaultReason ()
    {
      DropVirtualColumn (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON);
      Database.AddColumn (TableName.REASON_SLOT, new Column (REASON_SLOT_DEFAULT_REASON, DbType.Boolean));
      Database.ExecuteNonQuery (@"
UPDATE reasonslot
SET reasonslotdefaultreason=(reasonslotreasonsource IN (1,3))
");
      SetNotNull (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON);
    }
  }
}
