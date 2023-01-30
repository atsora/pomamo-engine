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
  /// Migration 904: add the column "rolewebappkey" in the table "role"
  /// </summary>
  [Migration (904)]
  public class AddRoleWebAppKey : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddRoleWebAppKey).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.ROLE, new Column (TableName.ROLE + "webappkey", DbType.String));
      MakeColumnCaseInsensitive (TableName.ROLE, TableName.ROLE + "webappkey");

      // Default value is the content of "TranslationKey" without the prefix "Role"
      Database.ExecuteNonQuery ("UPDATE role SET rolewebappkey = lower(REPLACE(roletranslationkey, 'Role', '')) WHERE rolewebappkey is NULL;");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.ROLE, TableName.ROLE + "webappkey");
    }
  }
}
