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
  /// Migration 812: remove the oids
  /// </summary>
  [Migration (812)]
  public class RemoveOids : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveOids).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Up ("sfkcfgsold", "sfkcmra", "sfkcompactortasks", "sfkaddr", "sfkmachtype", "sfkmach", "sfkgrpmail", "sfkgcom", "sfkmachinesfkprocesstype", "sfkgpms", "sfkperm", "sfkoptype", "sfkrrul", "sfksapp", "sfkshif", "sfkship", "sfkxreasons", "sfkupms", "sfkanly", "sfkanlytoday", "sfkanlz", "sfkgrps", "sfkmacp", "sfkcamsystem", "sfkmcls", "sfkconf", "sfkdicos", "sfkelshift", "sfkmgrp", "sfkopstrategy", "sfkfacts", "sfkgang", "sfksuperg", "sfksgrp", "sfkstartend", "sfkstok", "sfkuatl");
    }

    void Up (params string[] tableNames)
    {
      foreach (var tableName in tableNames) {
        Up (tableName);
      }
    }

    void Up (string tableName)
    {
      Database.ExecuteNonQuery ($"ALTER TABLE IF EXISTS {tableName} SET WITHOUT OIDS;");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
