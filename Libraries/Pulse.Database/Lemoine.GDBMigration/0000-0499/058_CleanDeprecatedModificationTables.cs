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
  /// Migration 058: Clean the following deprecated modification tables:
  /// <item>sequencedetection</item>
  /// <item>stampdetection</item>
  /// <item>isofileenddetection</item>
  /// </summary>
  [Migration(58)]
  public class CleanDeprecatedModificationTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CleanDeprecatedModificationTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.SEQUENCE_DETECTION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='SequenceDetection'");
        Database.RemoveTable (TableName.SEQUENCE_DETECTION);
      }
      
      if (Database.TableExists (TableName.STAMP_DETECTION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='StampDetection'");
        Database.RemoveTable (TableName.STAMP_DETECTION);
      }

      if (Database.TableExists (TableName.ISO_FILE_END_DETECTION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='IsoFileEndDetection'");
        Database.RemoveTable (TableName.ISO_FILE_END_DETECTION);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
