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
  /// Migration 262: adapt the database to process the day slots and the shift slots
  /// </summary>
  [Migration(262)]
  public class AddDaySlotProcess: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDaySlotProcess).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DayTemplateChangeUp ();
      DaySlotUp ();
      ShiftSlotUp ();
      AddShiftTemplateSlot ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveShiftTemplateSlot ();
      ShiftSlotDown ();
      DaySlotDown ();
      DayTemplateChangeDown ();
    }
    
    void DayTemplateChangeUp ()
    {
      Database.AddColumn (TableName.DAY_TEMPLATE_CHANGE,
                          new Column (TableName.DAY_TEMPLATE_CHANGE + "force", DbType.Boolean, ColumnProperty.NotNull, false));
    }
    
    void DayTemplateChangeDown ()
    {
      Database.RemoveColumn (TableName.DAY_TEMPLATE_CHANGE,
                             TableName.DAY_TEMPLATE_CHANGE + "force");
    }
    
    void DaySlotUp ()
    {
      // dayslot.day nullable
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.DAY_SLOT,
                                               ColumnName.DAY));

      // add column daytemplateid
      Database.AddColumn (TableName.DAY_SLOT,
                          new Column (ColumnName.DAY_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.DAY_SLOT, ColumnName.DAY_TEMPLATE_ID,
                                   TableName.DAY_TEMPLATE, ColumnName.DAY_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);

      // default value
      Database.ExecuteNonQuery (@"INSERT INTO dayslot (daytemplateid, dayslotrange)
SELECT daytemplateid, '[,)'::tsrange
FROM daytemplate
WHERE daytemplatename='migration'");
      
      // Add an index
      AddGistIndexCondition (TableName.DAY_SLOT, TableName.DAY_SLOT + "range", ColumnName.DAY + " IS NULL");
    }
    
    void DaySlotDown ()
    {
      RemoveIndexCondition (TableName.DAY_SLOT, TableName.DAY_SLOT + "range");
      
      Database.ExecuteNonQuery (@"TRUNCATE dayslot;");
      
      Database.RemoveColumn (TableName.DAY_SLOT, ColumnName.DAY_TEMPLATE_ID);
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.DAY_SLOT,
                                               ColumnName.DAY));
    }

    void ShiftSlotUp ()
    {
      // day column
      Database.AddColumn (TableName.SHIFT_SLOT,
                          ColumnName.DAY, DbType.Date, ColumnProperty.Null);
      Database.ExecuteNonQuery (@"UPDATE shiftslot SET day=shiftslotday;");
      Database.RemoveColumn (TableName.SHIFT_SLOT,
                             TableName.SHIFT_SLOT + "day");
      AddIndex (TableName.SHIFT_SLOT, ColumnName.DAY);
      
      // shiftslottemplateprocessed
      Database.AddColumn (TableName.SHIFT_SLOT,
                          new Column (TableName.SHIFT_SLOT + "templateprocessed", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      Database.ExecuteNonQuery (@"UPDATE shiftslot SET shiftslottemplateprocessed=TRUE");
    }
    
    void ShiftSlotDown ()
    {
      // shiftslotconsolidated
      Database.RemoveColumn (TableName.SHIFT_SLOT, TableName.SHIFT_SLOT + "templateprocessed");
      
      // day column
      Database.AddColumn (TableName.SHIFT_SLOT,
                          TableName.SHIFT_SLOT + "day", DbType.Date);
      Database.ExecuteNonQuery (@"UPDATE shiftslot SET shiftslotday=day;");
      Database.RemoveColumn (TableName.SHIFT_SLOT,
                             ColumnName.DAY);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {1} SET NOT NULL",
                                               TableName.SHIFT_SLOT,
                                               TableName.SHIFT_SLOT + "day"));
    }
    
    void AddShiftTemplateSlot ()
    {
      CheckPostgresqlVersion ();

      Database.AddTable (TableName.SHIFT_TEMPLATE_SLOT,
                         new Column (TableName.SHIFT_TEMPLATE_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_TEMPLATE_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.SHIFT_TEMPLATE_SLOT + "range", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      MakeColumnTsRange (TableName.SHIFT_TEMPLATE_SLOT, TableName.SHIFT_TEMPLATE_SLOT + "range");
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_SLOT, ColumnName.SHIFT_TEMPLATE_ID,
                                   TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddIndex (TableName.SHIFT_TEMPLATE_SLOT,
                ColumnName.SHIFT_TEMPLATE_ID);
      AddGistIndex (TableName.SHIFT_TEMPLATE_SLOT,
                    TableName.SHIFT_TEMPLATE_SLOT + "range");
      AddNoOverlapConstraintV1 (TableName.SHIFT_TEMPLATE_SLOT,
                                TableName.SHIFT_TEMPLATE_SLOT + "range");
    }
    
    void RemoveShiftTemplateSlot ()
    {
      Database.RemoveTable (TableName.SHIFT_TEMPLATE_SLOT);
    }
  }
}
