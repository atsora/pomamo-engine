// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  enum LogLevel {
    /// <summary>
    /// DEBUG log level
    /// </summary>
    DEBUG=0,
    /// <summary>
    /// INFO log level
    /// </summary>
    INFO=1,
    /// <summary>
    /// NOTICE log level
    /// </summary>
    NOTICE=2,
    /// <summary>
    /// WARN log level
    /// </summary>
    WARN=3,
    /// <summary>
    /// ERROR log level
    /// </summary>
    ERROR=4,
    /// <summary>
    /// CRIT log level
    /// </summary>
    CRIT=5
  };

  /// <summary>
  /// Migration 245: Convert the log levels from numeric to string
  /// </summary>
  [Migration(245)]
  public class ConvertLogLevel: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConvertLogLevel).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      string[] tableNames = new string[] {"analysislog", "detectionanalysislog", "synchronizationlog"};
      foreach (string tableName in tableNames) {
        ConvertToString (tableName, LogLevel.DEBUG);
        ConvertToString (tableName, LogLevel.INFO);
        ConvertToString (tableName, LogLevel.NOTICE);
        ConvertToString (tableName, LogLevel.WARN);
        ConvertToString (tableName, LogLevel.ERROR);
        ConvertToString (tableName, LogLevel.CRIT);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      string[] tableNames = new string[] {"analysislog", "detectionanalysislog", "synchronizationlog"};
      foreach (string tableName in tableNames) {
        ConvertFromString (tableName, LogLevel.DEBUG);
        ConvertFromString (tableName, LogLevel.INFO);
        ConvertFromString (tableName, LogLevel.NOTICE);
        ConvertFromString (tableName, LogLevel.WARN);
        ConvertFromString (tableName, LogLevel.ERROR);
        ConvertFromString (tableName, LogLevel.CRIT);
      }
    }

    void ConvertToString (string tableName, LogLevel logLevel)
    {
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET level='{1}'
WHERE level='{2}';",
                                               tableName,
                                               logLevel.ToString (),
                                               (int) logLevel));
    }
    
    void ConvertFromString (string tableName, LogLevel logLevel)
    {
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET level='{2}'
WHERE level='{1}';",
                                               tableName,
                                               logLevel.ToString (),
                                               (int) logLevel));
    }
    
  }
}
