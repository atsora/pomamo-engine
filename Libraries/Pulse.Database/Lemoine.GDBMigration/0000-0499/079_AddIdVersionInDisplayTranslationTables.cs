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
  /// Migration 079: Add Id and Version columns into the Display and Translation tables
  /// </summary>
  [Migration(79)]
  public class AddIdVersionInDisplayTranslationTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIdVersionInDisplayTranslationTables).FullName);
    
    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpgradeDisplayTable ();
      UpgradeTranslationTable ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeDisplayTable ();
      DowngradeTranslationTable ();
    }
    
    void UpgradeDisplayTable ()
    {
      Database.RenameTable (TableName.DISPLAY,
                            TableName.DISPLAY + MIG_SUFFIX);
      
      Database.AddTable (TableName.DISPLAY,
                         new Column ("displaytable", DbType.String, ColumnProperty.NotNull),
                         new Column ("displaypattern", DbType.String),
                         new Column (TableName.DISPLAY + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DISPLAY + "version", DbType.Int32, ColumnProperty.NotNull, 1));
      Database.ExecuteNonQuery ("ALTER TABLE display " +
                                "ALTER COLUMN displaytable " +
                                "SET DATA TYPE CITEXT;");
      AddUniqueConstraint (TableName.DISPLAY,
                           TableName.DISPLAY + "table");
      AddIndex (TableName.DISPLAY,
                TableName.DISPLAY + "table");
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(displaytable, displaypattern)
SELECT displaytable, displaypattern
FROM {0}{1}",
                                               TableName.DISPLAY,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.DISPLAY + MIG_SUFFIX);
    }
    
    void DowngradeDisplayTable ()
    {
      RemoveUniqueConstraint (TableName.DISPLAY,
                              TableName.DISPLAY + "table");
      RemoveIndex (TableName.DISPLAY,
                   TableName.DISPLAY + "table");
      Database.RenameTable (TableName.DISPLAY,
                            TableName.DISPLAY + MIG_SUFFIX);
      
      Database.AddTable (TableName.DISPLAY,
                         new Column ("displaytable", DbType.String, ColumnProperty.PrimaryKey),
                         new Column ("displaypattern", DbType.String));
      Database.ExecuteNonQuery ("ALTER TABLE display " +
                                "ALTER COLUMN displaytable " +
                                "SET DATA TYPE CITEXT;");
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(displaytable, displaypattern)
SELECT displaytable, displaypattern
FROM {0}{1}",
                                               TableName.DISPLAY,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.DISPLAY + MIG_SUFFIX);
    }
    
    void UpgradeTranslationTable ()
    {
      RemoveIndex (TableName.TRANSLATION,
                   ColumnName.TRANSLATION_KEY);
      Database.RenameTable (TableName.TRANSLATION,
                            TableName.TRANSLATION + MIG_SUFFIX);
      
      Database.AddTable (TableName.TRANSLATION,
                         new Column (ColumnName.LOCALE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.TRANSLATION_KEY, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.TRANSLATION_VALUE, DbType.String, ColumnProperty.NotNull),
                         new Column (TableName.TRANSLATION + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.TRANSLATION + "version", DbType.Int32, ColumnProperty.NotNull, 1));
      Database.ExecuteNonQuery ("ALTER TABLE translation " +
                                "ALTER COLUMN locale " +
                                "SET DATA TYPE CITEXT;");
      AddUniqueConstraint (TableName.TRANSLATION,
                           ColumnName.LOCALE,
                           ColumnName.TRANSLATION_KEY);
      AddIndex (TableName.TRANSLATION,
                ColumnName.TRANSLATION_KEY);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(locale, translationkey, translationvalue)
SELECT locale, translationkey, translationvalue
FROM {0}{1}",
                                               TableName.TRANSLATION,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.TRANSLATION + MIG_SUFFIX);
    }
    
    void DowngradeTranslationTable ()
    {
      RemoveUniqueConstraint (TableName.TRANSLATION,
                              ColumnName.LOCALE,
                              ColumnName.TRANSLATION_KEY);
      RemoveIndex (TableName.TRANSLATION,
                   ColumnName.TRANSLATION_KEY);
      Database.RenameTable (TableName.TRANSLATION,
                            TableName.TRANSLATION + MIG_SUFFIX);
      
      Database.AddTable (TableName.TRANSLATION,
                         new Column (ColumnName.LOCALE, DbType.String, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.TRANSLATION_KEY, DbType.String, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.TRANSLATION_VALUE, DbType.String, ColumnProperty.NotNull));
      Database.ExecuteNonQuery ("CREATE INDEX translation_translationkey_idx " +
                                "ON translation (translationkey);");
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(locale, translationkey, translationvalue)
SELECT locale, translationkey, translationvalue
FROM {0}{1}",
                                               TableName.TRANSLATION,
                                               MIG_SUFFIX));
      Database.RemoveTable (TableName.TRANSLATION + MIG_SUFFIX);
    }
  }
}
