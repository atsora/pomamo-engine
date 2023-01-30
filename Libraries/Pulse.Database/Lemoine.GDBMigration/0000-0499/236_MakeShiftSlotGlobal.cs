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
  /// Migration 236: make the shift slots global and not by machine
  /// </summary>
  [Migration(236)]
  public class MakeShiftSlotGlobal: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MakeShiftSlotGlobal).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      ShiftSlotUp ();
      ShiftChangeUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      ShiftChangeDown ();
      ShiftSlotDown ();
    }
    
    void ShiftSlotUp ()
    {
      Database.RemoveTable (TableName.SHIFT_SLOT);
      
      string beginDateTime = TableName.SHIFT_SLOT + "begindatetime";
      string endDateTime = TableName.SHIFT_SLOT + "enddatetime";
      string day = TableName.SHIFT_SLOT + "day";
      Database.AddTable (TableName.SHIFT_SLOT,
                         new Column (TableName.SHIFT_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (beginDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (endDateTime, DbType.DateTime),
                         new Column (day, DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32));
      Database.AddCheckConstraint ("shiftslot_notempty",
                                   TableName.SHIFT_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  beginDateTime, endDateTime));
      Database.GenerateForeignKey (TableName.SHIFT_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           beginDateTime);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           endDateTime);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           day,
                           ColumnName.SHIFT_ID);
      AddIndex (TableName.SHIFT_SLOT,
                TableName.SHIFT_SLOT + "enddatetime",
                TableName.SHIFT_SLOT + "begindatetime");
      
      // Add the initial data
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0} ({0}begindatetime, {0}day)
VALUES ('1970-01-01 00:00:00', '1970-01-01 00:00:00')",
                                               TableName.SHIFT_SLOT));
    }
    
    void ShiftSlotDown ()
    {
      Database.RemoveTable (TableName.SHIFT_SLOT);
      
      string beginDateTime = TableName.SHIFT_SLOT + "begindatetime";
      string endDateTime = TableName.SHIFT_SLOT + "enddatetime";
      string beginDay = TableName.SHIFT_SLOT + "beginday";
      Database.AddTable (TableName.SHIFT_SLOT,
                         new Column (TableName.SHIFT_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (beginDateTime, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (beginDay, DbType.Date, ColumnProperty.NotNull),
                         new Column (endDateTime, DbType.DateTime),
                         new Column (TableName.SHIFT_SLOT + "endDay", DbType.Date),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.AddCheckConstraint ("shiftslot_notempty",
                                   TableName.SHIFT_SLOT,
                                   string.Format ("{0} <> {1}",
                                                  beginDateTime, endDateTime));
      Database.GenerateForeignKey (TableName.SHIFT_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           ColumnName.MACHINE_ID,
                           beginDateTime);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                beginDay);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           ColumnName.MACHINE_ID,
                           endDateTime);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "endday",
                TableName.SHIFT_SLOT + "beginday");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "enddatetime",
                TableName.SHIFT_SLOT + "begindatetime");
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                TableName.SHIFT_SLOT + "begindatetime");
    }
    
    void ShiftChangeUp ()
    {
      Database.RemoveColumn (TableName.SHIFT_CHANGE,
                             ColumnName.COMPANY_ID);
      Database.RemoveColumn (TableName.SHIFT_CHANGE,
                             ColumnName.DEPARTMENT_ID);
      Database.RemoveColumn (TableName.SHIFT_CHANGE,
                             ColumnName.MACHINE_ID);
    }
    
    void ShiftChangeDown ()
    {
      Database.AddColumn (TableName.SHIFT_CHANGE,
                          new Column (ColumnName.COMPANY_ID, DbType.Int32));
      Database.AddColumn (TableName.SHIFT_CHANGE,
                          new Column (ColumnName.DEPARTMENT_ID, DbType.Int32));
      Database.AddColumn (TableName.SHIFT_CHANGE,
                          new Column (ColumnName.MACHINE_ID, DbType.Int32));
      
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.COMPANY_ID,
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.DEPARTMENT_ID,
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.COMPANY_ID);
      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.DEPARTMENT_ID);
      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.MACHINE_ID);
    }
  }
}
