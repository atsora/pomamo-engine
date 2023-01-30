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
  /// Migration 533: add a new index on table reasonmachineassociation
  /// </summary>
  [Migration (533)]
  public class ReasonMachineAssociationIndexOnDateTimes: MigrationExt
  {
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID);
      AddIndex (TableName.REASON_MACHINE_ASSOCIATION,
        new string[] { ColumnName.MACHINE_ID, "reasonmachineassociationend", "reasonmachineassociationbegin" });
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.REASON_MACHINE_ASSOCIATION,
        new string[] { ColumnName.MACHINE_ID, "reasonmachineassociationend", "reasonmachineassociationbegin" });
      AddIndex (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID);
    }
  }
}
