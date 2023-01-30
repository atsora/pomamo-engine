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
  /// Migration 263: migrate the columns begin/end to range
  /// </summary>
  [Migration(263)]
  public class MigrateShiftSlotToRange: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateShiftSlotToRange).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CheckPostgresqlVersion ();

      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (TableName.SHIFT_SLOT + "range", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.SHIFT_SLOT, TableName.SHIFT_SLOT + "range");
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}range=tsrange({0}begindatetime,{0}enddatetime)",
                                               TableName.SHIFT_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}range=tsrange(NULL,upper({0}range))
WHERE lower({0}range)='1970-01-01 00:00:00'",
                                               TableName.SHIFT_SLOT));
      Database.RemoveColumn (TableName.SHIFT_SLOT, TableName.SHIFT_SLOT + "begindatetime");
      Database.RemoveColumn (TableName.SHIFT_SLOT, TableName.SHIFT_SLOT + "enddatetime");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.SHIFT_SLOT,
                                               "range"));
      AddGistIndex (TableName.SHIFT_SLOT,
                    TableName.SHIFT_SLOT + "range");
      AddNoOverlapConstraintV1 (TableName.SHIFT_SLOT,
                                TableName.SHIFT_SLOT + "range");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (TableName.SHIFT_SLOT + "begindatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (TableName.SHIFT_SLOT + "enddatetime", DbType.DateTime));
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begindatetime=lower({0}range)
WHERE NOT lower_inf({0}range)",
                                               TableName.SHIFT_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begindatetime='1970-01-01 00:00:00'
WHERE lower_inf({0}range)",
                                               TableName.SHIFT_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}enddatetime=upper({0}range)
WHERE NOT upper_inf({0}range)",
                                               TableName.SHIFT_SLOT));
      Database.RemoveColumn (TableName.SHIFT_SLOT, TableName.SHIFT_SLOT + "range");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.SHIFT_SLOT,
                                               "begindatetime"));
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           TableName.SHIFT_SLOT + "begindatetime");
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           TableName.SHIFT_SLOT + "enddatetime");
      AddIndex (TableName.SHIFT_SLOT,
                TableName.SHIFT_SLOT + "enddatetime",
                TableName.SHIFT_SLOT + "begindatetime");
    }
  }
}
