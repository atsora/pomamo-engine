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
  /// Migration 1003: 
  /// </summary>
  [Migration (1003)]
  public class RemoveDeprecatedSchemasReportingRtd : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveDeprecatedSchemasReportingRtd));

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveSchema ("reporting");
      RemoveSchema ("rtd");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

  }
}
