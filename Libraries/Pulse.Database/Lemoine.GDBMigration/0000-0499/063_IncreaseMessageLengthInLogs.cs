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
  /// Migration 063: Increase the length of the message in logs to 1023 characters
  /// </summary>
  [Migration(63)]
  public class IncreaseMessageLengthInLogs: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (IncreaseMessageLengthInLogs).FullName);
    static readonly string ALTER_LENGTH_REQUEST = @"ALTER TABLE {0}
ALTER COLUMN {1} SET DATA TYPE CHARACTER VARYING({2})";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      int newMessageSize = 1023;
      
      AlterStringLength (TableName.ANALYSIS_LOG,
                         "message",
                         newMessageSize);
      AlterStringLength (TableName.DETECTION_ANALYSIS_LOG,
                         "message",
                         newMessageSize);
      AlterStringLength (TableName.SYNCHRONIZATION_LOG,
                         "message",
                         newMessageSize);
      
      AlterStringLength (TableName.SYNCHRONIZATION_LOG,
                         "xmlelement",
                         2047);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      int oldMessageSize = 255;
      
      AlterStringLength (TableName.ANALYSIS_LOG,
                         "message",
                         oldMessageSize);
      AlterStringLength (TableName.DETECTION_ANALYSIS_LOG,
                         "message",
                         oldMessageSize);
      AlterStringLength (TableName.SYNCHRONIZATION_LOG,
                         "message",
                         oldMessageSize);
    }
    
    void AlterStringLength (string tableName, string columnName, int newLength)
    {
      Database.ExecuteNonQuery (string.Format (ALTER_LENGTH_REQUEST,
                                               tableName,
                                               columnName,
                                               newLength));
    }
  }
}
