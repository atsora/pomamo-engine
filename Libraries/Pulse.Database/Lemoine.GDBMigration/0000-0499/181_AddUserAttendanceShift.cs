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
  /// Migration 181: Add the notion that a user may change dynamically its work shift
  /// </summary>
  [Migration(181)]
  public class AddUserAttendanceShift: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddUserAttendanceShift).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // userattendance
      Database.AddColumn (TableName.USER_ATTENDANCE,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.USER_ATTENDANCE, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      // userslot
      Database.AddColumn (TableName.USER_SLOT,
                          new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.USER_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      // machineobservationstate
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (TableName.MACHINE_OBSERVATION_STATE + "shiftrequired", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE, TableName.MACHINE_OBSERVATION_STATE + "shiftrequired");
      Database.RemoveColumn (TableName.USER_SLOT, ColumnName.SHIFT_ID);
      Database.RemoveColumn (TableName.USER_ATTENDANCE, ColumnName.SHIFT_ID);
    }
  }
}
