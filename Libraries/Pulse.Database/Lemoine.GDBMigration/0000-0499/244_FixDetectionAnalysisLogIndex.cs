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
  /// Migration 244: fix the index on detectionanalysislog that was badly set previously
  /// </summary>
  [Migration(244)]
  public class FixDetectionAnalysisLogIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixDetectionAnalysisLogIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DROP INDEX IF EXISTS detectionanalysislog_datetime_idx;");
      Database.ExecuteNonQuery (@"
CREATE INDEX detectionanalysislog_datetime_idx
ON detectionanalysislog (datetime);");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
