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
  /// Migration 053 (deprecated): Fix the view FactTransition
  /// 
  /// The view references the MachineModeId 14 that does not exist
  /// </summary>
  [Migration(53)]
  public class FixFactTransition: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixFactTransition).FullName);
    
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
