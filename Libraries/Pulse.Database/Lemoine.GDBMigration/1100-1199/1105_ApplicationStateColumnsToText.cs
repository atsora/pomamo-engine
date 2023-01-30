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
  /// Migration 1005: Convert the columns of applicationstate to the text type 
  /// </summary>
  [Migration (1105)]
  public class ApplicationStateColumnsToText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ApplicationStateColumnsToText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.APPLICATION_STATE, $"{TableName.APPLICATION_STATE}key");
      MakeColumnText (TableName.APPLICATION_STATE, $"{TableName.APPLICATION_STATE}value");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
