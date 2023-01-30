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
  /// Migration 031 (deprecated): views to get rid of the limitation of ODBC to understand the Boolean type
  /// </summary>
  [Migration(31)]
  public class BooleanViews: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BooleanViews).FullName);
    
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
