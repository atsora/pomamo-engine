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
  /// Migration 630: add operationcycleend to operationcycledatetime index
  /// </summary>
  [Migration (630)]
  public class AddOperationCycleDateTimeEndIndex : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddOperationCycleDateTimeEndIndex).FullName);

    static readonly string OLD_INDEX_NAME = TableName.OPERATION_CYCLE + "_datetime";
    static readonly string NEW_INDEX_NAME = TableName.OPERATION_CYCLE + "_datetime_end";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveNamedIndex (OLD_INDEX_NAME, TableName.OPERATION_CYCLE);

      AddNamedIndex (NEW_INDEX_NAME,
                     TableName.OPERATION_CYCLE,
                     ColumnName.MACHINE_ID,
                     "operationcycledatetime(operationcyclebegin, operationcycleend, operationcyclestatus)",
                     TableName.OPERATION_CYCLE + "end");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex (NEW_INDEX_NAME, TableName.OPERATION_CYCLE);

      AddNamedIndex (OLD_INDEX_NAME,
                     TableName.OPERATION_CYCLE,
                     ColumnName.MACHINE_ID,
                     "operationcycledatetime(operationcyclebegin, operationcycleend, operationcyclestatus)");
    }
  }
}
