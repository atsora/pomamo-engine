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
  /// Migration 080: add the FieldLegend table
  /// </summary>
  [Migration(80)]
  public class FieldLegend: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FieldLegend).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.FIELD_LEGEND,
                         new Column (TableName.FIELD_LEGEND + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.FIELD_LEGEND + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.FIELD_LEGEND + "stringvalue", DbType.String, ColumnProperty.Null),
                         new Column (TableName.FIELD_LEGEND + "minvalue", DbType.Double, ColumnProperty.Null),
                         new Column (TableName.FIELD_LEGEND + "maxvalue", DbType.Double, ColumnProperty.Null),
                         new Column (TableName.FIELD_LEGEND + "text", DbType.String, ColumnProperty.NotNull),
                         new Column (TableName.FIELD_LEGEND + "color", DbType.String, 7, ColumnProperty.NotNull));
      Database.ExecuteNonQuery ("ALTER TABLE fieldlegend " +
                                "ALTER COLUMN fieldlegendstringvalue " +
                                "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery ("ALTER TABLE fieldlegend " +
                                "ALTER COLUMN fieldlegendcolor " +
                                "SET DATA TYPE CITEXT;");
      Database.GenerateForeignKey (TableName.FIELD_LEGEND, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.FIELD_LEGEND, ColumnName.FIELD_ID);
      AddConstraintColor (TableName.FIELD_LEGEND, TableName.FIELD_LEGEND + "color");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.FIELD_LEGEND);
    }
  }
}
