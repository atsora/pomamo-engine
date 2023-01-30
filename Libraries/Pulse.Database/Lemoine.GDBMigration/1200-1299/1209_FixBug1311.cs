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
  /// Migration 1209: fix bug 1311
  /// </summary>
  [Migration (1209)]
  public class FixBug1311 : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixBug1311).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      var fixRequired = (bool)Database.ExecuteScalar ("select exists (select 1 from cncalarmseveritypattern c where 0 <> position ('Properties' in cncalarmseveritypatternpattern::text))");
      if (fixRequired) {
        foreach (var s in new string[] { "CncSubInfo", "Type", "Number", "Message", "Properties" }) {
          Database.ExecuteNonQuery ($"update cncalarmseveritypattern set cncalarmseveritypatternpattern = replace (cncalarmseveritypatternpattern::text, '\"{s}\":', '\"{s.ToLowerInvariant ()}\":')::json");
        }
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
