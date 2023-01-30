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
  /// Migration 050: Fix the type of the reasonsummarytime column
  /// in table reasonsummary
  /// </summary>
  [Migration(50)]
  public class FixReasonSummaryTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixReasonSummaryTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE reasonsummary
ALTER COLUMN reasonsummarytime
TYPE double precision
USING reasonsummarytime::double precision;");
      Database.ExecuteNonQuery (@"ALTER TABLE reasonsummary
ALTER COLUMN reasonsummarytime SET NOT NULL;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
