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
  /// Migration 1109: Convert the column cncvariablevalue of cncvariable to the text type 
  /// </summary>
  [Migration (1109)]
  public class CncVariableValueColumnsToText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncVariableValueColumnsToText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.CNC_VARIABLE, $"{TableName.CNC_VARIABLE}value");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
