// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1702: Add a column reasondata in reasonproposal table
  /// </summary>
  [Migration (1702)]
  public class AddReasonDataPossibleReason : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonDataReasonSlot).FullName);

    readonly static string REASON_DATA_COLUMN = "reasondata";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_PROPOSAL,
        new Column (REASON_DATA_COLUMN, DbType.String));
      MakeColumnJson (TableName.REASON_SLOT, REASON_DATA_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_PROPOSAL, REASON_DATA_COLUMN);
    }
  }
}
