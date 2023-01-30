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
  /// Migration 283: add a production column to observationstateslot
  /// </summary>
  [Migration(283)]
  public class AddObservationStateSlotProduction: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddObservationStateSlotProduction).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "production", DbType.Boolean));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT,
                             TableName.OBSERVATION_STATE_SLOT + "production");
    }
  }
}
