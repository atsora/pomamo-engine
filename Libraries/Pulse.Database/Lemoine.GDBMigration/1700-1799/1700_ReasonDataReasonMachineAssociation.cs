// Copyright (C) 2024 Atsora Solutions
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
  /// Migration 1700: Add a column reasondata in reasonmachineassociation table
  /// </summary>
  [Migration (1700)]
  public class AddReasonDataReasonMachineAssociation : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonDataReasonMachineAssociation).FullName);

    readonly static string REASON_DATA_COLUMN = "reasondata";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION,
        new Column (REASON_DATA_COLUMN, DbType.String));
      MakeColumnJson (TableName.REASON_MACHINE_ASSOCIATION, REASON_DATA_COLUMN);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_MACHINE_ASSOCIATION, REASON_DATA_COLUMN);
    }
  }
}
