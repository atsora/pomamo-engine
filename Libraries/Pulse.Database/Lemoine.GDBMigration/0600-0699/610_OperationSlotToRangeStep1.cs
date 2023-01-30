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
  /// Migration 610:
  /// 
  /// PostgreSQL >= 9.5 is required
  /// </summary>
  [Migration(610)]
  public class OperationSlotToRangeStep1: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotToRangeStep1).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CheckPostgreSQL95 ();
      
      RemoveOldIndexes ();

      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "datetimerange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "datetimerange");
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "dayrange", DbType.Int32, ColumnProperty.Null));
      MakeColumnDateRange (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "dayrange");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "datetimerange");
      Database.RemoveColumn (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "dayrange");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "begindatetime"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "enddatetime"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "beginday"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "endday"));
      Database.AddCheckConstraint ("operationslot_notempty",
                                   TableName.OPERATION_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  TableName.OPERATION_SLOT + "beginDateTime",
                                                  TableName.OPERATION_SLOT + "endDateTime"));
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
      RemoveIndex(TableName.OPERATION_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OPERATION_SLOT + "endday",
                  TableName.OPERATION_SLOT + "beginday");
      RemoveIndex(TableName.OPERATION_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OPERATION_SLOT + "enddatetime",
                  TableName.OPERATION_SLOT + "begindatetime");
      RemoveIndex(TableName.OPERATION_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OPERATION_SLOT + "beginday");
      RemoveIndex(TableName.OPERATION_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OPERATION_SLOT + "begindatetime");
      RemoveUniqueConstraint(TableName.OPERATION_SLOT,
                             ColumnName.MACHINE_ID,
                             TableName.OPERATION_SLOT + "begindatetime");
      RemoveUniqueConstraint(TableName.OPERATION_SLOT,
                             ColumnName.MACHINE_ID,
                             TableName.OPERATION_SLOT + "enddatetime");
    }
    
    void RestoreOldIndexes ()
    {
      AddUniqueConstraint (TableName.OPERATION_SLOT,
                           ColumnName.MACHINE_ID,
                           TableName.OPERATION_SLOT + "begindatetime");
      /* // Note before
      AddUniqueConstraint (TableName.OPERATION_SLOT,
                           ColumnName.MACHINE_ID,
                           TableName.OPERATION_SLOT + "enddatetime");
       */
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "enddatetime",
                TableName.OPERATION_SLOT + "begindatetime");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "endday",
                TableName.OPERATION_SLOT + "beginday");
      AddIndex (TableName.OPERATION_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OPERATION_SLOT + "beginday");
    }
  }
}
