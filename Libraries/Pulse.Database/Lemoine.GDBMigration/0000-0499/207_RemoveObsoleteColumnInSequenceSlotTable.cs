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
  /// Migration 207: Remove the two obsolete columns from sequenceslot table
  /// </summary>
  [Migration(207)]
  public class RemoveObsoleteColumnInSequenceSlotTable: MigrationExt
  {
    static readonly string ANALYSIS_STATUS_ID_COLUMN = "sequenceslotanalysisstatusid";
    static readonly string ANALYSIS_DATETIME_COLUMN = "sequenceslotanalysisdatetime";
    
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveObsoleteColumnInSequenceSlotTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.SEQUENCE_SLOT,
                                 ANALYSIS_STATUS_ID_COLUMN)) {
        Database.RemoveColumn (TableName.SEQUENCE_SLOT,
                               ANALYSIS_STATUS_ID_COLUMN);
      }
      if (Database.ColumnExists (TableName.SEQUENCE_SLOT, ANALYSIS_DATETIME_COLUMN)) {
        Database.RemoveColumn (TableName.SEQUENCE_SLOT,
                               ANALYSIS_DATETIME_COLUMN);
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
