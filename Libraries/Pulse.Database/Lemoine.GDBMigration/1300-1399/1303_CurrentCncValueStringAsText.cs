// Copyright (C) 2023 Atsora Solutions
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
  /// Migration 1303: Convert the column currentcncvaluestring of currentcncvalue to the text type 
  /// </summary>
  [Migration (1303)]
  public class CurrentCncValueStringAsText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentCncValueStringAsText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.CURRENT_CNC_VALUE, $"{TableName.CURRENT_CNC_VALUE}string");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
