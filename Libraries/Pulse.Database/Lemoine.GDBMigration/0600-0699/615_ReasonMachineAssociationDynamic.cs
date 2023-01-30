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
  /// Migration 615: add a column reasonmachineassociationdynamicend
  /// </summary>
  [Migration (615)]
  public class ReasonMachineAssociationDynamic : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SequenceToolNumberOldNumber).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION,
        new Column (TableName.REASON_MACHINE_ASSOCIATION + "dynamic", DbType.String));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_MACHINE_ASSOCIATION,
        TableName.REASON_MACHINE_ASSOCIATION + "dynamic");
    }
  }
}
