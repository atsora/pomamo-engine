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
  /// Migration 095: add the following new shift tables
  /// <item>shift</item>
  /// <item>shiftchange</item>
  /// <item>shiftslot</item>
  /// <item>machinecompanyupdate</item>
  /// <item>machinedepartmentupdate</item>
  /// </summary>
  [Migration(95)]
  public class AddNewShiftTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNewShiftTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddShiftTable ();
      AddShiftChangeTable ();
      AddShiftSlotTable ();
      AddMachineCompanyUpdateTable ();
      AddMachineDepartmentUpdateTable ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveMachineDepartmentUpdateTable ();
      RemoveMachineCompanyUpdateTable ();
      RemoveShiftSlotTable ();
      RemoveShiftChangeTable ();
      RemoveShiftTable ();
    }
    
    void AddShiftTable ()
    {
      Database.AddTable (TableName.SHIFT,
                         new Column (TableName.SHIFT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.SHIFT + "name", DbType.String),
                         new Column (TableName.SHIFT + "code", DbType.String),
                         new Column (TableName.SHIFT + "externalcode", DbType.String));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {0}{1}
SET DATA TYPE CITEXT;",
                                               TableName.SHIFT,
                                               "name"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {0}{1}
SET DATA TYPE CITEXT;",
                                               TableName.SHIFT,
                                               "code"));
      Database.AddCheckConstraint ("shift_name_code",
                                   TableName.SHIFT,
                                   "((shiftname IS NOT NULL) OR (shiftcode IS NOT NULL))");
      AddUniqueConstraint (TableName.SHIFT, TableName.SHIFT + "name");
      AddUniqueConstraint (TableName.SHIFT, TableName.SHIFT + "code");
      AddUniqueConstraint (TableName.SHIFT, TableName.SHIFT + "externalcode");
      AddIndexCondition (TableName.SHIFT, "shiftname IS NOT NULL", TableName.SHIFT + "name");
      AddIndexCondition (TableName.SHIFT, "shiftcode IS NOT NULL", TableName.SHIFT + "code");
      AddIndexCondition (TableName.SHIFT, "shiftexternalcode IS NOT NULL", TableName.SHIFT + "externalcode");
    }
    
    void RemoveShiftTable ()
    {
      Database.RemoveTable (TableName.SHIFT);
    }
    
    void AddShiftChangeTable ()
    {
      Database.AddTable (TableName.SHIFT_CHANGE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.COMPANY_ID, DbType.Int32),
                         new Column (ColumnName.DEPARTMENT_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32),
                         new Column (TableName.SHIFT_CHANGE + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_CHANGE + "end", DbType.DateTime));
      Database.AddCheckConstraint ("shiftchange_company_department_machine",
                                   TableName.SHIFT_CHANGE,
                                   @"(companyid IS NOT NULL) OR (departmentid IS NOT NULL) OR (machineid IS NOT NULL)");
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.COMPANY_ID,
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.DEPARTMENT_ID,
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_CHANGE, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.SHIFT_CHANGE);
      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.COMPANY_ID);
      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.DEPARTMENT_ID);
      AddIndex (TableName.SHIFT_CHANGE,
                ColumnName.MACHINE_ID);
    }
    
    void RemoveShiftChangeTable ()
    {
      Database.RemoveTable (TableName.SHIFT_CHANGE);
    }
    
    void AddShiftSlotTable ()
    {
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
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                beginDateTime);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           ColumnName.MACHINE_ID,
                           beginDateTime);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                beginDay);
      AddIndex (TableName.SHIFT_SLOT,
                ColumnName.MACHINE_ID,
                endDateTime);
      AddUniqueConstraint (TableName.SHIFT_SLOT,
                           ColumnName.MACHINE_ID,
                           endDateTime);
    }
    
    void RemoveShiftSlotTable ()
    {
      Database.RemoveTable (TableName.SHIFT_SLOT);
    }
    
    void AddMachineCompanyUpdateTable ()
    {
      Database.AddTable (TableName.MACHINE_COMPANY_UPDATE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("oldcompanyid", DbType.Int32),
                         new Column ("newcompanyid", DbType.Int32));
      Database.AddCheckConstraint ("machinecompanyupdate_old_new",
                                   TableName.MACHINE_COMPANY_UPDATE,
                                   @"(oldcompanyid IS NOT NULL) OR (newcompanyid IS NOT NULL)");
      Database.GenerateForeignKey (TableName.MACHINE_COMPANY_UPDATE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_COMPANY_UPDATE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_COMPANY_UPDATE, "oldcompanyid",
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_COMPANY_UPDATE, "newcompanyid",
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.MACHINE_COMPANY_UPDATE);
    }
    
    void RemoveMachineCompanyUpdateTable ()
    {
      Database.RemoveTable (TableName.MACHINE_COMPANY_UPDATE);
    }
    
    void AddMachineDepartmentUpdateTable ()
    {
      Database.AddTable (TableName.MACHINE_DEPARTMENT_UPDATE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("olddepartmentid", DbType.Int32),
                         new Column ("newdepartmentid", DbType.Int32));
      Database.AddCheckConstraint ("machinecompanydepartment_old_new",
                                   TableName.MACHINE_DEPARTMENT_UPDATE,
                                   @"(olddepartmentid IS NOT NULL) OR (newdepartmentid IS NOT NULL)");
      Database.GenerateForeignKey (TableName.MACHINE_DEPARTMENT_UPDATE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_DEPARTMENT_UPDATE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_DEPARTMENT_UPDATE, "olddepartmentid",
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_DEPARTMENT_UPDATE, "newdepartmentid",
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.MACHINE_DEPARTMENT_UPDATE);
    }
    
    void RemoveMachineDepartmentUpdateTable ()
    {
      Database.RemoveTable (TableName.MACHINE_DEPARTMENT_UPDATE);
    }
  }
}
