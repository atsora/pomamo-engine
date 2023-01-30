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
  /// Migration 637: add a column nextanalysisstatusid in tables machinemodificationstatus and globalmodificationstatus 
  /// </summary>
  [Migration (637)]
  public class AddNextAnalysisStatusId : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddNextAnalysisStatusId).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODIFICATION_STATUS,
                          new Column ("Next" + TableName.ANALYSIS_STATUS + "id", DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey ("fk_machinemodificationstatus_nextanalysisstatus",
        TableName.MACHINE_MODIFICATION_STATUS, "Next" + TableName.ANALYSIS_STATUS + "id",
        TableName.ANALYSIS_STATUS, TableName.ANALYSIS_STATUS + "id",
        Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.AddColumn (TableName.GLOBAL_MODIFICATION_STATUS,
                          new Column ("Next" + TableName.ANALYSIS_STATUS + "id", DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey ("fk_globalmodificationstatus_nextanalysisstatus",
        TableName.GLOBAL_MODIFICATION_STATUS, "Next" + TableName.ANALYSIS_STATUS + "id",
        TableName.ANALYSIS_STATUS, TableName.ANALYSIS_STATUS + "id",
        Migrator.Framework.ForeignKeyConstraint.Restrict);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODIFICATION_STATUS, "Next" + TableName.ANALYSIS_STATUS + "id");
      Database.RemoveColumn (TableName.GLOBAL_MODIFICATION_STATUS, "Next" + TableName.ANALYSIS_STATUS + "id");
    }
  }
}
