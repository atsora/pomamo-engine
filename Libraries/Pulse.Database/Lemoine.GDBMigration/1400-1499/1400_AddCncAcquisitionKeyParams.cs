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
  /// Migration 1400: Add a cncacquisitionkeyparam column
  /// </summary>
  [Migration (1400)]
  public class AddCncAcquisitionKeyParams : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncAcquisitionKeyParams).FullName);

    readonly static string CONFIG_KEY_PARAMS_COLUMN = $"{TableName.CNC_ACQUISITION}configkeyparams";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CNC_ACQUISITION,
        new Column (CONFIG_KEY_PARAMS_COLUMN, DbType.String));
      MakeColumnJson (TableName.CNC_ACQUISITION, CONFIG_KEY_PARAMS_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CNC_ACQUISITION, CONFIG_KEY_PARAMS_COLUMN);
    }
  }
}
