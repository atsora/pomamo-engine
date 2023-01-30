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
  /// Migration 1301: Add a package detail column
  /// </summary>
  [Migration (1301)]
  public class AddPackageDetail : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddSequenceDetail).FullName);

    static readonly string PACKAGE_DETAIL = TableName.PACKAGE + "detail";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.PACKAGE,
        new Column (PACKAGE_DETAIL, DbType.String));
      MakeColumnJson (TableName.PACKAGE, PACKAGE_DETAIL);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.PACKAGE, PACKAGE_DETAIL);
    }
  }
}
