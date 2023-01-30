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
  /// Migration 606:
  /// 
  /// PostgreSQL >= 9.5 is required
  /// </summary>
  [Migration(606)]
  public class ReasonSlotToRangeStep1: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSlotToRangeStep1).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CheckPostgreSQL95 ();
      
      RemoveOldIndexes ();

      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "datetimerange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.REASON_SLOT, TableName.REASON_SLOT + "datetimerange");
      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "dayrange", DbType.Int32, ColumnProperty.Null));
      MakeColumnDateRange (TableName.REASON_SLOT, TableName.REASON_SLOT + "dayrange");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_SLOT, TableName.REASON_SLOT + "datetimerange");
      Database.RemoveColumn (TableName.REASON_SLOT, TableName.REASON_SLOT + "dayrange");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "begindatetime"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "enddatetime"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "beginday"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "endday"));
      Database.AddCheckConstraint ("reasonslot_notempty",
                                   TableName.REASON_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  TableName.REASON_SLOT + "beginDateTime",
                                                  TableName.REASON_SLOT + "endDateTime"));
      RestoreOldIndexes ();
    }

    void CheckPostgreSQL95 ()
    {
      int version = GetPostgresqlVersion ();
      if (version < 9005000) {
        log.FatalFormat ("Please upgrade PostgreSQL first to >= 9.5");
        throw new Exception ("Please upgrade PostgreSQL to >= 9.5");
      }
    }
    
    void RemoveOldIndexes ()
    {
      RemoveIndex(TableName.REASON_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.REASON_SLOT + "endday",
                  TableName.REASON_SLOT + "beginday");
      RemoveIndex(TableName.REASON_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.REASON_SLOT + "enddatetime",
                  TableName.REASON_SLOT + "begindatetime");
      RemoveIndex(TableName.REASON_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.REASON_SLOT + "beginday");
      RemoveUniqueConstraint(TableName.REASON_SLOT,
                             ColumnName.MACHINE_ID,
                             TableName.REASON_SLOT + "begindatetime");
      RemoveUniqueConstraint(TableName.REASON_SLOT,
                             ColumnName.MACHINE_ID,
                             TableName.REASON_SLOT + "enddatetime");
    }
    
    void RestoreOldIndexes ()
    {
      AddUniqueConstraint (TableName.REASON_SLOT,
                           ColumnName.MACHINE_ID,
                           TableName.REASON_SLOT + "begindatetime");
      AddUniqueConstraint (TableName.REASON_SLOT,
                           ColumnName.MACHINE_ID,
                           TableName.REASON_SLOT + "enddatetime");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "enddatetime",
                TableName.REASON_SLOT + "begindatetime");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "endday",
                TableName.REASON_SLOT + "beginday");
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                TableName.REASON_SLOT + "beginday");
    }
  }
}
