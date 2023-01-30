// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Migration 805: consider the pallet number may be a string
  /// </summary>
  [Migration (805)]
  public class PalletNumberString : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PalletNumberString).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ($@"
UPDATE field
SET fieldtype='String'
WHERE fieldid=124
;");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
