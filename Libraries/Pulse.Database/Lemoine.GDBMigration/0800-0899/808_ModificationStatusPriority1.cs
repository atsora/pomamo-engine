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
  /// Migration 808: 
  /// </summary>
  [Migration (808)]
  public class ModificationStatusPriority1 : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ModificationStatusPriority1).FullName);

    static readonly string COLUMN_NAME = "modificationstatuspriority";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Up (TableName.GLOBAL_MODIFICATION);
      Up (TableName.MACHINE_MODIFICATION);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Down (TableName.MACHINE_MODIFICATION);
      Down (TableName.GLOBAL_MODIFICATION);
    }

    void Up (string modificationTable)
    {
      string modificationStatusTable = modificationTable + "status";

      Database.AddColumn (modificationStatusTable,
        new Column (COLUMN_NAME, DbType.Int32, ColumnProperty.Null, "100"));
    }

    void Down (string modificationTable)
    {
      string modificationStatusTable = modificationTable + "status";
      Database.RemoveColumn (modificationStatusTable, COLUMN_NAME);
    }
  }
}
