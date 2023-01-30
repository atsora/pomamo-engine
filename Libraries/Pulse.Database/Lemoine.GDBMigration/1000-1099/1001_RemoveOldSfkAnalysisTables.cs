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
  /// Migration 1001: remove some old sfk tables: sfkfacts, sfkanly, sfkanlz
  /// </summary>
  [Migration (1001)]
  public class RemoveOldSfkAnalysisTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveOldSfkAnalysisTables).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      var tables = new string[] { "sfkfacts", "sfkanly", "sfkanlz", "sfkstartend", "sfkelshift", "sfkcompactortasks", "sfkanlytoday" };
      foreach (var table in tables) {
        RemoveTable (table);
      }
      // Note: there are more to come later... But these are the largest tables
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
