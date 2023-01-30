// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 529: add the columns for auto-reason
  ///
  /// Replaced by migration 535 to 540
  /// </summary>
  [Migration (529)]
  public class AddAutoReason : MigrationExt
  {
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
