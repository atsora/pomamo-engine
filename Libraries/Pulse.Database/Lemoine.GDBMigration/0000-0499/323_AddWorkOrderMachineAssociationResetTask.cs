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
  /// Migration 323:
  /// </summary>
  [Migration(323)]
  public class AddWorkOrderMachineAssociationResetTask: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddWorkOrderMachineAssociationResetTask).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                          new Column (TableName.WORKORDER_MACHINE_ASSOCIATION + "resettask", DbType.Boolean));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                             TableName.WORKORDER_MACHINE_ASSOCIATION + "resettask");
    }
  }
}
