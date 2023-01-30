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
  /// Migration 248:
  /// </summary>
  [Migration(248)]
  public class AddShiftTemplate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SHIFT_TEMPLATE,
                         new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_TEMPLATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.SHIFT_TEMPLATE + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (TableName.SHIFT_TEMPLATE + "name", DbType.String, ColumnProperty.NotNull));
      AddTimeStampTrigger (TableName.SHIFT_TEMPLATE);
      MakeColumnCaseInsensitive (TableName.SHIFT_TEMPLATE,
                                 TableName.SHIFT_TEMPLATE + "name");
      AddUniqueIndex (TableName.SHIFT_TEMPLATE,
                      TableName.SHIFT_TEMPLATE + "name");
      
      Database.AddTable (TableName.SHIFT_TEMPLATE_ITEM,
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32), // Nullable for NHibernate
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()),
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "timeperiod", DbType.String, 17, ColumnProperty.Null),
                         new Column (TableName.SHIFT_TEMPLATE_ITEM + "day", DbType.DateTime, ColumnProperty.Null));
      AddTimeStampTrigger (TableName.SHIFT_TEMPLATE_ITEM);
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_ITEM, ColumnName.SHIFT_TEMPLATE_ID,
                                   TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_ITEM, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.SHIFT_TEMPLATE_ITEM, ColumnName.SHIFT_TEMPLATE_ID);

      Database.AddTable (TableName.SHIFT_TEMPLATE_BREAK,
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (ColumnName.SHIFT_TEMPLATE_ID, DbType.Int32), // Nullable for NHibernate
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()),
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "timeperiod", DbType.String, 17, ColumnProperty.Null),
                         new Column (TableName.SHIFT_TEMPLATE_BREAK + "day", DbType.DateTime, ColumnProperty.Null));
      AddTimeStampTrigger (TableName.SHIFT_TEMPLATE_BREAK);
      Database.GenerateForeignKey (TableName.SHIFT_TEMPLATE_BREAK, ColumnName.SHIFT_TEMPLATE_ID,
                                   TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.SHIFT_TEMPLATE_BREAK, ColumnName.SHIFT_TEMPLATE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.SHIFT_TEMPLATE_BREAK);
      RemoveTimeStampTrigger (TableName.SHIFT_TEMPLATE_BREAK);

      Database.RemoveTable (TableName.SHIFT_TEMPLATE_ITEM);
      RemoveTimeStampTrigger (TableName.SHIFT_TEMPLATE_ITEM);
      
      Database.RemoveTable (TableName.SHIFT_TEMPLATE);
      RemoveTimeStampTrigger (TableName.SHIFT_TEMPLATE);
    }
  }
}
