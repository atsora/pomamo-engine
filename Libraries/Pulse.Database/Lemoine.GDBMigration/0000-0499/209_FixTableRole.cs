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
  /// Migration 209: fix the table role where the type of roletranslationkey is wrong
  /// </summary>
  [Migration(209)]
  public class FixTableRole: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixTableRole).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // The next command was to fix the migration 191. Hopefully this is not needed any more
      // since ChangeColumn does not work well with PostgreSQL 12
      /*
      Database.ChangeColumn (TableName.ROLE,
                             new Column (TableName.ROLE + "translationkey", DbType.String, ColumnProperty.Unique));
      AddConstraintNameTranslationKey (TableName.ROLE,
                                       TableName.ROLE + "name",
                                       TableName.ROLE + "translationkey");
      AddUniqueIndex (TableName.ROLE,
                      TableName.ROLE + "name",
                      TableName.ROLE + "translationkey");
      */
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
