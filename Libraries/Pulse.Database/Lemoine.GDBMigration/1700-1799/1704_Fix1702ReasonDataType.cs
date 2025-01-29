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
  /// Migration 1704: Fix the type of reasondata in table reasonproposal
  /// </summary>
  [Migration (1704)]
  public class Fix1702ReasonDataType : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Fix1702ReasonDataType).FullName);

    readonly static string REASON_DATA_COLUMN = "reasondata";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnJson (TableName.REASON_PROPOSAL, REASON_DATA_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
