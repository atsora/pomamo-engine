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
  /// Migration 090: add version and id to StampingValue table and migrate past values
  /// </summary>
  [Migration(90)]
  public class AddIdVersionToStampingValueTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIdVersionToStampingValueTable).FullName);
    
    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.STAMPING_VALUE,
                   ColumnName.SEQUENCE_ID);
      
      Database.RenameTable (TableName.STAMPING_VALUE,
                            TableName.STAMPING_VALUE + MIG_SUFFIX);
      
      Database.AddTable (TableName.STAMPING_VALUE,
                         new Column (ColumnName.STAMPINGVALUE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.STAMPINGVALUE_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.SEQUENCE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.STAMPINGVALUE_STRING, DbType.String),
                         new Column (ColumnName.STAMPINGVALUE_INT, DbType.Int32),
                         new Column (ColumnName.STAMPINGVALUE_DOUBLE, DbType.Double));
      
      AddNamedUniqueConstraint ("stampingvalue_unique_sequenceid_fieldid",
                             TableName.STAMPING_VALUE,
                             new string[] {ColumnName.SEQUENCE_ID, ColumnName.FIELD_ID});
      
      Database.GenerateForeignKey (TableName.STAMPING_VALUE, ColumnName.SEQUENCE_ID,
                                   TableName.SEQUENCE, ColumnName.SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      Database.GenerateForeignKey (TableName.STAMPING_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      
      ResetSequence (TableName.STAMPING_VALUE, ColumnName.STAMPINGVALUE_ID);
      
      AddIndex(TableName.STAMPING_VALUE, ColumnName.SEQUENCE_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(sequenceid, fieldid, stampingvaluestring, stampingvalueint, stampingvaluedouble)
SELECT sequenceid, fieldid, stampingvaluestring, stampingvalueint, stampingvaluedouble
FROM {0}{1}",
                                               TableName.STAMPING_VALUE,
                                               MIG_SUFFIX));
      
      Database.RemoveTable (TableName.STAMPING_VALUE + MIG_SUFFIX);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.STAMPING_VALUE,
                   ColumnName.SEQUENCE_ID);

      Database.RenameTable (TableName.STAMPING_VALUE,
                            TableName.STAMPING_VALUE + MIG_SUFFIX);

      Database.AddTable (TableName.STAMPING_VALUE,
                         new Column (ColumnName.SEQUENCE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.STAMPINGVALUE_STRING, DbType.String),
                         new Column (ColumnName.STAMPINGVALUE_INT, DbType.Int32),
                         new Column (ColumnName.STAMPINGVALUE_DOUBLE, DbType.Double));

      Database.AddPrimaryKey("pk_stampingvalue", TableName.STAMPING_VALUE,
                             new string[] { ColumnName.SEQUENCE_ID, ColumnName.FIELD_ID });
      
      AddIndex(TableName.STAMPING_VALUE, ColumnName.SEQUENCE_ID);

      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(sequenceid, fieldid, stampingvaluestring, stampingvalueint, stampingvaluedouble)
SELECT sequenceid, fieldid, stampingvaluestring, stampingvalueint, stampingvaluedouble
FROM {0}{1}",
                                               TableName.STAMPING_VALUE,
                                               MIG_SUFFIX));
      
      Database.RemoveTable (TableName.STAMPING_VALUE + MIG_SUFFIX);
    }
  }
}
