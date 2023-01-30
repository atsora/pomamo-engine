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
  /// Migration 624: remove some deprecated sfk... tables 
  /// </summary>
  [Migration (624)]
  public class RemoveSfkTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveSfkTables).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists ("sfkscheduling")) {
        Database.RemoveTable ("sfkscheduling");
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
