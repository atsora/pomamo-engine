// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 337: fix a remaining index in parentindex that should have been removed before
  /// </summary>
  [Migration(337)]
  public class FixIndexObservationStateSlot: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixIndexObservationStateSlot).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveConstraint (TableName.OBSERVATION_STATE_SLOT,
                        BuildName (TableName.OBSERVATION_STATE_SLOT, "idx",
                                   ColumnName.MACHINE_ID,
                                   TableName.OBSERVATION_STATE_SLOT + "begindatetime"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
