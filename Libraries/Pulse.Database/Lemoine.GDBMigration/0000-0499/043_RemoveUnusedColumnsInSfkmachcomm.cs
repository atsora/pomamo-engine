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
  /// Migration 043 (deprecated): remove the last unused columns of sfkmachcomm
  /// </summary>
  [Migration(43)]
  public class RemoveUnusedColumnsInSfkmachcomm: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveUnusedColumnsInSfkmachcomm).FullName);
    
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
