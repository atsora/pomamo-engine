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
  /// Migration 618: rename reasonmachineassociationdynamicend into reasonmachineassociationdynamic 
  /// </summary>
  [Migration (618)]
  public class ReasonMachineAssociationDynamicRename : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonMachineAssociationDynamicRename).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
  {
      if (Database.ColumnExists (TableName.REASON_MACHINE_ASSOCIATION,
        TableName.REASON_MACHINE_ASSOCIATION + "dynamicend")) {
        Database.RenameColumn (TableName.REASON_MACHINE_ASSOCIATION,
          TableName.REASON_MACHINE_ASSOCIATION + "dynamicend",
          TableName.REASON_MACHINE_ASSOCIATION + "dynamic");
      }
  }

  /// <summary>
  /// Downgrade the database
  /// </summary>
  override public void Down ()
  {
  }
}
}
