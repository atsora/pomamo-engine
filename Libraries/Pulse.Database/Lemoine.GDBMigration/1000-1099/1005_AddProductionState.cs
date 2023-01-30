// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1005: add a productionstate table 
  /// </summary>
  [Migration (1005)]
  public class AddProductionState : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddProductionState).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.PRODUCTION_STATE,
                         new Column (ColumnName.PRODUCTION_STATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ($"{TableName.PRODUCTION_STATE}version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ($"{TableName.PRODUCTION_STATE}name", DbType.String, ColumnProperty.Unique),
                         new Column ($"{TableName.PRODUCTION_STATE}translationkey", DbType.String, ColumnProperty.Unique),
                         new Column ($"{TableName.PRODUCTION_STATE}description", DbType.String),
                         new Column ($"{TableName.PRODUCTION_STATE}descriptiontranslationkey", DbType.String),
                         new Column ($"{TableName.PRODUCTION_STATE}color", DbType.String, 7, ColumnProperty.NotNull),
                         new Column ($"{TableName.PRODUCTION_STATE}displaypriority", DbType.Int32));
      MakeColumnCaseInsensitive (TableName.PRODUCTION_STATE,
                                 $"{TableName.PRODUCTION_STATE}name");
      AddConstraintNameTranslationKey (TableName.PRODUCTION_STATE,
                                       $"{TableName.PRODUCTION_STATE}name",
                                       $"{TableName.PRODUCTION_STATE}translationkey");
      AddConstraintColor (TableName.PRODUCTION_STATE,
                          $"{TableName.PRODUCTION_STATE}color");
      AddUniqueIndex (TableName.PRODUCTION_STATE,
                      $"{TableName.PRODUCTION_STATE}name",
                      $"{TableName.PRODUCTION_STATE}translationkey");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.PRODUCTION_STATE);
    }
  }
}
