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
  /// Migration 806: consider the tool number may be a string
  /// </summary>
  [Migration (806)]
  public class ToolNumberString : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ToolNumberString).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ($@"
UPDATE field
SET fieldtype='String'
WHERE fieldid=119
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
