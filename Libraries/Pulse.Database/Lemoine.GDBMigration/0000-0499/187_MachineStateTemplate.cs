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
  /// Migration 187: Add the tables for the machine state template
  /// </summary>
  [Migration(187)]
  public class MachineStateTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE,
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_STATE_TEMPLATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.MACHINE_STATE_TEMPLATE + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (TableName.MACHINE_STATE_TEMPLATE + "name", DbType.String, ColumnProperty.NotNull));
      AddTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE);
      MakeColumnCaseInsensitive (TableName.MACHINE_STATE_TEMPLATE,
                                 TableName.MACHINE_STATE_TEMPLATE + "name");
      AddUniqueIndex (TableName.MACHINE_STATE_TEMPLATE,
                      TableName.MACHINE_STATE_TEMPLATE + "name");
      
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "order", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "timeperiod", DbType.String, 17, ColumnProperty.Null),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_ITEM + "day", DbType.DateTime, ColumnProperty.Null));
      AddTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE_ITEM);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueConstraint (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                           ColumnName.MACHINE_STATE_TEMPLATE_ID, TableName.MACHINE_STATE_TEMPLATE_ITEM + "order");
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_STATE_TEMPLATE_ID);

      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_STOP,
                         new Column (TableName.MACHINE_STATE_TEMPLATE_STOP + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_STOP + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_STOP + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_STOP + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()),
                         new Column (TableName.MACHINE_STATE_TEMPLATE_STOP + "localtime", DbType.Time, ColumnProperty.Null));
      AddTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE_STOP);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_STOP, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_STOP, ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_STOP);
      RemoveTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE_STOP);

      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_ITEM);
      RemoveTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE_ITEM);
      
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE);
      RemoveTimeStampTrigger (TableName.MACHINE_STATE_TEMPLATE);
    }
  }
}
