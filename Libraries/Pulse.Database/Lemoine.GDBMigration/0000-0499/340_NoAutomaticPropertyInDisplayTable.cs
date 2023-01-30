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
  /// Migration 340:
  /// </summary>
  [Migration(340)]
  public class NoAutomaticPropertyInDisplayTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NoAutomaticPropertyInDisplayTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(Tool|Company|Department|MachineCategory|MachineSubCategory)%>', '<%\1.Display%>', 'g');
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(Category|SubCategory)%>', '<%Machine\1.Display%>', 'g');
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
