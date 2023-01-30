// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 076 (deprecated): Migrate the data from sfkfacts to cncvalue
  /// </summary>
  [Migration(76)]
  public class MigrateCncValue: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateCncValue).FullName);
    
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
