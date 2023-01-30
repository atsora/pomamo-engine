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
  /// Migration 092: Add the current value tables:
  /// <item>currentcncvalue</item>
  /// <item>currentmachinemode</item>
  /// </summary>
  [Migration(92)]
  public class AddCurrentValues: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCurrentValues).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddCurrentCncValue ();
      AddCurrentMachineMode ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveCurrentMachineMode ();
      RemoveCurrentCncValue ();
    }
    
    void AddCurrentCncValue ()
    {
      Database.AddTable (TableName.CURRENT_CNC_VALUE,
                         new Column (TableName.CURRENT_CNC_VALUE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.CURRENT_CNC_VALUE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.CURRENT_CNC_VALUE + "datetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.CURRENT_CNC_VALUE + "string", DbType.String),
                         new Column (TableName.CURRENT_CNC_VALUE + "int", DbType.Int32),
                         new Column (TableName.CURRENT_CNC_VALUE + "double", DbType.Double));
      Database.GenerateForeignKey (TableName.CURRENT_CNC_VALUE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.CURRENT_CNC_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.CURRENT_CNC_VALUE,
                           ColumnName.MACHINE_MODULE_ID, ColumnName.FIELD_ID);
      AddIndex (TableName.CURRENT_CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID);
    }
    
    void RemoveCurrentCncValue ()
    {
      Database.RemoveTable (TableName.CURRENT_CNC_VALUE);
    }
    
    void AddCurrentMachineMode ()
    {
      Database.AddTable (TableName.CURRENT_MACHINE_MODE,
                         new Column (TableName.CURRENT_MACHINE_MODE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.CURRENT_MACHINE_MODE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.CURRENT_MACHINE_MODE + "datetime", DbType.DateTime, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.CURRENT_MACHINE_MODE, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.CURRENT_MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.CURRENT_MACHINE_MODE,
                           ColumnName.MACHINE_ID);
      AddIndex (TableName.CURRENT_MACHINE_MODE,
                ColumnName.MACHINE_ID);
    }
    
    void RemoveCurrentMachineMode ()
    {
      Database.RemoveTable (TableName.CURRENT_MACHINE_MODE);
    }
  }
}
