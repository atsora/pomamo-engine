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
  /// Migration 205: add an index to find the observation state slots without machine observation state
  /// </summary>
  [Migration(205)]
  public class AddIndexObservationStateSlot: MigrationExt
  {
    static readonly string INDEX_NAME = TableName.OBSERVATION_STATE_SLOT + "_nomos";
    
    static readonly ILog log = LogManager.GetLogger(typeof (AddIndexObservationStateSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndexCondition (INDEX_NAME,
                              TableName.OBSERVATION_STATE_SLOT,
                              "machineobservationstateid IS NULL",
                              ColumnName.MACHINE_ID,
                              TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                              TableName.OBSERVATION_STATE_SLOT + "begindatetime");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex (INDEX_NAME, TableName.OBSERVATION_STATE_SLOT);
    }
  }
}
