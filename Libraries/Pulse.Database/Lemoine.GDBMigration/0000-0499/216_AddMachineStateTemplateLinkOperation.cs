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
  /// Migration 216:
  /// </summary>
  [Migration(216)]
  public class AddMachineStateTemplateLinkOperation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateLinkOperation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (TableName.MACHINE_STATE_TEMPLATE + "linkoperation", DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             TableName.MACHINE_STATE_TEMPLATE + "linkoperation");
    }
  }
}
