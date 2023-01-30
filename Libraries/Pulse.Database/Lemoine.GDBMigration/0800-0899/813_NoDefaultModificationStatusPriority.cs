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
  /// Migration 813: no default modificationstatuspriority value
  /// </summary>
  [Migration (813)]
  public class NoDefaultModificationStatusPriority : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NoDefaultModificationStatusPriority).FullName);

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
    }

    void Up (string modificationTable)
    {
      string modificationStatusTable = modificationTable + "status";

      SetNotNull (modificationStatusTable, COLUMN_NAME);
    }
  }
}
