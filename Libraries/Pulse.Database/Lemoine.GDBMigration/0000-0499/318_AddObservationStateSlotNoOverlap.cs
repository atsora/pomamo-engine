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
  /// Migration 318: add a nooverlap constraint in observation state slot,
  /// but only if the version of PostgreSQL is >= 9.5
  /// </summary>
  [Migration(318)]
  public class AddObservationStateSlotNoOverlap: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddObservationStateSlotNoOverlap).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CheckPostgresqlVersion ();
      
      AddNoOverlapConstraint (TableName.OBSERVATION_STATE_SLOT,
                              TableName.OBSERVATION_STATE_SLOT + "datetimerange",
                              ColumnName.MACHINE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNoOverlapConstraint (TableName.OBSERVATION_STATE_SLOT);
    }
  }
}
