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
  /// Migration 547: 
  /// </summary>
  [Migration (547)]
  public class FixReasonSlotDefaultReasonVirtualColumn : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixReasonSlotDefaultReasonVirtualColumn).FullName);

    static readonly string REASON_SLOT_DEFAULT_REASON = "reasonslotdefaultreason";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddVirtualColumn (TableName.REASON_SLOT, REASON_SLOT_DEFAULT_REASON, "boolean", "SELECT $1.reasonslotreasonsource <= 1");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}