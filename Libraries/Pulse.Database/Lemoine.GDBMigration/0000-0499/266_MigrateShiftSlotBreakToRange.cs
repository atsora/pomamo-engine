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
  /// Migration 266: migrate the columns begin/end to range in table shiftslotbreak
  /// </summary>
  [Migration(266)]
  public class MigrateShiftSlotBreakToRange: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateShiftSlotBreakToRange).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CheckPostgresqlVersion ();

      Database.AddColumn (TableName.SHIFT_SLOT_BREAK,
                          new Column (TableName.SHIFT_SLOT_BREAK + "range", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.SHIFT_SLOT_BREAK, TableName.SHIFT_SLOT_BREAK + "range");
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}range=tsrange({0}begin,{0}end)",
                                               TableName.SHIFT_SLOT_BREAK));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}range=tsrange(NULL,upper({0}range))
WHERE lower({0}range)='1970-01-01 00:00:00'",
                                               TableName.SHIFT_SLOT_BREAK));
      Database.RemoveColumn (TableName.SHIFT_SLOT_BREAK, TableName.SHIFT_SLOT_BREAK + "begin");
      Database.RemoveColumn (TableName.SHIFT_SLOT_BREAK, TableName.SHIFT_SLOT_BREAK + "end");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.SHIFT_SLOT_BREAK,
                                               "range"));
      AddGistIndex (TableName.SHIFT_SLOT_BREAK,
                    TableName.SHIFT_SLOT_BREAK + "range");
      AddNoOverlapConstraintV1 (TableName.SHIFT_SLOT_BREAK,
                                TableName.SHIFT_SLOT_BREAK + "range");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.AddColumn (TableName.SHIFT_SLOT_BREAK,
                          new Column (TableName.SHIFT_SLOT_BREAK + "begin", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.SHIFT_SLOT_BREAK,
                          new Column (TableName.SHIFT_SLOT_BREAK + "end", DbType.DateTime, ColumnProperty.Null));
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begin=lower({0}range)
WHERE NOT lower_inf({0}range)",
                                               TableName.SHIFT_SLOT_BREAK));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begin='1970-01-01 00:00:00'
WHERE lower_inf({0}range)",
                                               TableName.SHIFT_SLOT_BREAK));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}end=upper({0}range)
WHERE NOT upper_inf({0}range)",
                                               TableName.SHIFT_SLOT_BREAK));
      Database.RemoveColumn (TableName.SHIFT_SLOT_BREAK, TableName.SHIFT_SLOT_BREAK + "range");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.SHIFT_SLOT_BREAK,
                                               "begin"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.SHIFT_SLOT_BREAK,
                                               "end"));
      AddUniqueConstraint (TableName.SHIFT_SLOT_BREAK,
                           TableName.SHIFT_SLOT_BREAK + "begin");
      AddUniqueConstraint (TableName.SHIFT_SLOT_BREAK,
                           TableName.SHIFT_SLOT_BREAK + "end");
      AddIndex (TableName.SHIFT_SLOT_BREAK,
                TableName.SHIFT_SLOT_BREAK + "end",
                TableName.SHIFT_SLOT_BREAK + "begin");
    }
  }
}
