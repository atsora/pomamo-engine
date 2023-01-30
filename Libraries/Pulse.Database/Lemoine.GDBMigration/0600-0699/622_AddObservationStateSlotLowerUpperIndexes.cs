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
  /// Migration 622: 
  /// </summary>
  [Migration (622)]
  public class AddObservationStateSlotLowerUpperIndexes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddObservationStateSlotLowerUpperIndexes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndex ("observationstateslot_lower", TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID, "lower(observationstateslotdatetimerange)");
      AddNamedIndex ("observationstateslot_upper", TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID, "upper(observationstateslotdatetimerange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("observationstateslot_lower", TableName.OBSERVATION_STATE_SLOT);
      RemoveNamedIndex ("observationstateslot_upper", TableName.OBSERVATION_STATE_SLOT);
    }
  }
}
