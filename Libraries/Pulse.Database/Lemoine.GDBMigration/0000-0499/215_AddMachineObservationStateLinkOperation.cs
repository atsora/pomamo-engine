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
  /// Migration 215:
  /// </summary>
  [Migration(215)]
  public class AddMachineObservationStateLinkOperation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineObservationStateLinkOperation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE,
                          new Column (TableName.MACHINE_OBSERVATION_STATE + "linkoperation", DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE,
                             TableName.MACHINE_OBSERVATION_STATE + "linkoperation");
    }
  }
}
