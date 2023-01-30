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
  /// Migration 300: add an option column to the table machineobservationstateassociation and machinestatetemplateassociation
  /// </summary>
  [Migration(300)]
  public class AddMachineObservationStateTemplateOption: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineObservationStateTemplateOption).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                          new Column (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION + "option", DbType.Int32));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                          new Column (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "option", DbType.Int32));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                             TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION + "option");
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                             TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "option");
    }
  }
}
