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
  /// Migration 812: deprecated
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
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
