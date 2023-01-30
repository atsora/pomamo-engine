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
  /// Migration 801: remove the obsolete key TrackNonStandardCycles
  /// </summary>
  [Migration (801)]
  public class RemoveTrackNonStandardCycles : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveTrackNonStandardCycles).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DELETE FROM config WHERE configkey='Analysis.TrackNonStandardCycles';
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
